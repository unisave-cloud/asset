using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using Unisave;
using LightJson;

public class InMemoryDataRepositoryTest
{
	private InMemoryDataRepository repo;

	[SetUp]
	public void SetUp()
	{
		repo = new InMemoryDataRepository();
		repo.Set("foo", 42);
	}

	[Test]
	public void ItReturnsNullForNonExistantFields()
	{
		Assert.AreEqual(JsonValue.Null, repo.Get("bar"));
	}

	[Test]
	public void ItReturnsValueForExistingFields()
	{
		Assert.AreEqual(42, (int)repo.Get("foo"));
	}

	[Test]
	public void ItCanStoreValue()
	{
		repo.Set("bar", "hello");
		Assert.AreEqual("hello", (string)repo.Get("bar"));
	}

	[Test]
	public void ItCanAskForKeyExistance()
	{
		Assert.IsTrue(repo.Has("foo"));
		Assert.IsFalse(repo.Has("bar"));
	}

	[Test]
	public void ItCanRemoveFields()
	{
		repo.Remove("foo");
		Assert.IsFalse(repo.Has("foo"));
	}

	[Test]
	public void ItCanListKeys()
	{
		repo.Set("bar", 42);
		string[] a = repo.AllKeys().ToArray();

		Assert.Contains("foo", a);
		Assert.Contains("bar", a);
	}
}
