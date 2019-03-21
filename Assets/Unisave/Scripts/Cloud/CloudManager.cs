#define DUMP_TRAFFIC_TO_PTSV

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Unisave.Serialization;

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
		/// Name of the preferences asset inside a Resources folder (without extension)
		/// </summary>
		private const string PREFERENCES_RESOURCE_NAME = "UnisavePreferencesInstance";

		/// <summary>
		/// Preferences that contain crutial login information
		/// </summary>
		private UnisavePreferences preferences;

		/// <summary>
		/// Keeps the access token that represents the player session
		/// Needed for further request authentication
		/// If null, no player is logged in
		/// </summary>
		private string accessToken = null;

		/// <summary>
		/// Email address of the logged in player
		/// </summary>
		private string playerEmail = null;

		/// <summary>
		/// If true, we have an authorized player session and we can make requests
		/// </summary>
		public bool LoggedIn
		{
			get
			{
				return accessToken != null;
			}
		}

		private bool loginCoroutineRunning = false;

		/// <summary>
		/// Holds all the player-related cloud data in a key->json manner
		/// (this is the ground truth for the data, when logged in)
		/// (this is also called the "cache")
		/// 
		/// Why not deserialized json? Because I don't know the
		/// type into which to deserialize it.
		/// </summary>
		private Dictionary<string, string> playerData;

		/// <summary>
		/// List of MonoBehaviours to which distribute the data after login
		/// </summary>
		private List<WeakReference> distributeAfterLogin = new List<WeakReference>();

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

		public CloudManager()
		{
			preferences = Resources.Load<UnisavePreferences>(PREFERENCES_RESOURCE_NAME);

			if (preferences == null)
			{
				preferences = new UnisavePreferences();
				Debug.LogWarning("Unisave preferences not found. Server connection will not work.");
			}

			GameObject go = new GameObject("UnisaveSaver");
			saver = go.AddComponent<SaverComponent>();
		}

		/// <summary>
		/// Starts the login coroutine
		/// </summary>
		/// <param name="callback">Calls methods here after coroutine finishes</param>
		/// <param name="email">Player email address</param>
		/// <param name="password">Player password</param>
		public void Login(ILoginCallback callback, string email, string password)
		{
			if (loginCoroutineRunning)
			{
				Debug.LogWarning("Trying to login while already logging in.");
				return;
			}

			if (LoggedIn)
			{
				Debug.LogWarning("Trying to login while already logged in.");
				return;
			}

			coroutineRunner.StartCoroutine(LoginCoroutine(callback, email, password));
		}

		/// <summary>
		/// The login coroutine that makes the HTTP request
		/// - sends login information
		/// - receives player data
		/// - triggers after-login distribution
		/// </summary>
		private IEnumerator LoginCoroutine(ILoginCallback callback, string email, string password)
		{
			loginCoroutineRunning = true;

			Dictionary<string, string> fields = new Dictionary<string, string>() {
				{"email", email},
				{"password", password},
				{"gameToken", preferences.gameToken}
			};

			UnityWebRequest request = UnityWebRequest.Post(GetApiUrl("login"), fields);
			
			yield return request.SendWebRequest();

			if (request.isNetworkError || request.isHttpError)
			{
				callback.LoginFailed(new LoginFailure() {
					type = LoginFailure.FailureType.ServerNotReachable,
					message = request.error
				});
			}
			else
			{
				LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);

				accessToken = response.accessToken;
				playerEmail = email;

				// TODO: figure out the HTTP API
				DeserializePlayerData(response.playerData);
				
				callback.LoginSucceeded();
			}

			loginCoroutineRunning = false;
		}

		private string GetApiUrl(string subPath)
		{
			#if DUMP_TRAFFIC_TO_PTSV
			return "http://ptsv2.com/t/unisave/post";
			#endif

			if (preferences.serverApiUrl == null || preferences.serverApiUrl.Length == 0)
				throw new UnisaveException("Unisave server API URL not set.");

			if (preferences.serverApiUrl.EndsWith("/"))
				return preferences.serverApiUrl + subPath;
			
			return preferences.serverApiUrl + "/" + subPath;
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

			// TODO: do the login
			//DeserializePlayerData(PlayerPrefs.GetString("", "{}"));
			accessToken = LOCAL_DEBUG_PLAYER_ACCESS_TOKEN;
			playerEmail = "local@debug.com";

			Debug.LogWarning("Local debug player has been logged in.");
		}

		/// <summary>
		/// Load player data from the downloaded json string
		/// </summary>
		private void DeserializePlayerData(string playerDataJson)
		{
			// TODO: deserialize this
		}

		/// <summary>
		/// Serialize player data into a json string for upload
		/// </summary>
		private string SerializePlayerData()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("{");
			foreach (KeyValuePair<string, string> pair in playerData)
			{
				sb.Append(Saver.Save(pair.Key));
				sb.Append(":");
				sb.Append(pair.Value);
				sb.Append(",");
			}

			return sb.ToString();
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
		/// </summary>
		public void Load(MonoBehaviour behaviour)
		{
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

			// foreach field and property (test if private can be accessed as well)
			// set the field value
			// keep field reference for data collection
		}

		/// <summary>
		/// Starts the logout coroutine or does nothing if already logged out
		/// </summary>
		public void Logout()
		{
			if (!LoggedIn)
				return;

			if (accessToken == LOCAL_DEBUG_PLAYER_ACCESS_TOKEN)
			{
				LogoutLocalDebugPlayer();
				return;
			}

			coroutineRunner.StartCoroutine(LogoutCoroutine());
		}

		/// <summary>
		/// Logout coroutine that makes the final save and the logout HTTP request
		/// </summary>
		private IEnumerator LogoutCoroutine()
		{
			Dictionary<string, string> fields = new Dictionary<string, string>() {
				{"accessToken", accessToken}
			};

			UnityWebRequest request = UnityWebRequest.Post(GetApiUrl("logout"), fields);

			accessToken = null;
			playerEmail = null;
			
			yield return request.SendWebRequest();

			if (request.isNetworkError || request.isHttpError)
			{
				Debug.Log(request.error);
			}
			else
			{
				Debug.Log("Request done!");
				Debug.Log(request.downloadHandler.text);
			}
		}

		/// <summary>
		/// Logs the local debug player out
		/// </summary>
		private void LogoutLocalDebugPlayer()
		{
			// TODO: save data
			accessToken = null;
			playerEmail = null;
		}
	}
}
