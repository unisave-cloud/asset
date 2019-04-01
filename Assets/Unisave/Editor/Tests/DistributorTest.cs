using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using Unisave;
using LightJson;

public class DistributorTest
{
	private class FakeTarget
	{
		[SavedAs("foo-field")]
		public string fooField;

		[SavedAs("foo-prop")]
		public string fooProp { get; set; }

		[SavedAs("bar-field")]
		public string barField = "default";

		[SavedAs("bar-prop")]
		public string barProp {
			get { return barPropBacking; }
			set { barPropBacking = value; }
		}
		private string barPropBacking = "default";

		// TODO: baz, private field
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
	public void ItLeavesDefaultValueIfKeyMissing()
	{
		Assert.IsNull(target.fooField);
		Assert.IsNull(target.fooField);
		
		Assert.AreEqual("default", target.barField);
		Assert.AreEqual("default", target.barProp);

		distributor.Distribute(target);

		Assert.IsNull(target.fooField);
		Assert.IsNull(target.fooField);

		Assert.AreEqual("default", target.barField);
		Assert.AreEqual("default", target.barProp);
	}

	[Test]
	public void ItDistributesNullValue()
	{
		repo.Set("bar-field", JsonValue.Null);
		repo.Set("bar-prop", JsonValue.Null);

		distributor.Distribute(target);

		Assert.IsNull(target.barField);
		Assert.IsNull(target.barField);
	}

	[Test]
	public void ItDistributesRepositoryDataToFields()
	{
		repo.Set("foo-field", "hello field");
		repo.Set("bar-field", "hello field 2");
		
		distributor.Distribute(target);

		Assert.AreEqual("hello field", target.fooField);
		Assert.AreEqual("hello field 2", target.barField);
	}

	[Test]
	public void ItDistributesRepositoryDataToProperties()
	{
		repo.Set("foo-prop", "hello prop");
		repo.Set("bar-prop", "hello prop 2");
		
		distributor.Distribute(target);

		Assert.AreEqual("hello prop", target.fooProp);
		Assert.AreEqual("hello prop 2", target.barProp);
	}
}
