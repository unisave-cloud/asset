using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using Unisave;
using LightJson;
using LightJson.Serialization;

public class UnisaveLocalTest
{
	private class FakeBehaviour : MonoBehaviour
	{
		[SavedAs("foo")]
		public string foo;

		[SavedAs("bar")]
		[NonNull]
		public string bar = "default";
	}

	private FakeBehaviour behaviour;

	[SetUp]
	public void SetUp()
	{
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();

		GameObject go = new GameObject("FakeGameObject");
		behaviour = go.AddComponent<FakeBehaviour>();
	}

	[Test]
	public void ItLeavesDefaultValueAfterLoadIfNoneSaved()
	{
		UnisaveLocal.Load(behaviour);

		Assert.IsNull(behaviour.foo);
		Assert.AreEqual("default", behaviour.bar);
	}

	[Test]
	public void ItLoadsNullValue()
	{
		PlayerPrefs.SetString(UnisaveLocal.PLAYER_PREFS_KEY_PREFIX + "foo", "null");
		PlayerPrefs.Save();

		behaviour.foo = "not-null";

		UnisaveLocal.Load(behaviour);

		Assert.IsNull(behaviour.foo);
	}

	[Test]
	public void ItLoadsSavedValues()
	{
		PlayerPrefs.SetString(UnisaveLocal.PLAYER_PREFS_KEY_PREFIX + "foo", "\"foo-val\"");
		PlayerPrefs.SetString(UnisaveLocal.PLAYER_PREFS_KEY_PREFIX + "bar", "\"bar-val\"");
		PlayerPrefs.Save();

		UnisaveLocal.Load(behaviour);

		Assert.AreEqual("foo-val", behaviour.foo);
		Assert.AreEqual("bar-val", behaviour.bar);
	}

	[Test]
	public void ItSavesChanges()
	{
		behaviour.foo = "foo-val";
		behaviour.bar = "bar-val";

		UnisaveLocal.Save(behaviour);

		Assert.AreEqual(
			"\"foo-val\"",
			PlayerPrefs.GetString(UnisaveLocal.PLAYER_PREFS_KEY_PREFIX + "foo")
		);
		Assert.AreEqual(
			"\"bar-val\"",
			PlayerPrefs.GetString(UnisaveLocal.PLAYER_PREFS_KEY_PREFIX + "bar")
		);
	}

	[Test]
	public void LoadingNullIntoNonNullKeepsDefaultInstead()
	{
		PlayerPrefs.SetString(UnisaveLocal.PLAYER_PREFS_KEY_PREFIX + "bar", "null");
		PlayerPrefs.Save();

		UnisaveLocal.Load(behaviour);

		Assert.AreEqual("default", behaviour.bar);
	}

	[Test]
	public void ItSavesNullSetToNonNull() // meaning NonNull cares only about loading
	{
		behaviour.bar = null;

		UnisaveLocal.Save(behaviour);

		Assert.AreEqual(
			"null",
			PlayerPrefs.GetString(UnisaveLocal.PLAYER_PREFS_KEY_PREFIX + "bar")
		);
	}
}
