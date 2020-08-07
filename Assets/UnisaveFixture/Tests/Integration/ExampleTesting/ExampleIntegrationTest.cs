using System.Collections;
using NUnit.Framework;
using Unisave.Facades;
using Unisave.Testing;
using UnisaveFixture.Backend.ExampleTesting;
using UnisaveFixture.ExampleTesting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace UnisaveFixture.Tests.ExampleTesting
{
    public class ExampleIntegrationTest : BackendTestCase
    {
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
        public void ItCanCallFacetsFromTestMethod()
        {
            ExampleTestingFacet.addNumbersWasCalled = false;
            
            var result = OnFacet<ExampleTestingFacet>.CallSync<int>(
                nameof(ExampleTestingFacet.AddNumbers),
                4, 7
            );
            
            Assert.AreEqual(11, result);
            Assert.IsTrue(ExampleTestingFacet.addNumbersWasCalled);
        }
        
        [UnityTest]
        public IEnumerator ItCanCallFacetsFromScene()
        {
            ExampleTestingFacet.addNumbersWasCalled = false;

            SceneManager.LoadScene(
                "UnisaveFixture/Scripts/ExampleTesting/ExampleTestingScene",
                LoadSceneMode.Additive
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