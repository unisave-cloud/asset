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

		public IEnumerator Login(Action<ServerApi.LoginResult> callback, string email, string password)
		{
			callback.Invoke(loginResult);
			yield break;
		}

		public IEnumerator Save(Action<ServerApi.SaveResult> callback, string accessToken, JsonObject playerData)
		{
			callback.Invoke(saveResult);
			yield break;
		}

		public IEnumerator Logout(Action<ServerApi.LogoutResult> callback, string accessToken, JsonObject playerData)
		{
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

	/*[Test]
	public void ItCanLoadDistributeCollectAndSave()
	{
		PlayerPrefs.SetString(CloudManager.LOCAL_DEBUG_PLAYER_PREFS_KEY, new JsonObject()
			.Add("foo", "lorem ipsum")
			.ToString()
		);
		PlayerPrefs.Save();

		UnisavePreferences preferences = ScriptableObject.CreateInstance<UnisavePreferences>();
		CloudManager manager = new CloudManager(preferences);
		manager.Login(null, preferences.localDebugPlayerEmail, "secret");

		GameObject go = new GameObject("FakeGameObject");
		FakeBehaviour behaviour = go.AddComponent<FakeBehaviour>();

		Assert.IsNull(behaviour.foo);

		manager.Load(behaviour);

		Assert.AreEqual("lorem ipsum", behaviour.foo);

		behaviour.foo = "changed value";

		manager.Save();

		JsonObject o = JsonReader.Parse(
			PlayerPrefs.GetString(CloudManager.LOCAL_DEBUG_PLAYER_PREFS_KEY, "{}"
		)).AsJsonObject;
		Assert.AreEqual("changed value", (string)o["foo"]);

		behaviour.foo = "changed again";

		manager.Logout();

		o = JsonReader.Parse(
			PlayerPrefs.GetString(CloudManager.LOCAL_DEBUG_PLAYER_PREFS_KEY, "{}"
		)).AsJsonObject;
		Assert.AreEqual("changed again", (string)o["foo"]);
	}*/
}
