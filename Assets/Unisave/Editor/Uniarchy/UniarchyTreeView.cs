using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Unisave.Database;

namespace Unisave.Uniarchy
{
    class UniarchyTreeView : TreeView
    {
        EmulatedDatabaseRepository databaseRepository;

        public UniarchyTreeView(TreeViewState treeViewState) : base(treeViewState)
        {
            databaseRepository = EmulatedDatabaseRepository.GetInstance();

            // reload on each db change
            databaseRepository.OnChange += Reload;

            Reload();
        }
        
        protected override TreeViewItem BuildRoot()
        {
            var idAllocator = new IdAllocator();

            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };

            var emulatedDatabases = new TreeViewItem { id = idAllocator.NextId(), displayName = "Emulated databases" };
            foreach (EmulatedDatabase database in databaseRepository.EnumerateDatabases())
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
    }
}
