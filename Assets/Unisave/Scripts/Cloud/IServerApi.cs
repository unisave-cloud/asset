using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightJson;

namespace Unisave
{
	/// <summary>
	/// Interface to the server api class used for mocking during testing
	/// </summary>
	public interface IServerApi
	{
		IEnumerator Login(Action<ServerApi.LoginResult> callback, string email, string password);
		IEnumerator Save(Action<ServerApi.SaveResult> callback, string accessToken, JsonObject playerData);
		IEnumerator Logout(Action<ServerApi.LogoutResult> callback, string accessToken, JsonObject playerData);
		IEnumerator Register(Action<ServerApi.RegistrationResult> callback, string email, string password);
	}
}
