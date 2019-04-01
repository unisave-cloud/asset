using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using Unisave;
using LightJson;

public class NonNullTest
{
	private class FakeTarget
	{
		[SavedAs("foo")]
		[NonNull]
		public string foo = "default-foo";

		[SavedAs("bar")]
		[NonNull]
		public string bar {
			get { return barPropBacking; }
			set { barPropBacking = value; }
		}
		private string barPropBacking = "default-bar";
	}

	private IDataRepository repo;
	private Distributor distributor;
	private FakeTarget target;

	[SetUp]
	public void SetUp()
	{
		repo = new InMemoryDataRepository();
		distributor = new Distributor(repo);
		target = new FakeTarget();
	}

	[Test]
	public void DistributingNullIntoNonNullKeepsDefaultInstead()
	{
		repo.Set("foo", JsonValue.Null);
		repo.Set("bar", JsonValue.Null);

		distributor.Distribute(target);

		Assert.AreEqual("default-foo", target.foo);
		Assert.AreEqual("default-bar", target.bar);
	}

	[Test]
	public void DistributingEmptyRepoIntoNonNullAlsoKeepsDefaults()
	{
		distributor.Distribute(target);

		Assert.AreEqual("default-foo", target.foo);
		Assert.AreEqual("default-bar", target.bar);
	}

	[Test]
	public void NonNullCanCollectNullValue() // meaning NonNull cares only about distribution
	{
		distributor.Distribute(target);

		target.foo = null;
		target.bar = null;

		distributor.Collect();

		Assert.AreEqual(JsonValue.Null, repo.Get("foo"));
		Assert.AreEqual(JsonValue.Null, repo.Get("bar"));
	}
}
