using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using NUnit.Framework;
using Unisave.Facets;
using UnityEngine;

namespace UnisaveFixture.Tests.Unit
{
    [TestFixture]
    public class LinqExpressionInterpreterTest
    {
        // === Helper types ===
        
        private class MyClass
        {
            public string myField = "foo";
            public static string myStaticField = "staticFoo";
            public const string MyConstant = "mYcOnStAnT";

            public int FortyTwo() => 42;
            public static int StaticFortyTwo() => 42;
        }

        private readonly MyClass myInstance = new MyClass();

        private enum MyEnum
        {
            Foo = 1,
            Bar = 2
        }
        
        // === Tests ===
        
        private T InterpretValue<T>(Expression<Func<T>> lambda)
        {
            object value = LinqExpressionInterpreter.Interpret(lambda.Body);
            return (T)(value ?? default(T));
        }
        
        [Test]
        public void Constants()
        {
            int value = InterpretValue(() => 5);
            Assert.AreEqual(5, value);
            
            string value2 = InterpretValue(() => "foo");
            Assert.AreEqual("foo", value2);

            object instance = new object();
            object value3 = InterpretValue(() => instance);
            Assert.AreSame(value3, instance);
        }

        [Test]
        public void Fields()
        {
            string value = InterpretValue(() => myInstance.myField);
            Assert.AreEqual("foo", value);
        }
        
        [Test]
        public void FieldOnNullInstance()
        {
            MyClass instance = null;

            Assert.Throws<NullReferenceException>(() => {
                InterpretValue(() => instance.myField);
            }, "instance.myField");
        }
        
        [Test]
        public void StaticFields()
        {
            string value = InterpretValue(() => MyClass.myStaticField);
            Assert.AreEqual("staticFoo", value);
        }

        [Test]
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public void EqualityComparisonTests()
        {
            int fiveInt32 = 5;
            float fiveFloat = 5f;
            double fiveDouble = 5f;
            string fooString = "foo";
            
            Assert.IsTrue(InterpretValue(() => fiveInt32 == 5));
            Assert.IsFalse(InterpretValue(() => fiveInt32 == 4));
            Assert.IsFalse(InterpretValue(() => fiveInt32 != 5));
            Assert.IsTrue(InterpretValue(() => fiveInt32 != 4));
            
            Assert.IsTrue(InterpretValue(() => fiveFloat == 5f));
            Assert.IsFalse(InterpretValue(() => fiveFloat == 4f));
            Assert.IsFalse(InterpretValue(() => fiveFloat != 5f));
            Assert.IsTrue(InterpretValue(() => fiveFloat != 4f));
            
            Assert.IsTrue(InterpretValue(() => fiveDouble == 5.0));
            Assert.IsFalse(InterpretValue(() => fiveDouble == 4.0));
            Assert.IsFalse(InterpretValue(() => fiveDouble != 5.0));
            Assert.IsTrue(InterpretValue(() => fiveDouble != 4.0));
            
            Assert.IsTrue(InterpretValue(() => fooString == "foo"));
            Assert.IsFalse(InterpretValue(() => fooString == "bar"));
            Assert.IsFalse(InterpretValue(() => fooString != "foo"));
            Assert.IsTrue(InterpretValue(() => fooString != "bar"));
        }

        [Test]
        public void ReturningEnums()
        {
            Assert.AreEqual(
                MyEnum.Foo,
                InterpretValue(() => MyEnum.Foo)
            );
            Assert.AreEqual(
                MyEnum.Bar,
                InterpretValue(() => MyEnum.Bar)
            );
        }

        [Test]
        public void CastingEnumsToInt()
        {
            Assert.AreEqual(
                (int)MyEnum.Foo,
                InterpretValue(() => (int)MyEnum.Foo)
            );
            Assert.AreEqual(
                (int)MyEnum.Bar,
                InterpretValue(() => (int)MyEnum.Bar)
            );
        }

        [Test]
        public void CastingNumericTypes()
        {
            // up-casting a byte to int
            byte myByte = 42;
            Assert.AreEqual(
                (int)myByte,
                InterpretValue(() => (int)myByte)
            );
            
            // down-casting an int to byte
            Assert.AreEqual(
                (byte)42,
                InterpretValue(() => (byte)42)
            );
            
            // int to double
            Assert.AreEqual(
                (double)42,
                InterpretValue(() => (double)42)
            );
            
            // double to int
            Assert.AreEqual(
                (int)42.0,
                InterpretValue(() => (int)42.0)
            );
        }

        [Test]
        public void BoxingValueTypes()
        {
            Assert.AreEqual(
                42,
                (int)InterpretValue(() => (object)42)
            );
            Assert.AreEqual(
                MyEnum.Foo,
                (MyEnum)InterpretValue(() => (object)MyEnum.Foo)
            );
        }
        
        [Test]
        public void UnboxingValueTypes()
        {
            object fortyTwo = 42;
            object foo = MyEnum.Foo;
            
            Assert.AreEqual(
                42,
                InterpretValue(() => (int)fortyTwo)
            );
            Assert.AreEqual(
                MyEnum.Foo,
                InterpretValue(() => (MyEnum)foo)
            );
        }

        [Test]
        public void CallingInstanceMethods()
        {
            Assert.AreEqual(
                42,
                InterpretValue(() => myInstance.FortyTwo())
            );
        }
        
        [Test]
        public void CallingStaticMethods()
        {
            Assert.AreEqual(
                42,
                InterpretValue(() => MyClass.StaticFortyTwo())
            );
        }

        [Test]
        public void AccessingConstants()
        {
            Assert.AreEqual(
                MyClass.MyConstant,
                InterpretValue(() => MyClass.MyConstant)
            );
        }

        [Test]
        public void RetuningNull()
        {
            Assert.IsNull(
                InterpretValue(() => (object)null)
            );
            
            Assert.IsNull(
                InterpretValue(() => (MyClass)null)
            );
            
            Assert.IsNull(
                InterpretValue(() => (int?)null)
            );
        }

        [Test]
        public void CreatingNewInstances()
        {
            List<int> instance = InterpretValue(
                () => new List<int>(new []{ 1, 2, 3 })
            );
            Assert.AreEqual(3, instance.Count);
            Assert.AreEqual(1, instance[0]);
            Assert.AreEqual(2, instance[1]);
            Assert.AreEqual(3, instance[2]);
            
            Vector2 value = InterpretValue(() => new Vector2(1f, 2f));
            Assert.AreEqual(1f, value.x);
            Assert.AreEqual(2f, value.y);
        }

        [Test]
        public void CreatingNewArrayInstances()
        {
            int[] array = InterpretValue(() => new int[] { 1, 2, 3 });
            Assert.AreEqual(3, array.Length);
            Assert.AreEqual(1, array[0]);
            Assert.AreEqual(2, array[1]);
            Assert.AreEqual(3, array[2]);
            
            string[] array2 = InterpretValue(() => new string[] { "1", "2", "3" });
            Assert.AreEqual(3, array2.Length);
            Assert.AreEqual("1", array2[0]);
            Assert.AreEqual("2", array2[1]);
            Assert.AreEqual("3", array2[2]);
        }
        
        // NOTE: add tests for any problems that arise later
    }
}