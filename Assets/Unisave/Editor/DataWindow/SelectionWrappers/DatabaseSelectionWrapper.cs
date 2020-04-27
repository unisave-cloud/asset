using Unisave.Editor.DataWindow.TreeItems;
using UnityEditor;
using UnityEngine;

namespace Unisave.Editor.DataWindow.SelectionWrappers
{
    public class DatabaseSelectionWrapper : ScriptableObject
    {
        public DatabaseItem TreeItem { get; private set; }
        
        public static DatabaseSelectionWrapper Create(DatabaseItem treeItem)
        {
            var wrapper = ScriptableObject
                .CreateInstance<DatabaseSelectionWrapper>();

            wrapper.name = treeItem.DatabaseName;
            wrapper.TreeItem = treeItem;

            return wrapper;
        }

        public static void Select(DatabaseItem treeItem)
        {
            Selection.SetActiveObjectWithContext(
                Create(treeItem),
                null
            );
        }
    }
}