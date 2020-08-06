using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LightJson;
using NUnit.Framework;
using Unisave.Arango;
using Unisave.Contracts;
using Unisave.Facades;
using Unisave.Foundation;
using Unisave.Runtime;
using UnisaveFixture.Backend.ExampleTesting;
using UnityEngine;
using UnityEngine.TestTools;
using Application = Unisave.Foundation.Application;

namespace UnisaveFixture.Tests.ExampleTesting
{
    public class ExampleIntegrationTest
    {
        #region "Test case behind the scenes"
        
        private Application app;
        
        [SetUp]
        public void SetUp()
        {
            var env = new Env();
            
            // TODO: the .env file will be downloaded from the cloud
            
            env["SESSION_DRIVER"] = "memory";
            
            env["ARANGO_DRIVER"] = "http";
            env["ARANGO_BASE_URL"] = "http://127.0.0.1:8529/";
            env["ARANGO_DATABASE"] = "db_Jj0Y3Fu6";
            env["ARANGO_USERNAME"] = "db_user_Jj0Y3Fu6";
            env["ARANGO_PASSWORD"] = "JSmhb08w9fDCweT+ux/CM/Ur";
            
            app = Bootstrap.Boot(
                GetGameAssemblyTypes(),
                env,
                new SpecialValues()
            );
            
            // TODO: set ClientApplication to ClientFacade
            Facade.SetApplication(app);
            
            ClearDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            Facade.SetApplication(null);
            
            app.Dispose();
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

        private void ClearDatabase()
        {
            var arango = (ArangoConnection)app.Resolve<IArango>();
            JsonArray collections = arango.Get("/_api/collection")["result"];
            foreach (var c in collections)
                if (!c["isSystem"].AsBoolean)
                    arango.DeleteCollection(c["name"]);
        }
        
        #endregion
        
        [Test]
        public void ItCanAccessDatabaseFromTestMethod()
        {
            Assert.AreEqual(0, DB.TakeAll<ExampleTestingEntity>().Get().Count);
            
            var entity = new ExampleTestingEntity {
                foo = "Hello",
                bar = 42
            };
            entity.Save();
            
            Assert.AreEqual(1, DB.TakeAll<ExampleTestingEntity>().Get().Count);

            var loaded = DB.TakeAll<ExampleTestingEntity>().First();
            Assert.AreEqual(entity.foo, loaded.foo);
            Assert.AreEqual(entity.bar, loaded.bar);
            Assert.AreEqual(entity.CreatedAt, loaded.CreatedAt);
        }
        
        [Test]
        public async Task ItCanCallFacetsFromTestMethod()
        {
            int result = await OnFacet<ExampleTestingFacet>.CallAsync<int>(
                nameof(ExampleTestingFacet.AddNumbers),
                4, 7
            );
            
            Assert.AreEqual(11, result);
        }
        
        // TODO: ItCanCallFacetsFromScene
        
//        [Test]
//        public void ExampleIntegrationTestSimplePasses()
//        {
//            // Use the Assert class to test conditions.
//            
//        }
//
//        // A UnityTest behaves like a coroutine in PlayMode
//        // and allows you to yield null to skip a frame in EditMode
//        [UnityTest]
//        public IEnumerator ExampleIntegrationTestWithEnumeratorPasses()
//        {
//            // Use the Assert class to test conditions.
//            // yield to skip a frame
//            yield return null;
//        }
    }
}