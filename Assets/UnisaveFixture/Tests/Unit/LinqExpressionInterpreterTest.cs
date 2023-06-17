using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Unisave.Facets;

namespace UnisaveFixture.Tests.Unit
{
    [TestFixture]
    public class LinqExpressionInterpreterTest
    {
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
        
        private class MyClass
        {
            public string myField = "foo";
            public static string myStaticField = "staticFoo";
        }

        private MyClass myInstance = new MyClass();

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
        
        // NOTE: add tests for any problems that arise later
        
        // To be added that are implemented:
        // - returning value types (enum)
        // - casting values from object
        // - casting values to object
        // - calling all sorts of functions (methods, statics)
    }
}