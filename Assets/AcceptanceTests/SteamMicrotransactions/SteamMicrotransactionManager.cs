using System;
using Steamworks;
using Unisave.Facades;
using UnityEngine;

namespace AcceptanceTests.SteamMicrotransactions
{
    public class SteamMicrotransactionManager : MonoBehaviour
    {
        public async void PlayerClickedBuy()
        {
//            var cart = new SteamShoppingCart()
//                .Add(new SmallGoldPack());
//
//            await OnFacet<SteamPurchasingServerFacet>.CallAsync(
//                nameof(SteamPurchasingServerFacet.InitiateTransaction),
//                cart
//            );
        }
        
        public async void AfterSteamHandledCheckout(
            MicroTxnAuthorizationResponse_t response
        )
        {
//            await OnFacet<SteamPurchasingServerFacet>.CallAsync(
//                nameof(SteamPurchasingServerFacet.FinalizeTransaction),
//                response.m_unAppID, // uint
//                response.m_ulOrderID, // ulong
//                response.m_bAuthorized // byte
//            );

            if (response.m_bAuthorized == 1)
            {
                // display success to the user
                Debug.Log("Transaction succeeded");
            }
            else
            {
                // display error to the user
                Debug.Log("User rejected the transaction");
            }
        }
        
        // ========= code that registers the steam callback =========
        
        private Callback<MicroTxnAuthorizationResponse_t> callback;

        private void OnEnable()
        {
            if (SteamManager.Initialized)
            {
                callback = Callback<MicroTxnAuthorizationResponse_t>
                    .Create(AfterSteamHandledCheckout);
            }
        }
        
        private void OnDisable()
        {
            if (callback != null)
            {
                callback.Dispose();
                callback = null;
            }
        }
    }
}