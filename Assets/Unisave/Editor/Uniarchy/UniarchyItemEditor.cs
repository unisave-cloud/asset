using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unisave;
using Unisave.Uniarchy;
using UnityEditor.IMGUI.Controls;
using Unisave.Serialization;
using Unisave.Editor.JsonEditor;

namespace Unsiave.Uniarchy
{
    [CustomEditor(typeof(SelectionWrapper), false)]
    public class UniarchyItemEditor : Editor
    {
		private JsonEditor jsonEditor;

        private void OnEnable()
        {
            // this.target
        }

        private void OnDisable()
        {
            
        }

        public override void OnInspectorGUI()
        {
            if (this.target == null)
            {
                Debug.Log("Returning!");
                return;
            }

            TreeViewItem selection = (TreeViewItem)((SelectionWrapper)this.target).content;

			if (selection is DatabaseItem)
			{
				DrawInspectorTitle("Database");
				
				var arango = ((DatabaseItem)selection).Arango;

				ReadOnlyField("Name", "TODO Arango name??"); //arango.Name); // TODO

				EditorGUILayout.BeginHorizontal();
				// TODO
//				if (GUILayout.Button("Clear"))
//					arango.Clear(true);
//				if (GUILayout.Button("Delete"))
//					EmulatedDatabaseRepository.GetInstance().DeleteDatabase(arango.Name);
				EditorGUILayout.EndHorizontal();
			}

			if (selection is EntityItem)
			{
				DrawInspectorTitle("Entity");

				var entity = ((EntityItem)selection).GetEntity();

				ReadOnlyField("ID", entity["_id"]);
				ReadOnlyField("Type", entity["$type"]);
				ReadOnlyField("Created", Serializer.ToJson(entity["CreatedAt"]).AsString);
				ReadOnlyField("Updated", Serializer.ToJson(entity["UpdatedAt"]).AsString);

				if (jsonEditor == null)
				{
					jsonEditor = new JsonEditor();
					jsonEditor.SetValue(entity);
				}
				
				jsonEditor.OnGUI();
				
				DrawInspectorTitle("READ ONLY!\nchanges cannot be saved yet\nplanned feature");
			}
        }

        void ReadOnlyField(string label, string content)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
			EditorGUILayout.SelectableLabel(content, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
			EditorGUILayout.EndHorizontal();
		}

		void DrawInspectorTitle(string text)
		{
			GUILayout.Label(
				text,
				new GUIStyle {
					fontSize = 18,
					fontStyle = FontStyle.Bold,
					alignment = TextAnchor.MiddleCenter
				}
			);
		}
    }
}
