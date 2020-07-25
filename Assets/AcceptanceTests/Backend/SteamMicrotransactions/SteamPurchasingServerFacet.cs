using Unisave.Facets;

namespace AcceptanceTests.Backend.SteamMicrotransactions
{
    public class SteamPurchasingServerFacet : Facet
    {
        public void InitiateTransaction(/*SteamShoppingCart cart*/)
        {
//            var response = Http.Post(
//                "https://partner.steam-api.com/ISteamMicroTxn/InitTxn/v3/",
//                new Dictionary<string, string>() {
//                    ["key"] => "key",
//                    ["orderid"] => "orderid",
//                    ["steamid"] => "steamid",
//                    ["appid"] => "appid",
//                    // ...
//                }
//            );
//
//            response.Throw();
        }

        public void FinalizeTransaction(/* ... */)
        {
            
        }
    }
}