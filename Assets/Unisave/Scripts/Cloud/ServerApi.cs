using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LightJson;
using LightJson.Serialization;

namespace Unisave
{
	/// <summary>
	/// Encapsulates communication with the unisave server
	/// It's stateless, it accepts only parameters in the constructor
	/// </summary>
	public class ServerApi
	{
		/// <summary>
		/// URL to root of the server api
		/// </summary>
		private string apiUrl;

		/// <summary>
		/// Token that identifies this game
		/// </summary>
		private string gameToken;

		/// <summary>
		/// Key that the editor uses to authenticate itself
		/// </summary>
		private string editorKey;

		public ServerApi(string apiUrl, string gameToken, string editorKey)
		{
			if (apiUrl == null)
				throw new ArgumentNullException("apiUrl");

			// let the server tell the developer about missing game token on login
			if (gameToken == null)
				gameToken = "";

			this.apiUrl = apiUrl;
			this.gameToken = gameToken;
			this.editorKey = editorKey;

			if (!this.apiUrl.EndsWith("/"))
				this.apiUrl += "/";
		}

		/// <summary>
		/// Constructs the URL for a given API endpoint
		/// </summary>
		/// <param name="tail">Url suffix relatiev to the api root url</param>
		private string Url(string tail)
		{
			return apiUrl + tail;
		}

		////////////
		// Logout //
		////////////
		
		public class LogoutResult
		{
			public LogoutResultType type;
			public string message;
		}

		public enum LogoutResultType
		{
			OK,
			NetworkError,
			NotLoggedIn,
			OtherError
		}

		public IEnumerator Logout(Action<LogoutResult> callback, string accessToken, JsonObject playerData)
		{
			string payload = new JsonObject()
				.Add("accessToken", accessToken)
				.Add("playerData", playerData)
				.ToString();

			// put because post does not work with json for some reason
			// https://forum.unity.com/threads/posting-json-through-unitywebrequest.476254/
			UnityWebRequest request = UnityWebRequest.Put(Url("logout"), payload);
			request.SetRequestHeader("Content-Type", "application/json");
			request.SetRequestHeader("Accept", "application/json");
			
			yield return request.SendWebRequest();

			if (request.isNetworkError)
			{
				callback.Invoke(new LogoutResult {
					type = LogoutResultType.NetworkError,
					message = request.error
				});
				yield break;
			}

			if (request.responseCode == 401)
			{
				callback.Invoke(new LogoutResult {
					type = LogoutResultType.NotLoggedIn,
					message = "Provided access token was not accepted."
				});
				yield break;
			}

			JsonObject response;
			string code;
			try
			{
				JsonValue responseValue = JsonReader.Parse(request.downloadHandler.text);
				
				if (!responseValue.IsJsonObject)
					throw new JsonParseException();
				
				response = responseValue.AsJsonObject;

				if (!response.ContainsKey("code"))
					throw new JsonParseException();

				code = response["code"].AsString;
			}
			catch (JsonParseException)
			{
				callback.Invoke(new LogoutResult {
					type = LogoutResultType.OtherError,
					message = "Server response had invalid format."
				});
				yield break;
			}

			switch (code)
			{
				case "ok":
					callback.Invoke(new LogoutResult {
						type = LogoutResultType.OK
					});
					break;

				default:
					callback.Invoke(new LogoutResult {
						type = LogoutResultType.OtherError,
						message = "Server response had invalid format."
					});
					break;
			}
		}

		//////////////
		// Register //
		//////////////
		
		public class RegistrationResult
		{
			public RegistrationResultType type;
			public string message;
		}

		public enum RegistrationResultType
		{
			OK,
			NetworkError,
			InvalidGameToken,
			EmailAlreadyRegistered,
			InvalidEmail,
			InvalidPassword,
			ServerUnderMaintenance,
			OtherError
		}

		public IEnumerator Register(Action<RegistrationResult> callback, string email, string password)
		{
			string payload = new JsonObject()
				.Add("email", email)
				.Add("password", password)
				.Add("gameToken", gameToken)
				.Add("buildGUID", Application.buildGUID)
				.Add("version", Application.version)
				.Add("editorKey", editorKey)
				.ToString();

			// put because post does not work with json for some reason
			// https://forum.unity.com/threads/posting-json-through-unitywebrequest.476254/
			UnityWebRequest request = UnityWebRequest.Put(Url("register"), payload);
			request.SetRequestHeader("Content-Type", "application/json");
			request.SetRequestHeader("Accept", "application/json");
			
			yield return request.SendWebRequest();

			if (request.isNetworkError)
			{
				callback.Invoke(new RegistrationResult {
					type = RegistrationResultType.NetworkError,
					message = request.error
				});
				yield break;
			}

			JsonObject response;
			string code;
			try
			{
				JsonValue responseValue = JsonReader.Parse(request.downloadHandler.text);
				
				if (!responseValue.IsJsonObject)
					throw new JsonParseException();
				
				response = responseValue.AsJsonObject;

				if (!response.ContainsKey("code"))
					throw new JsonParseException();

				code = response["code"].AsString;
			}
			catch (JsonParseException)
			{
				callback.Invoke(new RegistrationResult {
					type = RegistrationResultType.OtherError,
					message = "Server response had invalid format."
				});
				yield break;
			}

			switch (code)
			{
				case "ok":
					callback.Invoke(new RegistrationResult {
						type = RegistrationResultType.OK
					});
					break;

				case "email-already-registered":
					callback.Invoke(new RegistrationResult {
						type = RegistrationResultType.EmailAlreadyRegistered,
						message = response["message"].AsString
					});
					break;

				case "maintenance-mode":
					callback.Invoke(new RegistrationResult {
						type = RegistrationResultType.ServerUnderMaintenance,
						message = response["message"].AsString
					});
					break;

				case "invalid-game-token":
					callback.Invoke(new RegistrationResult {
						type = RegistrationResultType.InvalidGameToken,
						message = response["message"].AsString
					});
					break;

				case "invalid-email":
					callback.Invoke(new RegistrationResult {
						type = RegistrationResultType.InvalidEmail,
						message = response["message"].AsString
					});
					break;

				case "invalid-password":
					callback.Invoke(new RegistrationResult {
						type = RegistrationResultType.InvalidPassword,
						message = response["message"].AsString
					});
					break;

				default:
					callback.Invoke(new RegistrationResult {
						type = RegistrationResultType.OtherError,
						message = response.ContainsKey("message")
							? response["message"].AsString
							: "Server response had invalid format."
					});
					break;
			}
		}
	}
}
