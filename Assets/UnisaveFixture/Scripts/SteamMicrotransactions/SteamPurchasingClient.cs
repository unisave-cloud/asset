using System;
using Steamworks;
using Unisave.Facades;
using UnisaveFixture.Backend.SteamMicrotransactions;
using UnisaveFixture.Backend.SteamMicrotransactions.VirtualProducts;
using UnityEngine;

namespace UnisaveFixture.SteamMicrotransactions
{
    public class SteamPurchasingClient : MonoBehaviour
    {
        /// <summary>
        /// Emulates clicking "Buy" on an ExampleVirtualProduct.
        /// Modify, rename, duplicate this method as you see fit.
        /// </summary>
        public async void PlayerClickedBuy()
        {
            // NOTE: This method acts as an example. You can modify,
            // duplicate, rename it to implement all the behaviour you need.
            
            var transaction = new SteamTransactionEntity {
                playerSteamId = GetSteamId(),
                language = "en",
                currency = "USD"
            };

            transaction.AddProduct<ExampleVirtualProduct>(
                quantity: 3
            );

            try
            {
                await OnFacet<SteamPurchasingServerFacet>.CallAsync(
                    nameof(SteamPurchasingServerFacet.InitiateTransaction),
                    transaction
                );
            }
            catch (Exception e)
            {
                ReportErrorToThePlayer(e.Message);
            }
        }
        
        /// <summary>
        /// Called when a transaction successfully finishes
        /// </summary>
        /// <param name="transaction"></param>
        public void TransactionHasBeenSuccessful(
            SteamTransactionEntity transaction
        )
        {
            // NOTE: Modify this method to perform the logic you want.
            // - Load new player data from the server
            //    - e.g. reload this scene / go to another scene / other reloading
            // - Display a success dialog
            
            // WARNING: Don't give the purchased products to the player here,
            // use ExampleVirtualProduct.GiveToPlayer(...) instead.
            // Here we are on the untrusted client-side.
            
            Debug.Log(
                "The transaction has succeeded and the products have been " +
                "given to the player. Now it's time to refresh data from the " +
                "server so that the player sees the purchased items."
            );
        }

        /// <summary>
        /// Displays error message to the player
        /// </summary>
        /// <param name="message">The technical message behind the error</param>
        public void ReportErrorToThePlayer(string message)
        {
            // NOTE: Modify this method to perform the logic you want.
            // - Display an error dialog
            
            Debug.LogError("Steam microtransaction failed: " + message);
        }
        
        
        //
        // ====================================================
        //
        //           Don't worry about the code below.
        //
        //           (you can, but you don't have to)
        //
        // ====================================================
        //
  
        
        /// <summary>
        /// This method is called by Steamworks when the transaction finishes
        /// (successfully or not - see the method argument)
        /// </summary>
        /// <param name="response"></param>
        public async void AfterSteamHandledCheckout(
            MicroTxnAuthorizationResponse_t response
        )
        {
            // finish the transaction
            SteamTransactionEntity transaction;
            try
            {
                transaction = await OnFacet<SteamPurchasingServerFacet>
                    .CallAsync<SteamTransactionEntity>(
                        nameof(SteamPurchasingServerFacet.FinalizeTransaction),
                        response.m_ulOrderID,
                        response.m_bAuthorized == 1
                    );
            }
            catch (Exception e)
            {
                ReportErrorToThePlayer(e.Message);
                return;
            }
            
            // transaction has been rejected by the player
            if (response.m_bAuthorized != 1)
            {
                ReportErrorToThePlayer("User rejected the transaction");
                return;
            }

            // everything went according to plans
            TransactionHasBeenSuccessful(transaction);
        }
        
        /// <summary>
        /// The Steamworks callback for transaction finalization
        /// </summary>
        private Callback<MicroTxnAuthorizationResponse_t> callback;

        private void OnEnable()
        {
            // register Steamworks callback
            if (SteamManager.Initialized)
            {
                callback = Callback<MicroTxnAuthorizationResponse_t>
                    .Create(AfterSteamHandledCheckout);
            }
        }
        
        private void OnDisable()
        {
            // un-register Steamworks callback
            if (callback != null)
            {
                callback.Dispose();
                callback = null;
            }
        }

        /// <summary>
        /// Returns steam ID of the current player.
        /// (It's a separate method to allow for mocking and testing.)
        /// </summary>
        protected virtual ulong GetSteamId()
        {
            return SteamUser.GetSteamID().m_SteamID;
        }
    }
}