using System.Collections.Generic;
using Unisave.Arango;
using Unisave.Editor.DataWindow.SelectionWrappers;
using Unisave.Editor.DataWindow.TreeItems;
using Unisave.Foundation;
using Unisave.Sessions;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Unisave.Editor.DataWindow
{
    class DataWindowTreeView : TreeView
    {
        public static TreeViewItem SelectedItem { get; private set; }

        public DataWindowTreeView(TreeViewState treeViewState) : base(treeViewState)
        {
            Reload();
        }
        
        protected override TreeViewItem BuildRoot()
        {
            var idAllocator = new IdAllocator();

            var root = new TreeViewItem {
                id = 0,
                depth = -1,
                displayName = "Root"
            };

            root.AddChild(new EmulatedDatabasesItem(
                ClientApplication.GetInstance().Resolve<ArangoRepository>(),
                idAllocator
            ));
            
            root.AddChild(new SessionIdItem(
                ClientApplication.GetInstance().Resolve<SessionIdRepository>(),
                idAllocator
            ));

            SetupDepthsFromParentsAndChildren(root);

            return root;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds.Count == 0 || selectedIds.Count > 1)
            {
                SelectedItem = null;
                return;
            }

            SelectedItem = FindItem(selectedIds[0], rootItem);
            
            switch (SelectedItem)
            {
                case DatabaseItem item:
                    DatabaseSelectionWrapper.Select(item);
                    break;
                
                case CollectionItem item:
                    CollectionSelectionWrapper.Select(item);
                    break;
                
                case DocumentItem item:
                    DocumentSelectionWrapper.Select(item);
                    break;
                
                default:
                    Selection.SetActiveObjectWithContext(null, null);
                    break;
            }
        }
    }
}
