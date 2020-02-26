using LightJson;
using UnityEditor.IMGUI.Controls;

namespace Unisave.Uniarchy
{
    class EntityItem : TreeViewItem
    {
        private JsonObject entity;

        public EntityItem(JsonObject entity, IdAllocator idAllocator) : base()
        {
            this.entity = entity;
            
            displayName = entity["$type"].AsString + " [" + entity["_id"] + "]";
            id = idAllocator.NextId();
        }

        public JsonObject GetEntity()
        {
            return entity;
        }
    }
}
