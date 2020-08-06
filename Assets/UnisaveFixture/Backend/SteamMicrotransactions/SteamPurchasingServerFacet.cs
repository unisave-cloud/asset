using System;
using System.Collections.Generic;
using Unisave.Facades;
using Unisave.Facets;
using Unisave.Foundation;

namespace UnisaveFixture.Backend.SteamMicrotransactions
{
    public class SteamPurchasingServerFacet : Facet
    {
        private Env env;
        
        public SteamPurchasingServerFacet()
        {
            // TODO: create env facade
            env = Facade.App.Resolve<Env>();
        }
        
        /// <summary>
        /// Call this method to initiate a new transaction
        /// </summary>
        /// <param name="transaction"></param>
        public void InitiateTransaction(SteamTransactionEntity transaction)
        {
            // check transaction has all the required values present
            ValidateNewTransaction(transaction);
            
            // remember the transaction
            //
            // NOTE: Here you can add additional data into
            // the transaction entity, like currently logged-in player, etc...
            transaction.state = SteamTransactionEntity.BeingPreparedState;
            transaction.Save();
            
            // tell Steam we want to initiate a transaction
            var response = Http.Post(
                "https://partner.steam-api.com/ISteamMicroTxn/InitTxn/v3/",
                BuildInitTxnRequestBody(transaction)
            );
            response.Throw();

            // Steam returned a non-OK response
            if (response["response"]["result"].AsString != "OK")
            {
                transaction.state = SteamTransactionEntity.InitiationErrorState;
                transaction.errorCode
                    = response["response"]["error"]["errorcode"].AsString;
                transaction.errorDescription
                    = response["response"]["error"]["errordesc"].AsString;
                transaction.Save();
                
                Log.Error(
                    "Steam returned a non-OK initiation response.",
                    transaction
                );
                throw new Exception(
                    "Steam didn't like the transaction initiation:\n" +
                    $"[{transaction.errorCode}] {transaction.errorDescription}"
                );
            }
            
            // remember the transaction ID and update transaction state
            transaction.state = SteamTransactionEntity.InitiatedState;
            transaction.transactionId
                = ulong.Parse(response["response"]["params"]["transid"].AsString);
            transaction.Save();
            
            // Now the player will be prompted by Steam app to authorize the
            // transaction and after that, Steam will notify your game
            // via a Steamworks callback.
        }

        private void ValidateNewTransaction(SteamTransactionEntity transaction)
        {
            if (transaction.EntityId != null)
                throw new ArgumentException(
                    "Given transaction has already been initiated."
                );
            
            if (transaction.playerSteamId == 0)
                throw new ArgumentException(
                    $"Given transaction does not have " +
                    $"{nameof(transaction.playerSteamId)} specified."
                );
            
            if (transaction.items.Count == 0)
                throw new ArgumentException(
                    "Given transaction has no items inside of it."
                );
        }

        private Dictionary<string, string> BuildInitTxnRequestBody(
            SteamTransactionEntity transaction
        )
        {
            var body = new Dictionary<string, string> {
                ["key"] = env["STEAM_PUBLISHER_KEY"],
                ["orderid"] = transaction.orderId.ToString(),
                ["steamid"] = transaction.playerSteamId.ToString(),
                ["appid"] = env["STEAM_APP_ID"],
                ["itemcount"] = transaction.items.Count.ToString(),
                ["language"] = transaction.language,
                ["currency"] = transaction.currency
            };

            for (int i = 0; i < transaction.items.Count; i++)
            {
                var item = transaction.items[i];
            
                body[$"itemid[{i}]"] = item.itemId.ToString();
                body[$"qty[{i}]"] = item.quantity.ToString();
                body[$"amount[{i}]"] = item.totalAmountInCents.ToString();
                body[$"description[{i}]"] = item.description;
                if (!string.IsNullOrWhiteSpace(item.category))
                    body[$"category[{i}]"] = item.category;
            }

            return body;
        }

        /// <summary>
        /// This method is called after a transaction is authorized
        /// by the player and it performs the finalization
        /// (verifies transaction was paid and gives the player bought products)
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="authorized">
        /// Did the player authorize the transaction?
        /// </param>
        public SteamTransactionEntity FinalizeTransaction(
            ulong orderId,
            bool authorized
        )
        {
            // get the whole transaction again from the order id
            var transaction = FindInitiatedTransaction(orderId);
            
            // tell Steam we want to finalize the transaction
            var response = Http.Post(
                "https://partner.steam-api.com/ISteamMicroTxn/FinalizeTxn/v2/",
                new Dictionary<string, string> {
                    ["key"] = env["STEAM_PUBLISHER_KEY"],
                    ["orderid"] = orderId.ToString(),
                    ["appid"] = env["STEAM_APP_ID"]
                }
            );
            response.Throw();

            // The player didn't authorize the transaction
            if (!authorized)
            {
                transaction.state = SteamTransactionEntity.AuthorizationDeniedState;
                transaction.Save();
                return transaction;
            }
            
            // Steam returned a non-OK response
            if (response["response"]["result"].AsString != "OK")
            {
                transaction.state = SteamTransactionEntity.FinalizationErrorState;
                transaction.errorCode
                    = response["response"]["error"]["errorcode"].AsString;
                transaction.errorDescription
                    = response["response"]["error"]["errordesc"].AsString;
                transaction.Save();
                
                Log.Error(
                    "Steam returned a non-OK finalization response.",
                    transaction
                );
                throw new Exception(
                    "Steam didn't finalize the transaction:\n" +
                    $"[{transaction.errorCode}] {transaction.errorDescription}"
                );
            }
            
            // the transaction has been authorized by the player and paid
            transaction.state = SteamTransactionEntity.AuthorizedState;
            transaction.Save();
            
            // give the bought products to the player
            GiveProductsToPlayer(transaction);
            transaction.state = SteamTransactionEntity.CompletedState;
            transaction.Save();

            return transaction;
        }

        private SteamTransactionEntity FindInitiatedTransaction(ulong orderId)
        {
            var transaction = DB.TakeAll<SteamTransactionEntity>()
                .Filter(t =>
                    t.orderId == orderId &&
                    t.state == SteamTransactionEntity.InitiatedState
                )
                .First();
            
            if (transaction == null)
                throw new Exception(
                    $"No initiated transaction with order id {orderId} was found."
                );

            return transaction;
        }

        private void GiveProductsToPlayer(SteamTransactionEntity transaction)
        {
            foreach (var item in transaction.items)
            {
                Type productType = Type.GetType(item.productClass);
                    
                if (productType == null)
                    throw new Exception(
                        $"Cannot find product {item.productClass}"
                    );
                
                var instance = (IVirtualProduct)Activator.CreateInstance(
                    productType
                );

                var method = productType.GetMethod(
                    nameof(IVirtualProduct.GiveToPlayer)
                );
                
                if (method == null)
                    throw new Exception(
                        $"Class {item.productClass} does not contain " +
                        $"method {nameof(IVirtualProduct.GiveToPlayer)}"
                    );

                for (int i = 0; i < item.quantity; i++)
                {
                    method.Invoke(instance, new object[] { transaction });
                    
                    Log.Info("Product has been given to the player.", item);
                }
            }
        }
    }
}