using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unisave;
using Unisave.Uniarchy;
using Unisave.Database;
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
				
				var database = ((DatabaseItem)selection).Database;

				ReadOnlyField("Name", database.Name);

				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("Clear"))
					database.Clear(true);
				if (GUILayout.Button("Delete"))
					EmulatedDatabaseRepository.GetInstance().DeleteDatabase(database.Name);
				EditorGUILayout.EndHorizontal();
			}

			if (selection is EntityItem)
			{
				DrawInspectorTitle("Entity");

				var entity = ((EntityItem)selection).GetEntity();

				ReadOnlyField("ID", entity.id);
				ReadOnlyField("Type", entity.type);
				ReadOnlyField("Owners", string.Join(", ", entity.ownerIds));
				ReadOnlyField("Created", Serializer.ToJson(entity.createdAt).AsString);
				ReadOnlyField("Updated", Serializer.ToJson(entity.updatedAt).AsString);

				if (jsonEditor == null)
				{
					jsonEditor = new JsonEditor();
					jsonEditor.SetValue(entity.data);
				}
				
				jsonEditor.OnGUI();
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
