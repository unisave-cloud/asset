using System;
using System.Collections;
using System.Collections.Generic;
using LightJson;
using NUnit.Framework;
using Unisave.Arango;
using Unisave.Contracts;
using Unisave.Facades;
using Unisave.Facets;
using Unisave.Foundation;
using Unisave.Runtime;
using UnisaveFixture.Backend.ExampleTesting;
using UnisaveFixture.ExampleTesting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Application = Unisave.Foundation.Application;

namespace UnisaveFixture.Tests.ExampleTesting
{
    public class ExampleIntegrationTest
    {
        #region "Test case behind the scenes"
        
        private Application app;
        private ClientApplication clientApp;
        
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
            
            clientApp = new ClientApplication(
                // TODO: resolve unisave preferences via some overriding stack
                UnisavePreferences.LoadOrCreate()
            );
            
            // swap out facet caller implementation
            clientApp.Singleton<FacetCaller>(
                _ => new TestingFacetCaller(app, clientApp)
            );
            
            Facade.SetApplication(app);
            ClientFacade.SetApplication(clientApp);
            
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
        
        [UnityTest]
        public IEnumerator ItCanCallFacetsFromTestMethod()
        {
            ExampleTestingFacet.addNumbersWasCalled = false;
            
            // TODO: CallSync for TestingFacetCaller
            yield return OnFacet<ExampleTestingFacet>.Call<int>(
                nameof(ExampleTestingFacet.AddNumbers),
                4, 7
            ).Then(result => {
                Assert.AreEqual(11, result);
                Assert.IsTrue(ExampleTestingFacet.addNumbersWasCalled);
            }).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator ItCanCallFacetsFromScene()
        {
            ExampleTestingFacet.addNumbersWasCalled = false;
            
            SceneManager.LoadScene(
                "UnisaveFixture/Scripts/ExampleTesting/ExampleTestingScene"
            );
            
            yield return null;

            var client = GameObject
                .Find("ExampleTestingClient")
                .GetComponent<ExampleTestingClient>();
            
            client.CallAddingFacet();
            
            yield return null;
            
            Assert.IsTrue(ExampleTestingFacet.addNumbersWasCalled);
        }
    }
}