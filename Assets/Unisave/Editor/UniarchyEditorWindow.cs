using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Linq;

namespace Unisave
{
    public class UniarchyEditorWindow : EditorWindow
    {
        [SerializeField]
        TreeViewState treeViewState;

        SimpleTreeView treeView;

        [MenuItem("Window/Unisave/Uniarchy")]
		public static void ShowWindow()
		{
			EditorWindow.GetWindow(
				typeof(UniarchyEditorWindow),
				false,
				"Uniarchy"
			);
		}

        void OnEnable()
        {
            if (treeViewState == null)
                treeViewState = new TreeViewState();

            treeView = new SimpleTreeView(treeViewState);
        }

        void OnGUI()
        {
            /*
                - client entity cache
                    - MotorbikeEntity (yamaha)
                - emulated databases
                    - main
                        - players
                        - entities
                    - motorbike example db
                        - players
                        - entities
                - testing database (if exists)
                - database backups
                    - my-cool-backup
                    - other backup
             */


            //treeView.OnGUI(new Rect(0, 0, position.width, position.height));

            var s = Unisave.Serialization.Saver.Save(
                Unisave.UnisaveServer.DefaultInstance.TestingDatabase.entities.Values.ToList()
            ).ToString(true);

            s += "\n\n" + Unisave.UnisaveServer.DefaultInstance.EmulatedDatabase.DatabaseName;

            GUI.Label(new Rect(0, 0, position.width, position.height), "DB: \n" + s);
        }

        class SimpleTreeView : TreeView
        {
            public SimpleTreeView(TreeViewState treeViewState)
                : base(treeViewState)
            {
                Reload();
            }
            
            protected override TreeViewItem BuildRoot()
            {
                // BuildRoot is called every time Reload is called to ensure that TreeViewItems 
                // are created from data. Here we create a fixed set of items. In a real world example,
                // a data model should be passed into the TreeView and the items created from the model.

                // This section illustrates that IDs should be unique. The root item is required to 
                // have a depth of -1, and the rest of the items increment from that.
                var root = new TreeViewItem {id = 0, depth = -1, displayName = "Root"};
                var allItems = new List<TreeViewItem> 
                {
                    new TreeViewItem {id = 1, depth = 0, displayName = "Animals"},
                    new TreeViewItem {id = 2, depth = 1, displayName = "Mammals"},
                    new TreeViewItem {id = 3, depth = 2, displayName = "Tiger"},
                    new TreeViewItem {id = 4, depth = 2, displayName = "Elephant"},
                    new TreeViewItem {id = 5, depth = 2, displayName = "Okapi"},
                    new TreeViewItem {id = 6, depth = 2, displayName = "Armadillo"},
                    new TreeViewItem {id = 7, depth = 1, displayName = "Reptiles"},
                    new TreeViewItem {id = 8, depth = 2, displayName = "Crocodile"},
                    new TreeViewItem {id = 9, depth = 2, displayName = "Lizard"},
                };
                
                // Utility method that initializes the TreeViewItem.children and .parent for all items.
                SetupParentsAndChildrenFromDepths (root, allItems);
                    
                // Return root of the tree
                return root;
            }
        }
    }
}
