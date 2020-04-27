using LightJson;
using Unisave.Arango;
using Unisave.Arango.Emulation;

namespace Unisave.Sessions
{
    /// <summary>
    /// Stores session data in a database collection
    /// TODO: should be replaced by a framework implementation once sessions
    /// TODO: won't be handled via sandbox API
    /// </summary>
    public class EmulatedSessionStorage : ISessionStorage
    {
        private const string CollectionName = "sessions";
        
        private ArangoInMemory arango;
        
        public EmulatedSessionStorage(ArangoInMemory arango)
        {
            this.arango = arango;
        }

        private Collection Collection()
        {
            try
            {
                return arango.GetCollection(CollectionName);
            }
            catch (ArangoException)
            {
                arango.CreateCollection(CollectionName, CollectionType.Document);
                return arango.GetCollection(CollectionName);
            }
        }
        
        public JsonObject Load(string sessionId)
        {
            return Collection().GetDocument(sessionId)?["sessionData"]
                   ?? new JsonObject();
        }

        public void Store(string sessionId, JsonObject sessionData, int lifetime)
        {
            Collection().InsertDocument(
                new JsonObject()
                    .Add("_key", sessionId)
                    .Add("sessionData", sessionData),
                new JsonObject()
                    .Add("overwrite", true)
            );
        }
    }
}