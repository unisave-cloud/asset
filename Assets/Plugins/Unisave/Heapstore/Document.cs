using LightJson;

namespace Unisave.Heapstore
{
    public class Document
    {
        public string Collection { get; }
        public string Key { get; }
        public string Id { get; }
        public JsonObject Data { get; }

        public T As<T>()
        {
            return default;
        }
    }
}