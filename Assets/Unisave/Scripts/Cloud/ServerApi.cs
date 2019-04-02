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

		public ServerApi(string apiUrl, string gameToken)
		{
			if (apiUrl == null)
				throw new ArgumentNullException("apiUrl");

			if (gameToken == null)
				throw new ArgumentNullException("gameToken");

			this.apiUrl = apiUrl;
			this.gameToken = gameToken;

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

		public class LoginResult
		{
			public LoginResultType type;
			public string message;
			public string accessToken;
			public JsonObject playerData;
		}

		public enum LoginResultType
		{
			OK,
			NetworkError,
			InvalidGameToken,
			InvalidCredentials,
			PlayerBanned,
			ServerUnderMaintenance,
			GameClientOutdated,
			OtherError
		}

		/// <summary>
		/// Sends the login request to the server
		/// </summary>
		/// <param name="callback">Called after the routine finishes</param>
		/// <param name="email">Player email</param>
		/// <param name="password">Player password</param>
		public IEnumerator Login(Action<LoginResult> callback, string email, string password)
		{
			Dictionary<string, string> fields = new Dictionary<string, string>() {
				{"email", email},
				{"password", password},
				{"gameToken", gameToken}
			};

			UnityWebRequest request = UnityWebRequest.Post(Url("login"), fields);
			
			yield return request.SendWebRequest();

			if (request.isNetworkError)
			{
				callback.Invoke(new LoginResult {
					type = LoginResultType.NetworkError,
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
				callback.Invoke(new LoginResult {
					type = LoginResultType.OtherError,
					message = "Server response had invalid format."
				});
				yield break;
			}

			switch (code)
			{
				case "ok":
					callback.Invoke(new LoginResult {
						type = LoginResultType.OK,
						accessToken = response["accessToken"].AsString,
						playerData = response["playerData"].AsJsonObject,
					});
					break;

				case "invalid-credentials":
					callback.Invoke(new LoginResult {
						type = LoginResultType.InvalidCredentials,
						message = response["message"].AsString
					});
					break;

				case "maintenance-mode":
					callback.Invoke(new LoginResult {
						type = LoginResultType.ServerUnderMaintenance,
						message = response["message"].AsString
					});
					break;

				case "invalid-game-token":
					callback.Invoke(new LoginResult {
						type = LoginResultType.InvalidGameToken,
						message = response["message"].AsString
					});
					break;

				case "client-outdated":
					callback.Invoke(new LoginResult {
						type = LoginResultType.GameClientOutdated,
						message = response["message"].AsString
					});
					break;

				default:
					callback.Invoke(new LoginResult {
						type = LoginResultType.OtherError,
						message = "Server response had invalid format."
					});
					break;
			}
		}
	}
}
