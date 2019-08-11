using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Linq;

namespace Unisave.Uniarchy
{
    public class UniarchyEditorWindow : EditorWindow
    {
        [SerializeField]
        private TreeViewState treeViewState;
        private UniarchyTreeView treeView;

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

            treeView = new UniarchyTreeView(treeViewState);
        }

        void OnGUI()
        {
            /*
                - client entity cache
                    - MotorbikeEntity (yamaha)
                - emulated databases
                    - ...
                - database backups (database snapshots rather)
                    - my-cool-backup
                    - other backup
             */

            if (treeView == null)
                OnEnable();

            treeView.OnGUI(new Rect(0, 0, position.width, position.height));
        }
    }
}
