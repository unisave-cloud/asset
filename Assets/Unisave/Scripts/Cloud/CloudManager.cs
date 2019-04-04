using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using Unisave.Serialization;
using LightJson;
using LightJson.Serialization;

namespace Unisave
{
	/// <summary>
	/// Provides API for managing the cloud part of Unisave
	/// Most usual usage is through the static UnisaveCloud
	/// </summary>
	public class CloudManager
	{
		/// <summary>
		/// Fake access token for the local debug player to fake saving and logout
		/// </summary>
		private const string LocalDebugPlayerAccessToken = "<local-debug-player>";

		/// <summary>
		/// Key in PlayerPrefs for the local debug player storage
		/// </summary>
		public const string LocalDebugPlayerPrefsKey = "unisave.localDebugPlayer";

		/// <summary>
		/// Token for request authentication after login
		/// If null, no player is logged in
		/// </summary>
		public string AccessToken { get; private set; }

		/// <summary>
		/// If true, we have an authorized player session and we can make requests
		/// </summary>
		public bool LoggedIn
		{
			get
			{
				return AccessToken != null;
			}
		}

		/// <summary>
		/// Email address of the logged in player
		/// (private because the email address should not be used for identification;
		/// use player id instead, it's not that sensitive piece of information)
		/// </summary>
		private string PlayerEmail { get; set; }

		/// <summary>
		/// Information about the logged-in player
		/// Null if no player logged in
		/// </summary>
		public PlayerInformation PlayerInfo { get; private set; }

		/// <summary>
		/// API for server communication
		/// </summary>
		private IServerApi api;

		/// <summary>
		/// Caches player data between server api calls
		/// For distribution and collection
		/// </summary>
		private IDataRepository repository;

		/// <summary>
		/// Distributes data from cache into scripts
		/// </summary>
		private Distributor distributor;

		/// <summary>
		/// A DontDestroyOnLoad component, that runs all unisave coroutines
		/// </summary>
		private CoroutineRunnerComponent coroutineRunner;

		/// <summary>
		/// List of player scripts to which distribute the data after login
		/// </summary>
		private List<WeakReference> distributeAfterLogin = new List<WeakReference>();

		/// <summary>
		/// Email value to use for detecting the local debug player
		/// </summary>
		private readonly string localDebugPlayerEmail;

		private bool loginCoroutineRunning = false;
		private bool savingCoroutineRunning = false;
		private bool logoutCoroutineRunning = false;

		/// <summary>
		/// Creates the instance that is used via the UnisaveCloud facade
		/// </summary>
		public static CloudManager CreateDefaultInstance()
		{
			var preferences = UnisavePreferences.LoadPreferences();

			// make sure a continuous saver exists and is running
			SaverComponent.GetInstance();

			return new CloudManager(
				CoroutineRunnerComponent.GetInstance(),
				new ServerApi(
					preferences.serverApiUrl,
					preferences.gameToken,
					preferences.editorKey
				),
				new InMemoryDataRepository(),
				preferences.localDebugPlayerEmail
			);
		}

		public CloudManager(
			CoroutineRunnerComponent coroutineRunner,
			IServerApi api,
			IDataRepository repository,
			string localDebugPlayerEmail
		)
		{
			this.coroutineRunner = coroutineRunner;
			this.api = api;
			this.repository = repository;
			this.distributor = new Distributor(this.repository);
			this.localDebugPlayerEmail = localDebugPlayerEmail;
		}

		/// <summary>
		/// Starts the login coroutine
		/// </summary>
		/// <param name="callback">Calls methods here after coroutine finishes</param>
		/// <param name="email">Player email address</param>
		/// <param name="password">Player password</param>
		/// <returns>False if the login request was ignored for some reason</returns>
		public bool Login(ILoginCallback callback, string email, string password)
		{
			Action success = null;
			Action<LoginFailure> failure = null;
			
			if (callback != null)
			{
				success = callback.LoginSucceeded;
				failure = callback.LoginFailed;
			}

			return Login(success, failure, email, password);
		}

		public bool Login(Action success, Action<LoginFailure> failure, string email, string password)
		{
			if (loginCoroutineRunning)
			{
				Debug.LogWarning("Unisave: Trying to login while already logging in.");
				return false;
			}

			if (LoggedIn)
			{
				Debug.LogWarning("Unisave: Trying to login while already logged in.");
				return false;
			}

			if (Application.isEditor && email == localDebugPlayerEmail)
			{
				LoginLocalDebugPlayer();
				
				if (success != null)
					success.Invoke();

				return true;
			}

			loginCoroutineRunning = true;
			
			IEnumerator coroutine = api.Login(result => {

				loginCoroutineRunning = false;

				if (result.type == ServerApi.LoginResultType.OK)
				{
					AccessToken = result.accessToken;
					PlayerEmail = email;
					PlayerInfo = result.playerInfo;

					DataRepositoryHelper.Clear(repository);
					DataRepositoryHelper.InsertJsonObject(repository, result.playerData);

					PerformAfterLoginDistribution();

					if (success != null)
						success.Invoke();
				}
				else
				{
					if (failure != null)
						failure.Invoke(new LoginFailure {
							type = LoginFailure.TypeFromApiResultType(result.type),
							message = result.message
						});
				}
				
			}, email, password);

			coroutineRunner.StartCoroutine(coroutine);

			return true;
		}

		/// <summary>
		/// Logs in a fake local player that is useful during development of login-only scenes
		/// </summary>
		private void LoginLocalDebugPlayer()
		{
			if (!Application.isEditor)
				throw new UnisaveException("Cannot login local debug player on build platform.");

			if (LoggedIn)
				throw new UnisaveException("Trying to login local debug player, but someone is already logged in.");

			AccessToken = LocalDebugPlayerAccessToken;
			PlayerEmail = localDebugPlayerEmail;
			PlayerInfo = new PlayerInformation("<local-debug-player-id>");

			JsonObject json = JsonReader.Parse(
				PlayerPrefs.GetString(LocalDebugPlayerPrefsKey, "{}")
			);
			DataRepositoryHelper.Clear(repository);
			DataRepositoryHelper.InsertJsonObject(repository, json);
			
			PerformAfterLoginDistribution();

			Debug.LogWarning("Unisave: Local debug player has been logged in.");
		}

		/// <summary>
		/// Registers the behaviour to be loaded after login succeeds
		/// Or loads it now, if user already logged in
		/// </summary>
		public void LoadAfterLogin(object target)
		{
			if (LoggedIn)
				Load(target);
			else
				distributeAfterLogin.Add(new WeakReference(target));
		}

		private void PerformAfterLoginDistribution()
		{
			foreach (WeakReference reference in distributeAfterLogin)
			{
				object target = reference.Target;

				if (target != null)
					distributor.Distribute(target);
			}

			distributeAfterLogin.Clear();
		}

		/// <summary>
		/// Distributes cloud data from cache to a given script instance
		/// Player needs to be logged in. Local debug player is logged in if in editor
		/// </summary>
		public void Load(object target)
		{
			if (target == null)
				throw new ArgumentNullException("target");

			/*
				When you program a scene, that expects a player to be logged in,
				and you start it from the editor, we don't want to kill the game.
				Instead we would like to debug the scene. So here we log in a fake
				local user. It's data is stored locally, no internet connection needed.
			 */
			if (!LoggedIn)
			{
				// throws exception on build target
				LoginLocalDebugPlayer();
			}

			distributor.Distribute(target);
		}

		/// <summary>
		/// Saves all changes to the server by starting a saving coroutine
		/// <returns>False if the save request was ignored for some reason</returns>
		/// </summary>
		public bool Save()
		{
			if (!LoggedIn)
			{
				Debug.LogWarning("Unisave: Cannot save data while not beign logged in.");
				return false;
			}

			if (savingCoroutineRunning)
			{
				Debug.LogWarning("Unisave: Save called while already saving. Ignoring.");
				return false;
			}

			if (loginCoroutineRunning || logoutCoroutineRunning)
			{
				Debug.LogWarning("Unisave: Cannot save, other coroutine already running. Ignoring.");
				return false;
			}

			distributor.Collect();

			if (Application.isEditor && AccessToken == LocalDebugPlayerAccessToken)
			{
				SaveLocalDebugPlayer();
				return true;
			}

			savingCoroutineRunning = true;

			IEnumerator coroutine = api.Save(result => {

				savingCoroutineRunning = false;

				switch (result.type)
				{
					case ServerApi.SaveResultType.OK:
						// nothing, it's all fine
						break;

					case ServerApi.SaveResultType.NotLoggedIn:
						HandleUnexpectedLogout(true);
						break;

					default:
						Debug.LogWarning("Unisave: Saving error: " + result.message);
						break;
				}

			}, AccessToken, DataRepositoryHelper.ToJsonObject(repository));

			coroutineRunner.StartCoroutine(coroutine);

			return true;
		}

		private void SaveLocalDebugPlayer()
		{
			PlayerPrefs.SetString(
				LocalDebugPlayerPrefsKey,
				DataRepositoryHelper.ToJsonObject(repository).ToString()
			);
			PlayerPrefs.Save();
		}

		/// <summary>
		/// Called when a request is made, but the server says we are not logged in anymore.
		/// That might be due to session expiration, server-side logout or some error.
		/// </summary>
		private void HandleUnexpectedLogout(bool callUserCallbacks)
		{
			Debug.LogWarning("Unisave: Unexpected logout occured!");

			AccessToken = null;
			PlayerEmail = null;

			// if (callUserCallbacks)
			// TODO: call user-defined handlers to actually "log the player out"
		}

		/// <summary>
		/// Starts the logout coroutine or does nothing if already logged out
		/// <returns>False if the logout request was ignored for some reason</returns>
		/// </summary>
		public bool Logout()
		{
			if (!LoggedIn)
				return false;

			if (logoutCoroutineRunning)
				return false;

			distributor.Collect();

			if (Application.isEditor && AccessToken == LocalDebugPlayerAccessToken)
			{
				LogoutLocalDebugPlayer();
				return true;
			}

			logoutCoroutineRunning = true;

			IEnumerator coroutine = api.Logout(result => {

				logoutCoroutineRunning = false;

				switch (result.type)
				{
					case ServerApi.LogoutResultType.OK:
						AccessToken = null;
						PlayerEmail = null;
						PlayerInfo = null;
						DataRepositoryHelper.Clear(repository);
						break;

					case ServerApi.LogoutResultType.NotLoggedIn:
						HandleUnexpectedLogout(false);
						break;

					default:
						Debug.LogWarning("Unisave: Saving error: " + result.message);
						break;
				}

			}, AccessToken, DataRepositoryHelper.ToJsonObject(repository));

			coroutineRunner.StartCoroutine(coroutine);

			return true;
		}

		/// <summary>
		/// Logs the local debug player out
		/// </summary>
		private void LogoutLocalDebugPlayer()
		{
			SaveLocalDebugPlayer();

			AccessToken = null;
			PlayerEmail = null;
			PlayerInfo = null;
			
			DataRepositoryHelper.Clear(repository);
		}
	}
}
