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

	[Test]
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
	}
}
