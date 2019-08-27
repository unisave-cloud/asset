using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unisave.Database;
using Unisave.Uniarchy;

namespace Unisave
{
	public class UnisaveEditorWindow : EditorWindow
	{
		/// <summary>
		/// Reference to the preferences file
		/// </summary>
		private UnisavePreferences preferences;

		private Texture unisaveLogo;

		private Vector2 windowScroll = Vector3.zero;

		[MenuItem("Window/Unisave/Preferences")]
		public static void ShowWindow()
		{
			EditorWindow.GetWindow(
				typeof(UnisaveEditorWindow),
				false,
				"Unisave"
			);
		}

		void OnEnable()
		{
			UniarchyTreeView.OnSelectionChange += () => {
				Repaint();
			};
		}

		void OnGUI()
		{
			if (preferences == null)
				preferences = UnisavePreferences.LoadOrCreate();

			windowScroll = GUILayout.BeginScrollView(windowScroll);

			DrawUnisaveLogo();

			GUILayout.Label("Unisave server connection", EditorStyles.boldLabel);
			preferences.ServerUrl = EditorGUILayout.TextField("Server URL", preferences.ServerUrl);
			preferences.GameToken = EditorGUILayout.TextField("Game token", preferences.GameToken);
			preferences.EditorKey = EditorGUILayout.TextField("Editor key", preferences.EditorKey);

			GUILayout.Label("Development", EditorStyles.boldLabel);
			preferences.BackendFolder = EditorGUILayout.TextField("Backend assets folder", preferences.BackendFolder);
			preferences.EmulatedDatabaseName = EditorGUILayout.TextField("Emulated database name", preferences.EmulatedDatabaseName);
			preferences.AlwaysEmulate = EditorGUILayout.Toggle("Always emulate", preferences.AlwaysEmulate);
			preferences.AutoLoginPlayerEmail = EditorGUILayout.TextField("Auto-login email", preferences.AutoLoginPlayerEmail);

			GUILayout.Space(30f);

			GUILayout.Label("Changes to configuration are saved automatically.");

			GUILayout.Space(30f);

			DrawUnspector();

			GUILayout.EndScrollView();
		}

		void DrawUnisaveLogo()
		{
			const float height = 120f;
			const float margin = 10f;

			if (unisaveLogo == null)
				unisaveLogo = Resources.Load<Texture>("UnisaveLogo");

			GUI.DrawTexture(
				new Rect((position.width - height) / 2, margin, height, height),
				unisaveLogo
			);
			GUILayout.Space(height + 2 * margin);
		}

		void DrawUnspector()
		{
			var selection = UniarchyTreeView.SelectedItem;

			if (selection == null)
				return;

			if (selection is DatabaseItem)
			{
				DrawUnspectorTitle("Database");
				
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
				DrawUnspectorTitle("Entity");

				var entity = ((EntityItem)selection).GetEntity();

				ReadOnlyField("ID", entity.id);
				ReadOnlyField("Type", entity.type);

				GUILayout.Label(entity.ToJson().ToString(true));
			}
		}

		void ReadOnlyField(string label, string content)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
			EditorGUILayout.SelectableLabel(content, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
			EditorGUILayout.EndHorizontal();
		}

		void DrawUnspectorTitle(string text)
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

		void OnFocus()
		{
			// force the file to reload by forgetting it
			preferences = null;
		}

		void OnLostFocus()
		{
			if (preferences != null)
				preferences.Save();
		}
	}
}
