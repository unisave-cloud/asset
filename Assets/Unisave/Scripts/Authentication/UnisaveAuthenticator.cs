using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RSG;
using Unisave.Utils;
using Unisave.Serialization;
using Unisave.Exceptions;
using Unisave.Exceptions.ServerConnection;
using Unisave.Exceptions.PlayerRegistration;
using LightJson;

namespace Unisave.Authentication
{
    /// <summary>
    /// Handles player authentication against unisave servers
    /// </summary>
    public class UnisaveAuthenticator : IAuthenticator
    {
        /// <summary>
        /// Authenticated player
        /// </summary>
        public UnisavePlayer Player { get; private set; }

        /// <summary>
        /// Is some player logged in
        /// </summary>
        public bool LoggedIn => Player != null;

        /// <summary>
        /// Token that is used to access the server when authenticated
        /// </summary>
        public string AccessToken { get; private set; }

        // flags showing which coroutines are currently running
        private bool loginCoroutineRunning = false;
		private bool logoutCoroutineRunning = false;
		private bool registrationCoroutineRunning = false;

		private ApiUrl apiUrl;
		private string gameToken;
		private string editorKey;

        public UnisaveAuthenticator(ApiUrl apiUrl, string gameToken, string editorKey)
		{
			this.apiUrl = apiUrl;
			this.gameToken = gameToken;
			this.editorKey = editorKey;
		}

		/// <inheritdoc/>
        public IPromise Login(string email, string password)
		{
			if (loginCoroutineRunning)
			{
				Debug.LogWarning("Unisave: Trying to login while already logging in.");
				return new Promise(); // won't resolve nor reject
			}

			if (Player != null)
			{
				return Promise.Rejected(new LoginFailure {
					type = LoginFailureType.AlreadyLoggedIn,
					message = "Trying to login while already logged in."
				});
			}

			loginCoroutineRunning = true;
			
			var promise = new Promise();

			Http.Post(
				apiUrl.Login(),
				new JsonObject()
					.Add("email", email)
					.Add("password", password)
					.Add("game_token", gameToken)
					.Add("client", new JsonObject()
						.Add("backend_hash", UnisavePreferences.LoadOrCreate().BackendHash)
						.Add("framework_version", FrameworkMeta.Version)
						.Add("asset_version", AssetMeta.Version)
						.Add("build_guid", Application.buildGUID)
						.Add("version_string", Application.version)
					)
			).Then(response => {
				switch (response["code"].AsString)
				{
					case "ok":
						AccessToken = response["access_token"].AsString;
						Player = new UnisavePlayer(response["player"].AsJsonObject["id"]);
						promise.Resolve();
						break;

					case "invalid-credentials":
						promise.Reject(new LoginFailure {
							type = LoginFailureType.InvalidCredentials,
							message = response["message"].AsString
						});
						break;

					case "maintenance-mode":
						promise.Reject(new LoginFailure {
							type = LoginFailureType.ServerUnderMaintenance,
							message = response["message"].AsString
						});
						break;

					case "invalid-game-token":
						promise.Reject(new LoginFailure {
							type = LoginFailureType.InvalidGameToken,
							message = response["message"].AsString
						});
						break;

					case "client-outdated":
						promise.Reject(new LoginFailure {
							type = LoginFailureType.GameClientOutdated,
							message = response["message"].AsString
						});
						break;
					
					case "unknown-client":
						// hack, make it look like outdated client,
						// but with a different message
						promise.Reject(new LoginFailure {
							type = LoginFailureType.GameClientOutdated,
							message = response["message"].AsString
						});
						break;

					default:
						promise.Reject(new LoginFailure {
							type = LoginFailureType.OtherError,
							message = "Server response had invalid format."
						});
						break;
				}
			})
			.Catch(e => {
				if (e.GetType() != typeof(HttpException))
				{
					promise.Reject(e);
					return;
				}

				HttpException he = (HttpException)e;

				switch (he.Type)
				{
					case HttpException.ExceptionType.NetworkError:
						promise.Reject(new LoginFailure {
							type = LoginFailureType.NetworkError,
							message = he.Response.ErrorMessage
						});
						break;

					case HttpException.ExceptionType.JsonParseException:
						promise.Reject(new LoginFailure {
							type = LoginFailureType.OtherError,
							message = "Server response had invalid format."
						});
						break;
				}
			})
			.Finally(() => {
				loginCoroutineRunning = false;
			});

			return promise;
		}

		/// <inheritdoc/>
		public IPromise Logout()
		{
			if (logoutCoroutineRunning)
			{
				Debug.LogWarning("Unisave: Trying to logout while already logging out.");
				return new Promise(); // won't resolve nor reject
			}

			if (Player == null)
			{
				return Promise.Rejected(new LogoutFailure {
					type = LogoutFailureType.OtherError,
					message = "No player is logged in, cannot log out."
				});
			}

			logoutCoroutineRunning = true;
			
			var promise = new Promise();

			Http.Post(
				apiUrl.Logout(),
				new JsonObject()
					.Add("accessToken", AccessToken)
			).Then((JsonValue jsonValue) => {
				JsonObject response = jsonValue;
				switch (response["code"].AsString)
				{
					case "ok":
						Player = null;
						AccessToken = null;
						promise.Resolve();
						break;

					default:
						promise.Reject(new LogoutFailure {
							type = LogoutFailureType.OtherError,
							message = "Server response had invalid format."
						});
						break;
				}
			})
			.Catch(e => {
				if (e.GetType() != typeof(HttpException))
				{
					promise.Reject(e);
					return;
				}

				HttpException he = (HttpException)e;

				switch (he.Type)
				{
					case HttpException.ExceptionType.NetworkError:
						promise.Reject(new LogoutFailure {
							type = LogoutFailureType.NetworkError,
							message = he.Response.ErrorMessage
						});
						break;

					case HttpException.ExceptionType.JsonParseException:
						promise.Reject(new LogoutFailure {
							type = LogoutFailureType.OtherError,
							message = "Server response had invalid format."
						});
						break;
				}
			})
			.Finally(() => {
				logoutCoroutineRunning = false;
			});

			return promise;
		}

		/// <inheritdoc/>
		public IPromise Register(string email, string password, Dictionary<string, object> hookArguments)
		{
			if (registrationCoroutineRunning)
			{
				Debug.LogWarning("Unisave: Trying to register while already registering.");
				return new Promise(); // won't resolve nor reject
			}

			registrationCoroutineRunning = true;
			
			var promise = new Promise();

			Http.Post(
				apiUrl.Register(),
				new JsonObject()
					.Add("email", email)
					.Add("password", password)
					.Add("hook_arguments", Serializer.ToJson(hookArguments))
					.Add("game_token", gameToken)
					.Add("client", new JsonObject()
						.Add("backend_hash", UnisavePreferences.LoadOrCreate().BackendHash)
						.Add("framework_version", FrameworkMeta.Version)
						.Add("asset_version", AssetMeta.Version)
						.Add("build_guid", Application.buildGUID)
						.Add("version_string", Application.version)
					)
			).Then((JsonValue jsonValue) => {
				JsonObject response = jsonValue;
				switch (response["code"].AsString)
				{
					case "ok":
						promise.Resolve();
						break;

					case "email-already-registered":
						promise.Reject(
							new EmailAlreadyRegisteredException()
								.TellPlayer(response["message"].AsString)
						);
						break;

					case "maintenance-mode":
						promise.Reject(
							new ServerUnderMaintenanceException()
								.TellPlayer(response["message"].AsString)
						);
						break;

					case "invalid-game-token":
						promise.Reject(new InvalidGameTokenException());
						break;

					case "invalid-email":
						promise.Reject(
							new InvalidEmailException()
								.TellPlayer(response["message"].AsString)
						);
						break;

					case "invalid-password":
						promise.Reject(
							new InvalidPasswordException()
								.TellPlayer(response["message"].AsString)
						);
						break;

					case "hook-crashed":
						if (response["exception"].IsNull)
						{
							promise.Reject(new UnisaveException(
								response["message"].AsString + "\n" + response["exceptionAsString"].AsString
							));
						}
						else
						{
							promise.Reject(
								(Exception)Serializer.FromJson(response["exception"], typeof(Exception))
							);
						}
						break;

					default:
						promise.Reject(new UnisaveException(
							response.ContainsKey("message")
								? response["message"].AsString
								: "Server response had invalid format."
						));
						break;
				}
			})
			.Catch(e => {
				if (e.GetType() != typeof(HttpException))
				{
					promise.Reject(e);
					return;
				}

				HttpException he = (HttpException)e;

				promise.Reject(new ServerConnectionException(
					he.Response.ErrorMessage
				));
			})
			.Finally(() => {
				registrationCoroutineRunning = false;
			});

			return promise;
		}
    }
}
