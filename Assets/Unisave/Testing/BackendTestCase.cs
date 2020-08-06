using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unisave.Foundation;
using Unisave.Testing;

namespace Unisave.Testing
{
    /// <summary>
    /// Base class for Unisave backend tests
    /// </summary>
    public class BackendTestCase : BasicBackendTestCase
    {
        // TODO:
        // - soak up BasicBackendTestCase into this class
        //     - actually just move testing away from framework into the asset
        // - redesign client -> ClientApplication, ClientFacade(s)
        //     - all interactions with the ClientApp goe through ClientFacades
        
        [SetUp]
        public virtual void SetUp()
        {
            var env = new Env();
            SetUpDefaultEnv(env);
            
            // override with additional test configuration
            var preferences = UnisavePreferences.LoadOrCreate();
            if (preferences.TestingEnv != null)
            {
                var overrideEnv = Env.Parse(preferences.TestingEnv.text);
                env.OverrideWith(overrideEnv);
            }
            
            base.SetUp(
                GetGameAssemblyTypes(),
                env
            );
        }
        
        /// <summary>
        /// Sets up default values for the env configuration,
        /// before they get overriden by the testing env file
        /// </summary>
        private void SetUpDefaultEnv(Env env)
        {
            env["SESSION_DRIVER"] = "memory";
        }
        
        private Type[] GetGameAssemblyTypes()
        {
            // NOTE: gets all possible types, since there might be asm-def files
            // that make the situation more difficult
            
            List<Type> types = new List<Type>();

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                types.AddRange(asm.GetTypes());
            }

            return types.ToArray();
        }
        
        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }
        
        //////////////////////////
        // Implement assertions //
        //////////////////////////
        
        protected override void AssertAreEqual(
            object expected, object actual, string message = null
        )
        {
            if (message == null)
                Assert.AreEqual(expected, actual);
            else
                Assert.AreEqual(expected, actual, message);
        }

        protected override void AssertIsNull(
            object subject, string message = null
        )
        {
            if (message == null)
                Assert.IsNull(subject);
            else
                Assert.IsNull(subject, message);
        }

        protected override void AssertIsNotNull(
            object subject, string message = null
        )
        {
            if (message == null)
                Assert.IsNotNull(subject);
            else
                Assert.IsNotNull(subject, message);
        }

        public override void AssertIsTrue(
            bool condition, string message = null
        )
        {
            if (message == null)
                Assert.IsTrue(condition);
            else
                Assert.IsTrue(condition, message);
        }

        public override void AssertIsFalse(
            bool condition, string message = null
        )
        {
            if (message == null)
                Assert.IsFalse(condition);
            else
                Assert.IsFalse(condition, message);
        }
    }
}