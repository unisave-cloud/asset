#define DUMP_TRAFFIC_TO_PTSV

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Unisave
{
	/// <summary>
	/// Provides API for managing the cloud part of Unisave
	/// Most usual usage is through the static UnisaveCloud
	/// </summary>
	public class CloudManager
	{
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
		/// If true, we have an authorized player session and we can make requests
		/// </summary>
		public bool LoggedIn
		{
			get
			{
				return accessToken != null;
			}
		}

		/// <summary>
		/// Holds all the player-related cloud data in a key->json manner
		/// (this is the ground truth for the data, when logged in)
		/// (this is also called the "cache")
		/// </summary>
		private Dictionary<string, string> playerData;

		/// <summary>
		/// List of MonoBehaviours to which distribute the data after login
		/// </summary>
		private List<WeakReference> distributeOnLogin = new List<WeakReference>();

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
				
				callback.LoginSucceeded();
			}
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
		/// Distributes cloud data from cache to a given behavior instance,
		/// or registers the behaviour as to-be-distributed after login occurs
		/// </summary>
		public void Load(MonoBehaviour behaviour)
		{
			if (!LoggedIn)
			{
				distributeOnLogin.Add(new WeakReference(behaviour));
				return;
			}

			// foreach field and property (test if private can be accessed as well)
			// set the field value
			// keep field reference for data collection
		}

		/// <summary>
		/// Starts the logout coroutine
		/// </summary>
		public void Logout()
		{
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
	}
}
