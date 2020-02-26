using System.Collections.Generic;
using Unisave.Arango;
using Unisave.Arango.Emulation;
using UnityEditor.IMGUI.Controls;

namespace Unisave.Uniarchy
{
    class UniarchyTreeView : TreeView
    {
        /// <summary>
        /// The selected item
        /// </summary>
        public static TreeViewItem SelectedItem { get; private set; }

        ArangoRepository databaseRepository;

        public UniarchyTreeView(TreeViewState treeViewState) : base(treeViewState)
        {
            databaseRepository = ArangoRepository.GetInstance();

            // reload on each db change
            databaseRepository.OnChange += Reload;

            Reload();
        }
        
        protected override TreeViewItem BuildRoot()
        {
            var idAllocator = new IdAllocator();

            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };

            var emulatedDatabases = new TreeViewItem { id = idAllocator.NextId(), displayName = "Emulated databases" };
            foreach (ArangoInMemory database in databaseRepository.EnumerateDatabases())
            {
                emulatedDatabases.AddChild(new DatabaseItem(database, idAllocator));
            }
            root.AddChild(emulatedDatabases);

            var developmentDatabase = new TreeViewItem { id = idAllocator.NextId(), displayName = "Development database" };
            developmentDatabase.AddChild(
                new TreeViewItem {
                    id = idAllocator.NextId(),
                    displayName = "Will be inspectable soon..."
                }
            );
            root.AddChild(developmentDatabase);

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

            SelectionWrapper.SelectObject(SelectedItem);
        }
    }
}
