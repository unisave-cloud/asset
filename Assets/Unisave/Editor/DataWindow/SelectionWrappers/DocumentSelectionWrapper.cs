using Unisave.Editor.DataWindow.TreeItems;
using UnityEditor;
using UnityEngine;

namespace Unisave.Editor.DataWindow.SelectionWrappers
{
    public class DocumentSelectionWrapper : ScriptableObject
    {
        public DocumentItem TreeItem { get; private set; }
        
        public static DocumentSelectionWrapper Create(DocumentItem treeItem)
        {
            var wrapper = ScriptableObject
                .CreateInstance<DocumentSelectionWrapper>();

            wrapper.name = treeItem.DocumentId;
            wrapper.TreeItem = treeItem;

            return wrapper;
        }

        public static void Select(DocumentItem treeItem)
        {
            Selection.SetActiveObjectWithContext(
                Create(treeItem),
                null
            );
        }
    }
}