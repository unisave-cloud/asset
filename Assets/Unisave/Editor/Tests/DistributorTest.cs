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

		[SavedAs("baz-field")]
		private string bazPrivate;
		public string bazAccess {
			get { return bazPrivate; }
			set { bazPrivate = value; }
		}
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
		repo.Set("baz-field", JsonValue.Null);

		distributor.Distribute(target);

		Assert.IsNull(target.barField);
		Assert.IsNull(target.barField);
		Assert.IsNull(target.bazAccess);
	}

	[Test]
	public void ItDistributesRepositoryDataToFields()
	{
		repo.Set("foo-field", "hello field");
		repo.Set("bar-field", "hello field 2");
		repo.Set("baz-field", "hello field 3");
		
		distributor.Distribute(target);

		Assert.AreEqual("hello field", target.fooField);
		Assert.AreEqual("hello field 2", target.barField);
		Assert.AreEqual("hello field 3", target.bazAccess);
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

	[Test]
	public void ItCollectsDataWhenDistributionDidntWriteAnything()
	{
		distributor.Distribute(target);

		target.fooField = "lorem";
		target.barProp = "ipsum";

		distributor.Collect();

		Assert.AreEqual("lorem", (string)repo.Get("foo-field"));
		Assert.AreEqual("ipsum", (string)repo.Get("bar-prop"));
	}

	[Test]
	public void ItCollectsDataThatHasBeenPreviouslyDistributed()
	{
		repo.Set("foo-field", "hello prop");
		repo.Set("bar-prop", "hello prop 2");

		distributor.Distribute(target);

		target.fooField = "lorem";
		target.barProp = "ipsum";

		distributor.Collect();

		Assert.AreEqual("lorem", (string)repo.Get("foo-field"));
		Assert.AreEqual("ipsum", (string)repo.Get("bar-prop"));
	}
}
