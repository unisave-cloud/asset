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
		PlayerPrefs.SetString(LocalManager.PlayerPrefsKeyPrefix + "foo", "null");
		PlayerPrefs.Save();

		behaviour.foo = "not-null";

		UnisaveLocal.Load(behaviour);

		Assert.IsNull(behaviour.foo);
	}

	[Test]
	public void ItLoadsSavedValues()
	{
		PlayerPrefs.SetString(LocalManager.PlayerPrefsKeyPrefix + "foo", "\"foo-val\"");
		PlayerPrefs.SetString(LocalManager.PlayerPrefsKeyPrefix + "bar", "\"bar-val\"");
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
			PlayerPrefs.GetString(LocalManager.PlayerPrefsKeyPrefix + "foo")
		);
		Assert.AreEqual(
			"\"bar-val\"",
			PlayerPrefs.GetString(LocalManager.PlayerPrefsKeyPrefix + "bar")
		);
	}

	[Test]
	public void LoadingNullIntoNonNullKeepsDefaultInstead()
	{
		PlayerPrefs.SetString(LocalManager.PlayerPrefsKeyPrefix + "bar", "null");
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
			PlayerPrefs.GetString(LocalManager.PlayerPrefsKeyPrefix + "bar")
		);
	}
}
