using Unisave.Editor.DataWindow.TreeItems;
using UnityEditor;
using UnityEngine;

namespace Unisave.Editor.DataWindow.SelectionWrappers
{
    public class CollectionSelectionWrapper : ScriptableObject
    {
        public CollectionItem TreeItem { get; private set; }
        
        public static CollectionSelectionWrapper Create(CollectionItem treeItem)
        {
            var wrapper = ScriptableObject
                .CreateInstance<CollectionSelectionWrapper>();

            wrapper.name = treeItem.CollectionName;
            wrapper.TreeItem = treeItem;

            return wrapper;
        }

        public static void Select(CollectionItem treeItem)
        {
            Selection.SetActiveObjectWithContext(
                Create(treeItem),
                null
            );
        }
    }
}