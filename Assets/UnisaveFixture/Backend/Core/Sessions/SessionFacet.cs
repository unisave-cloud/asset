using LightJson;
using Unisave.Arango;
using Unisave.Facades;
using Unisave.Facets;
using UnityEngine;

namespace UnisaveFixture.Backend.Core.Sessions
{
    public class SessionFacet : Facet
    {
        public void Set(string key, Vector3 value)
        {
            Session.Set(key, value);
        }

        public Vector3 Get(string key, Vector3 defaultValue)
        {
            return Session.Get(key, defaultValue);
        }

        public JsonObject GetSessionRecord()
        {
            try
            {
                return DB.Query(@"
                    FOR r IN u_sessions
                        LIMIT 1
                        RETURN r
                ").First();
            }
            catch (ArangoException e) when (e.ErrorNumber == 1203)
            {
                // collection or view not found
                return null;
            }
        }

        public void EmptyFacetMethod()
        {
            //
        }
    }
}