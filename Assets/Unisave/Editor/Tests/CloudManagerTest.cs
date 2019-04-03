using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using Unisave;
using LightJson;
using LightJson.Serialization;

public class CloudManagerTest
{
	private class FakeBehaviour : MonoBehaviour
	{
		[SavedAs("foo")]
		public string foo;
	}

	private class FakeServerApi : IServerApi
	{
		public ServerApi.LoginResult loginResult;
		public ServerApi.SaveResult saveResult;
		public ServerApi.LogoutResult logoutResult;

		public Action<string, JsonObject> onSave;
		public Action<string, JsonObject> onLogout;

		public IEnumerator Login(Action<ServerApi.LoginResult> callback, string email, string password)
		{
			callback.Invoke(loginResult);
			yield break;
		}

		public IEnumerator Save(Action<ServerApi.SaveResult> callback, string accessToken, JsonObject playerData)
		{
			onSave.Invoke(accessToken, playerData);
			callback.Invoke(saveResult);
			yield break;
		}

		public IEnumerator Logout(Action<ServerApi.LogoutResult> callback, string accessToken, JsonObject playerData)
		{
			onLogout.Invoke(accessToken, playerData);
			callback.Invoke(logoutResult);
			yield break;
		}
	}

	private FakeBehaviour behaviour;

	private IDataRepository repo;

	private FakeServerApi api;

	private CloudManager manager;

	[SetUp]
	public void SetUp()
	{
		repo = new InMemoryDataRepository();
		api = new FakeServerApi();
		manager = new CloudManager(api, repo, "local");

		GameObject go = new GameObject("FakeGameObject");
		behaviour = go.AddComponent<FakeBehaviour>();
	}
	
	///////////
	// Login //
	///////////

	[Test]
	public void ItCanObtainAccessTokenByLoggingIn()
	{
		api.loginResult = new ServerApi.LoginResult {
			type = ServerApi.LoginResultType.OK,
			accessToken = "foo"
		};

		manager.Login(null, "email", "password");

		Assert.AreEqual("foo", manager.AccessToken);
	}

	[Test]
	public void ItWillDistributeDataAfterLogin()
	{
		api.loginResult = new ServerApi.LoginResult {
			type = ServerApi.LoginResultType.OK,
			accessToken = "<access-token>",
			playerData = new JsonObject()
				.Add("foo", "foo-value")
		};

		manager.Login(null, "email", "password");

		manager.Load(behaviour);

		Assert.AreEqual("foo-value", behaviour.foo);
	}

	[Test]
	public void ItWillDistributeDataRegisteredBeforeLogin()
	{
		api.loginResult = new ServerApi.LoginResult {
			type = ServerApi.LoginResultType.OK,
			accessToken = "<access-token>",
			playerData = new JsonObject()
				.Add("foo", "foo-value")
		};

		manager.LoadAfterLogin(behaviour);

		manager.Login(null, "email", "password");

		Assert.AreEqual("foo-value", behaviour.foo);
	}

	[Test]
	public void ItWillPutPlayerDataIntoCacheAfterLogin()
	{
		api.loginResult = new ServerApi.LoginResult {
			type = ServerApi.LoginResultType.OK,
			playerData = new JsonObject()
				.Add("foo", "foo-value")
				.Add("bar", 42)
		};

		manager.Login(null, "email", "password");

		Assert.AreEqual("foo-value", (string)repo.Get("foo"));
		Assert.AreEqual(42, (int)repo.Get("bar"));
	}

	[Test]
	public void ItIgnoresLoginCallWhenLoggedIn()
	{
		api.loginResult = new ServerApi.LoginResult {
			type = ServerApi.LoginResultType.OK,
			accessToken = "<access-token>",
			playerData = new JsonObject()
		};

		Assert.IsFalse(manager.LoggedIn);
		Assert.IsTrue(manager.Login(null, "email", "password"));
		Assert.IsTrue(manager.LoggedIn);
		Assert.IsFalse(manager.Login(null, "email", "password"));
	}

	[Test]
	public void LoginSuccessCallbackGetsCalled()
	{
		api.loginResult = new ServerApi.LoginResult {
			type = ServerApi.LoginResultType.OK,
			accessToken = "<access-token>",
			playerData = new JsonObject()
		};

		bool successCalled = false;
		bool failureCalled = false;
		
		manager.Login(
			() => { successCalled = true; },
			failure => { failureCalled = true; },
		"email", "password");

		Assert.True(successCalled);
		Assert.False(failureCalled);
	}

	[Test]
	public void LoginFailureCallbackGetsCalled()
	{
		api.loginResult = new ServerApi.LoginResult {
			type = ServerApi.LoginResultType.InvalidCredentials
		};

		bool successCalled = false;
		bool failureCalled = false;
		
		manager.Login(
			() => { successCalled = true; },
			failure => {
				failureCalled = true;
				Assert.AreEqual(LoginFailure.FailureType.BadCredentials, failure.type);
			},
		"email", "password");

		Assert.False(successCalled);
		Assert.True(failureCalled);
	}

	[Test]
	public void LocalDebugPlayerCanLogin()
	{
		PlayerPrefs.SetString(
			CloudManager.LocalDebugPlayerPrefsKey,
			new JsonObject()
				.Add("foo", "hello world")
				.Add("bar", 42)
				.ToString()
		);
		PlayerPrefs.Save();

		manager.Login(null, "local", "");

		manager.Load(behaviour);

		Assert.AreEqual("hello world", behaviour.foo);
		Assert.AreEqual(42, (int)repo.Get("bar"));
	}

	[Test]
	public void LocalDebugPlayerCanLoginImplicitly()
	{
		PlayerPrefs.SetString(
			CloudManager.LocalDebugPlayerPrefsKey,
			new JsonObject()
				.Add("foo", "hello world")
				.Add("bar", 42)
				.ToString()
		);
		PlayerPrefs.Save();

		manager.Load(behaviour);

		Assert.AreEqual("hello world", behaviour.foo);
		Assert.AreEqual(42, (int)repo.Get("bar"));
	}

	//////////
	// Save //
	//////////
	
	[Test]
	public void CannotSaveWhileNotLoggedIn()
	{
		Assert.False(manager.Save());
	}
	
	[Test]
	public void SavingCollectsDataAndSendsThemToServer()
	{
		api.loginResult = new ServerApi.LoginResult {
			type = ServerApi.LoginResultType.OK,
			accessToken = "<access-token>",
			playerData = new JsonObject().Add("foo", "initial-value")
		};

		manager.Login(null, "email", "password");
		manager.Load(behaviour);

		behaviour.foo = "changed-value";

		bool saveApiCalled = false;

		api.saveResult = new ServerApi.SaveResult {
			type = ServerApi.SaveResultType.OK
		};

		api.onSave = (accessToken, playerData) => {
			saveApiCalled = true;
			Assert.AreEqual("<access-token>", accessToken);
			Assert.AreEqual(DataRepositoryHelper.ToJsonObject(repo).ToString(), playerData.ToString());
		};

		Assert.True(manager.Save());

		Assert.True(saveApiCalled);
	}

	[Test]
	public void LocalDebugPlayerCanBeSaved()
	{
		PlayerPrefs.DeleteKey(CloudManager.LocalDebugPlayerPrefsKey);
		PlayerPrefs.Save();

		manager.Login(null, "local", "");
		manager.Load(behaviour);

		behaviour.foo = "new-value";

		api.onSave = (_, __) => Assert.Fail("Save api method was called when shouldn't.");

		Assert.True(manager.Save());

		var json = LightJson.Serialization.JsonReader.Parse(
			PlayerPrefs.GetString(CloudManager.LocalDebugPlayerPrefsKey, "{}")
		);

		Assert.AreEqual("new-value", (string)json["foo"]);
		Assert.AreEqual(1, json.AsJsonObject.Count);
	}

	[Test]
	public void UnexpectedLogoutOnSavingTriggersLogout()
	{
		api.loginResult = new ServerApi.LoginResult {
			type = ServerApi.LoginResultType.OK,
			accessToken = "<access-token>",
			playerData = new JsonObject()
		};

		manager.Login(null, "email", "password");

		api.saveResult = new ServerApi.SaveResult {
			type = ServerApi.SaveResultType.NotLoggedIn
		};
		api.onSave = (_, __) => {};

		Assert.True(manager.LoggedIn);

		manager.Save();

		Assert.IsNull(manager.AccessToken);
		Assert.False(manager.LoggedIn);
	}

	////////////
	// Logout //
	////////////

	[Test]
	public void CannotLogoutWhileNotLoggedIn()
	{
		Assert.False(manager.Logout());
	}

	[Test]
	public void LogoutCollectsDataAndSendsThemToServer()
	{
		api.loginResult = new ServerApi.LoginResult {
			type = ServerApi.LoginResultType.OK,
			accessToken = "<access-token>",
			playerData = new JsonObject().Add("foo", "initial-value")
		};

		manager.Login(null, "email", "password");
		manager.Load(behaviour);

		behaviour.foo = "changed-value";

		bool logoutApiCalled = false;

		api.logoutResult = new ServerApi.LogoutResult {
			type = ServerApi.LogoutResultType.OK
		};

		api.onLogout = (accessToken, playerData) => {
			logoutApiCalled = true;
			Assert.AreEqual("<access-token>", accessToken);
			Assert.AreEqual(DataRepositoryHelper.ToJsonObject(repo).ToString(), playerData.ToString());
		};

		Assert.True(manager.Logout());

		Assert.True(logoutApiCalled);
	}

	[Test]
	public void LocalDebugPlayerCanBeLoggedOut()
	{
		PlayerPrefs.DeleteKey(CloudManager.LocalDebugPlayerPrefsKey);
		PlayerPrefs.Save();

		manager.Login(null, "local", "");
		manager.Load(behaviour);

		behaviour.foo = "new-value";

		api.onLogout = (_, __) => Assert.Fail("Logout api method was called when shouldn't.");

		Assert.True(manager.Logout());

		var json = LightJson.Serialization.JsonReader.Parse(
			PlayerPrefs.GetString(CloudManager.LocalDebugPlayerPrefsKey, "{}")
		);

		Assert.AreEqual("new-value", (string)json["foo"]);
		Assert.AreEqual(1, json.AsJsonObject.Count);
	}
}
