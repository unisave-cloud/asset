#define DUMP_TRAFFIC_TO_PTSV

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
		public string PlayerEmail { get; private set; }

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
		/// List of places, where the values have been distributed and from which
		/// to collect values before saving
		/// </summary>
		private List<DistributedValue> distributedValues = new List<DistributedValue>();

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

		public static CloudManager CreateDefaultInstance()
		{
			var preferences = Resources.Load<UnisavePreferences>(PREFERENCES_RESOURCE_NAME);

			if (preferences == null)
			{
				preferences = ScriptableObject.CreateInstance<UnisavePreferences>();
				Debug.LogWarning("Unisave preferences not found. Server connection will not work.");
			}

			return new CloudManager(preferences);
		}

		public CloudManager(UnisavePreferences preferences)
		{
			this.preferences = preferences;

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
				Debug.LogWarning("Unisave: Trying to login while already logging in.");
				return;
			}

			if (LoggedIn)
			{
				Debug.LogWarning("Unisave: Trying to login while already logged in.");
				return;
			}

			if (Application.isEditor && email == preferences.localDebugPlayerEmail)
			{
				LoginLocalDebugPlayer();
				
				if (callback != null)
					callback.LoginSucceeded();

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
				if (callback != null)
					callback.LoginFailed(new LoginFailure() {
						type = LoginFailure.FailureType.ServerNotReachable,
						message = request.error
					});
			}
			else
			{
				JsonValue responseValue = JsonReader.Parse(request.downloadHandler.text);
				JsonObject response = responseValue.AsJsonObject;

				if (response == null)
				{
					if (callback != null)
						callback.LoginFailed(new LoginFailure() {
							type = LoginFailure.FailureType.ServerNotReachable,
							message = "Server responded strangely."
						});
				}
				else
				{
					accessToken = response["accessToken"];
					PlayerEmail = email;
					DeserializePlayerData(response["playerData"]);
					
					callback.LoginSucceeded();
				}
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

			DeserializePlayerData(
				JsonReader.Parse(
					PlayerPrefs.GetString(LOCAL_DEBUG_PLAYER_PREFS_KEY, "{}")
				)
			);
			accessToken = LOCAL_DEBUG_PLAYER_ACCESS_TOKEN;
			PlayerEmail = preferences.localDebugPlayerEmail;

			Debug.LogWarning("Unisave: Local debug player has been logged in.");
		}

		/// <summary>
		/// Load player data from the downloaded json string
		/// </summary>
		private void DeserializePlayerData(JsonValue json)
		{
			playerData = new Dictionary<string, JsonValue>();

			if (!json.IsJsonObject)
			{
				Debug.LogError("Player data for deserialization is not a JSON object: " + json.ToString());
				return;
			}

			JsonObject jsonObject = json.AsJsonObject;

			foreach (KeyValuePair<string, JsonValue> pair in jsonObject)
				playerData.Add(pair.Key, pair.Value);
		}

		/// <summary>
		/// Serialize player data into a json string for upload
		/// </summary>
		private string SerializePlayerData()
		{
			JsonObject jsonObject = new JsonObject();

			foreach (KeyValuePair<string, JsonValue> pair in playerData)
				jsonObject.Add(pair.Key, pair.Value);

			return jsonObject.ToString();
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
			if (behaviour == null)
				throw new ArgumentNullException("behaviour");

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

			Type type = behaviour.GetType();

			foreach (FieldInfo fi in type.GetFields())
			{
				if (fi.IsPublic && !fi.IsStatic)
				{
					object[] attrs = fi.GetCustomAttributes(typeof(SavedAsAttribute), false);
					if (attrs.Length > 0)
					{
						string key = ((SavedAsAttribute)attrs[0]).Key;
						if (playerData.ContainsKey(key))
						{
							fi.SetValue(behaviour, Loader.Load(playerData[key], fi.FieldType));

							distributedValues.Add(new DistributedValue() {
								behaviour = new WeakReference(behaviour),
								fieldInfo = fi,
								key = key
							});
						}
					}
				}
			}
		}

		/// <summary>
		/// Saves all changes to the server by starting a saving coroutine
		/// </summary>
		public void Save()
		{
			if (!LoggedIn)
			{
				Debug.LogWarning("Unisave: Cannot save data while not beign logged in.");
				return;
			}

			if (savingCoroutineRunning)
			{
				Debug.LogWarning("Unisave: Save called while already saving. Ignoring.");
				return;
			}

			if (Application.isEditor && PlayerEmail == preferences.localDebugPlayerEmail)
			{
				SaveLocalDebugPlayer();
				return;
			}

			coroutineRunner.StartCoroutine(SaveCoroutine());
		}

		private IEnumerator SaveCoroutine()
		{
			savingCoroutineRunning = true;

			CollectData();

			// TODO: save request
			yield return null;

			savingCoroutineRunning = false;
		}

		private void SaveLocalDebugPlayer()
		{
			CollectData();

			PlayerPrefs.SetString(LOCAL_DEBUG_PLAYER_PREFS_KEY, SerializePlayerData());
			PlayerPrefs.Save();
		}

		/// <summary>
		/// Collect all data distributed into mono behaviours back into the cache
		/// </summary>
		private void CollectData()
		{
			List<DistributedValue> itemsToRemove = new List<DistributedValue>();

			foreach (DistributedValue item in distributedValues)
			{
				MonoBehaviour behaviour = item.behaviour.Target as MonoBehaviour;

				if (behaviour == null)
				{
					itemsToRemove.Add(item);
					continue;
				}

				JsonValue newValue = Saver.Save(item.fieldInfo.GetValue(behaviour));
				playerData[item.key] = newValue;
			}

			foreach (DistributedValue item in itemsToRemove)
				distributedValues.Remove(item);
		}

		/// <summary>
		/// Starts the logout coroutine or does nothing if already logged out
		/// </summary>
		public void Logout()
		{
			if (!LoggedIn)
				return;

			if (logoutCoroutineRunning)
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
			logoutCoroutineRunning = true;

			CollectData();

			string payload = new JsonObject()
				.Add("accessToken", accessToken)
				.Add("playerData", SerializePlayerData())
				.ToString();

			UnityWebRequest request = UnityWebRequest.Post(GetApiUrl("logout"), payload);

			accessToken = null;
			PlayerEmail = null;
			
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

			logoutCoroutineRunning = false;
		}

		/// <summary>
		/// Logs the local debug player out
		/// </summary>
		private void LogoutLocalDebugPlayer()
		{
			SaveLocalDebugPlayer();

			accessToken = null;
			PlayerEmail = null;
		}
	}
}
