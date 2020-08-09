using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LightJson;
using NUnit.Framework;
using Steamworks;
using Unisave.Facades;
using Unisave.Testing;
using UnisaveFixture.Backend.SteamMicrotransactions;
using UnisaveFixture.Backend.SteamMicrotransactions.VirtualProducts;
using UnisaveFixture.SteamMicrotransactions;
using UnityEngine;
using UnityEngine.TestTools;

// Steam documentation of the entire process:
// https://partner.steamgames.com/doc/features/microtransactions/implementation

namespace UnisaveFixture.Tests.SteamMicrotransactions
{
    [TestFixture]
    public class SteamMicrotransactionTest : BackendTestCase
    {
        private class SteamPurchasingClientMock
            : SteamPurchasingClient
        {
            protected override ulong GetSteamId()
            {
                return 123456789L;
            }
        }
        
        public override void SetUp()
        {
            base.SetUp();
            
            // fail-load the steam manager
            LogAssert.Expect(
                LogType.Error,
                new Regex(
                    @"\[Steamworks\.NET\] SteamAPI_Init\(\) failed\."
                )
            );
            Assert.IsFalse(SteamManager.Initialized);
            
            // setup environment variables
            Env.Set("STEAM_API_URL", "http://api.steam.com/");
            Env.Set("STEAM_PUBLISHER_KEY", "<steam-publisher-key>");
            Env.Set("STEAM_APP_ID", "<steam-app-id>");
        }

        [UnityTest]
        public IEnumerator ClickingBuyInitiatesTransaction()
        {
            // setup scene
            var go = new GameObject();
            var smm = go.AddComponent<SteamPurchasingClientMock>();
            yield return null;

            // setup HTTP
            Http.Fake(Http.Response(new JsonObject {
                ["response"] = new JsonObject {
                    ["result"] = "OK",
                    ["params"] = new JsonObject {
                        // this order id is not used and
                        // it does not match the generated
                        ["orderid"] = "938473",
                        ["transid"] = "374839"
                    }
                }
            }, 200));
            
            // click the button
            smm.PlayerClickedBuy();
            
            // assert request to initiate transaction has been sent
            Http.AssertSent(request =>
                request.Url.Contains("api.steam.com") &&
                request.Url.Contains("InitTxn/v3") &&
                request["key"] == "<steam-publisher-key>" &&
                request["appid"] == "<steam-app-id>" &&
                request["orderid"] != "0" &&
                request["steamid"] == "123456789" &&
                request["itemcount"] == "1" &&
                request["language"] == "en" &&
                request["currency"] == "USD" &&
                request["itemid[0]"] == "1" &&
                request["qty[0]"] == "3" &&
                request["amount[0]"] == "1500" &&
                request["description[0]"].AsString
                    .Contains("An example product")
            );
            
            // assert a transaction entity has been created
            var entities = DB.TakeAll<SteamTransactionEntity>().Get();
            Assert.AreEqual(1, entities.Count);
            var entity = entities[0];
            Assert.AreEqual(SteamTransactionEntity.InitiatedState, entity.state);
            Assert.AreEqual("USD", entity.currency);
            Assert.AreEqual("en", entity.language);
            Assert.AreNotEqual(0L, entity.orderId);
            Assert.AreEqual(374839L, entity.transactionId);
            Assert.AreEqual(123456789L, entity.playerSteamId);
            Assert.AreEqual(1, entity.items.Count);
            Assert.AreEqual(1, entity.items[0].itemId);
            Assert.AreEqual(15_00, entity.items[0].totalAmountInCents);
            Assert.AreEqual(3, entity.items[0].quantity);
            StringAssert.Contains(
                "An example product",
                entity.items[0].description
            );
            Assert.AreEqual(
                typeof(ExampleVirtualProduct).FullName,
                entity.items[0].productClass
            );

            // now the Steamworks API will notify the game via a callback
        }

        [UnityTest]
        public IEnumerator ReceivingPositiveUserResponseFinishesTransaction()
        {
            // setup scene
            var go = new GameObject();
            var smm = go.AddComponent<SteamPurchasingClientMock>();
            yield return null;
            
            // setup database
            var transaction = new SteamTransactionEntity {
                state = SteamTransactionEntity.InitiatedState,
                playerSteamId = 123456789L,
                orderId = 111222333L,
                transactionId = 374839L,
                language = "en",
                currency = "USD",
                items = new List<SteamTransactionEntity.Item> {
                    new SteamTransactionEntity.Item {
                        itemId = 1,
                        quantity = 3,
                        totalAmountInCents = 15_00,
                        description = "An example product, that a user can buy.",
                        category = null,
                        productClass = typeof(ExampleVirtualProduct).FullName
                    }
                }
            };
            transaction.Save();
            
            // setup HTTP
            Http.Fake(Http.Response(new JsonObject {
                ["response"] = new JsonObject {
                    ["result"] = "OK",
                    ["params"] = new JsonObject {
                        ["orderid"] = "111222333",
                        ["transid"] = "374839"
                    }
                }
            }, 200));
            
            // Steamworks fires the callback
            smm.SteamworksCallbackHandler(
                new MicroTxnAuthorizationResponse_t {
                    m_ulOrderID = 111222333L,
                    m_unAppID = 440,
                    m_bAuthorized = 1
                }
            );
            
            // steam received finalization request
            Http.AssertSent(request =>
                request.Url.Contains("api.steam.com") &&
                request.Url.Contains("FinalizeTxn/v2") &&
                request["key"] == "<steam-publisher-key>" &&
                request["appid"] == "<steam-app-id>" &&
                request["orderid"] == "111222333"
            );
            
            // transaction was updated accordingly
            transaction.Refresh();
            Assert.AreEqual(
                SteamTransactionEntity.CompletedState,
                transaction.state
            );
            
            // products were given to the player
            LogAssert.Expect(
                LogType.Log,
                $"Giving item {nameof(ExampleVirtualProduct)} to the player..."
            );
            
            // client logs success
            LogAssert.Expect(
                LogType.Log,
                new Regex(
                    @"^The transaction has succeeded and the products have",
                    RegexOptions.Multiline
                )
            );
        }
        
        // TODO: finalizing transaction that does not exist
        
        // TODO: finalizing transaction that isn't initiated
        
        // TODO: user rejects transaction authentication
        
        // TODO: steam rejects transaction initiation
        
        // TODO: steam rejects transaction finalization
    }
}