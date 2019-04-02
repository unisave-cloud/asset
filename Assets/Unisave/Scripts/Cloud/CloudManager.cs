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
		private const string LOCAL_DEBUG_PLAYER_ACCESS_TOKEN = "local debug player access token";

		/// <summary>
		/// Key in PlayerPrefs for the local debug player storage
		/// </summary>
		public const string LOCAL_DEBUG_PLAYER_PREFS_KEY = "unisave.localDebugPlayer";

		/// <summary>
		/// Name of the preferences asset inside a Resources folder (without extension)
		/// </summary>
		private const string PREFERENCES_RESOURCE_NAME = "UnisavePreferencesInstance";

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
		/// API for server communication
		/// </summary>
		private IServerApi api;

		private bool loginCoroutineRunning = false;
		private bool savingCoroutineRunning = false;
		private bool logoutCoroutineRunning = false;

		/// <summary>
		/// Holds all the player-related cloud data in a key->json manner
		/// (this is the ground truth for the data, when logged in)
		/// (this is also called the "cache")
		/// 
		/// Why not deserialized json? Because I don't know the
		/// type into which to deserialize it.
		/// </summary>
		private Dictionary<string, JsonValue> playerData;

		/// <summary>
		/// List of MonoBehaviours to which distribute the data after login
		/// </summary>
		private List<WeakReference> distributeAfterLogin = new List<WeakReference>();

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
		/// A DontDestroyOnLoad component, that performs continual saving
		/// </summary>
		private SaverComponent saver;

		/// <summary>
		/// A DontDestroyOnLoad component, that runs all unisave coroutines
		/// (spoiler: it's the saver component)
		/// </summary>
		private MonoBehaviour coroutineRunner
		{
			get
			{
				return saver;
			}
		}

		private readonly string localDebugPlayerEmail;

		public static CloudManager CreateDefaultInstance()
		{
			var preferences = Resources.Load<UnisavePreferences>(PREFERENCES_RESOURCE_NAME);

			if (preferences == null)
			{
				preferences = ScriptableObject.CreateInstance<UnisavePreferences>();
				Debug.LogWarning("Unisave preferences not found. Server connection will not work.");
			}

			//return new CloudManager(preferences);
			return null;
		}

		public CloudManager(IServerApi api, IDataRepository repository)
		{
			this.api = api;
			this.repository = repository;
			distributor = new Distributor(this.repository);

			GameObject go = new GameObject("UnisaveSaver");
			saver = go.AddComponent<SaverComponent>();
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
			
			api.Login(result => {

				loginCoroutineRunning = false;

				if (result.type == ServerApi.LoginResultType.OK)
				{
					AccessToken = result.accessToken;
					PlayerEmail = email;
					DataRepositoryHelper.Clear(repository);
					DataRepositoryHelper.InsertJsonObject(repository, result.playerData);

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

			JsonObject json = JsonReader.Parse(
				PlayerPrefs.GetString(LOCAL_DEBUG_PLAYER_PREFS_KEY, "{}")
			);
			DataRepositoryHelper.Clear(repository);
			DataRepositoryHelper.InsertJsonObject(repository, json);
			AccessToken = LOCAL_DEBUG_PLAYER_ACCESS_TOKEN;
			PlayerEmail = localDebugPlayerEmail;

			Debug.LogWarning("Unisave: Local debug player has been logged in.");
		}

		/// <summary>
		/// Registers the behaviour to be loaded after login succeeds
		/// Or loads it now, if user already logged in
		/// </summary>
		public void LoadAfterLogin(MonoBehaviour behaviour)
		{
			if (LoggedIn)
				Load(behaviour);
			else
				distributeAfterLogin.Add(new WeakReference(behaviour));
		}

		/// <summary>
		/// Distributes cloud data from cache to a given behavior instance
		/// Player needs to be logged in.false If not, local debug player is logged in if in editor
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
				return;
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

			distributor.Collect();

			if (Application.isEditor && PlayerEmail == localDebugPlayerEmail)
			{
				SaveLocalDebugPlayer();
				return true;
			}

			savingCoroutineRunning = true;

			IEnumerator coroutine = api.Save(result => {

				savingCoroutineRunning = false;

				// TODO: HANDLE RESULT

			}, AccessToken, DataRepositoryHelper.ToJsonObject(repository));

			coroutineRunner.StartCoroutine(coroutine);

			return true;
		}

		private void SaveLocalDebugPlayer()
		{
			PlayerPrefs.SetString(
				LOCAL_DEBUG_PLAYER_PREFS_KEY,
				DataRepositoryHelper.ToJsonObject(repository).ToString()
			);
			PlayerPrefs.Save();
		}

		/// <summary>
		/// Starts the logout coroutine or does nothing if already logged out
		/// </summary>
		public bool Logout()
		{
			if (!LoggedIn)
				return false;

			if (logoutCoroutineRunning)
				return false;

			distributor.Collect();

			if (AccessToken == LOCAL_DEBUG_PLAYER_ACCESS_TOKEN)
			{
				LogoutLocalDebugPlayer();
				return true;
			}

			coroutineRunner.StartCoroutine(LogoutCoroutine());

			return true;
		}

		/// <summary>
		/// Logout coroutine that makes the final save and the logout HTTP request
		/// </summary>
		private IEnumerator LogoutCoroutine()
		{
			logoutCoroutineRunning = true;

			yield return null;

			logoutCoroutineRunning = false;
		}

		/// <summary>
		/// Logs the local debug player out
		/// </summary>
		private void LogoutLocalDebugPlayer()
		{
			SaveLocalDebugPlayer();

			AccessToken = null;
			PlayerEmail = null;
		}
	}
}
