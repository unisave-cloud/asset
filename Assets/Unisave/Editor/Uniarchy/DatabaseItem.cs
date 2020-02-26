using Unisave.Arango.Emulation;
using UnityEditor.IMGUI.Controls;

namespace Unisave.Uniarchy
{
    sealed class DatabaseItem : TreeViewItem
    {
        public ArangoInMemory Arango { get; }

        public DatabaseItem(ArangoInMemory arango, IdAllocator idAllocator)
        {
            Arango = arango;

            displayName = "TODO arango name"; // TODO arango name
            id = idAllocator.NextId();
            
            BuildChildren(idAllocator);
        }

        private void BuildChildren(IdAllocator idAllocator)
        {
            foreach (var pair in Arango.Collections)
            {
                var collectionNode = new TreeViewItem {
                    id = idAllocator.NextId(),
                    displayName = pair.Key
                };

                foreach (var document in pair.Value)
                {
                    var entityNode = new EntityItem(document, idAllocator);
                    collectionNode.AddChild(entityNode);
                }
                
                AddChild(collectionNode);
            }
        }
    }
}
