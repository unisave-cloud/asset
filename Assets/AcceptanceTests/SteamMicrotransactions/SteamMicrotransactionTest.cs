using System.Collections;
using System.Text.RegularExpressions;
using AcceptanceTests.Backend.Logging;
using NUnit.Framework;
using Steamworks;
using Unisave.Testing;
using UnityEngine;
using UnityEngine.TestTools;

// Steam documentation of the entire process:
// https://partner.steamgames.com/doc/features/microtransactions/implementation

namespace AcceptanceTests.SteamMicrotransactions
{
    [TestFixture]
    public class SteamMicrotransactionTest : BackendTestCase
    {
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
        }
        
        [UnityTest]
        public IEnumerator ClickingBuySendsHttpRequestToSteam()
        {
            // scene setup
            var go = new GameObject();
            var smm = go.AddComponent<SteamMicrotransactionManager>();
            
            yield return null;
            
            // TODO: BackendTestCase should replace OnFacet facade as well
            // smm.PlayerClickedBuy();
            // assert http request was made
            
//            yield return OnFacet<LogFacet>.Call(
//                nameof(LogFacet.LogInfo)
//            ).AsCoroutine();

            yield return null;
        }

        [UnityTest]
        public IEnumerator ReceivingPositiveUserResponseFinishesTransaction()
        {
            // scene setup
            var go = new GameObject();
            var smm = go.AddComponent<SteamMicrotransactionManager>();
            
            yield return null;
            
            smm.AfterSteamHandledCheckout(
                new MicroTxnAuthorizationResponse_t {
                    m_ulOrderID = 42,
                    m_unAppID = 440,
                    m_bAuthorized = 1
                }
            );
            
            // assert finalization happens
        }
    }
}