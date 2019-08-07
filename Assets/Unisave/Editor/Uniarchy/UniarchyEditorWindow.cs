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
                    - main
                        - players XXX
                        - entities XXX
                    - motorbike example db
                        - players  NOPE use an intuitive structure:
                                gameentities, player -> playerentities, sharedentitites
                        - entities XXX
                - development database
                - database backups
                    - my-cool-backup
                    - other backup
             */

            treeView.OnGUI(new Rect(0, 0, position.width, position.height));

            /*var s = Unisave.Serialization.Saver.Save(
                Unisave.UnisaveServer.DefaultInstance.EmulatedDatabase.entities.Values.ToList()
            ).ToString(true);

            s += "\n\n" + Unisave.UnisaveServer.DefaultInstance.EmulatedDatabase.DatabaseName;

            GUI.Label(new Rect(0, 0, position.width, position.height), "DB: \n" + s);*/
        }
    }
}
