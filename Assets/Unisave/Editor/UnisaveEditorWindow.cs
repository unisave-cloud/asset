using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Unisave
{
	public class UnisaveEditorWindow : EditorWindow
	{
		/// <summary>
		/// Reference to the preferences file
		/// </summary>
		private UnisavePreferences preferences;

		private Texture unisaveLogo;

		[MenuItem("Window/Unisave/Preferences")]
		public static void ShowWindow()
		{
			EditorWindow.GetWindow(
				typeof(UnisaveEditorWindow),
				false,
				"Unisave"
			);
		}

		void OnGUI()
		{
			if (preferences == null)
				preferences = UnisavePreferences.LoadOrCreate();

			DrawUnisaveLogo();

			GUILayout.Label("Unisave server connection", EditorStyles.boldLabel);
			preferences.ServerUrl = EditorGUILayout.TextField("Server URL", preferences.ServerUrl);
			preferences.GameToken = EditorGUILayout.TextField("Game token", preferences.GameToken);
			preferences.EditorKey = EditorGUILayout.TextField("Editor key", preferences.EditorKey);

			GUILayout.Label("Development", EditorStyles.boldLabel);
			preferences.BackendFolder = EditorGUILayout.TextField("Backend assets folder", preferences.BackendFolder);
			preferences.EmulatedDatabaseName = EditorGUILayout.TextField("Emulated database name", preferences.EmulatedDatabaseName);

			GUILayout.Space(30f);

			GUILayout.Label("Changes to configuration are saved automatically.");
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
