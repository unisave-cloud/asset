using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using Unisave;

public class SerializerTest
{
	////////////////
	// Primitives //
	////////////////

	[Test]
	public void ItSerializesIntegers()
	{
		Assert.AreEqual("42", Serializer.Save(42));
		Assert.AreEqual("0", Serializer.Save(0));
		Assert.AreEqual("-5", Serializer.Save(-5));

		Assert.AreEqual(42, Serializer.Load<int>("42"));
		Assert.AreEqual(0, Serializer.Load<int>("0"));
		Assert.AreEqual(-5, Serializer.Load<int>("-5"));
	}

	[Test]
	public void ItSerializesStrings()
	{
		Assert.AreEqual("\"foo\"", Serializer.Save("foo"));
		Assert.AreEqual("\"lorem\\nipsum\"", Serializer.Save("lorem\nipsum"));
		
		Assert.AreEqual("foo", Serializer.Load<string>("\"foo\""));
		Assert.AreEqual("lorem\nipsum", Serializer.Load<string>("\"lorem\\nipsum\""));
	}

	//////////////////////
	// Unity Primitives //
	//////////////////////

	[Test]
	public void ItSerializesVectors()
	{
		Assert.AreEqual(@"{""x"":1.0,""y"":2.0,""z"":3.0}", Serializer.Save(new Vector3(1, 2, 3)));
		Assert.AreEqual(new Vector3(1, 2, 3), Serializer.Load<Vector3>(@"{""x"":1.0,""y"":2.0,""z"":3.0}"));

		Assert.AreEqual(@"{""x"":1.0,""y"":2.0}", Serializer.Save(new Vector2(1, 2)));
		Assert.AreEqual(new Vector2(1, 2), Serializer.Load<Vector2>(@"{""x"":1.0,""y"":2.0}"));
	}
}
