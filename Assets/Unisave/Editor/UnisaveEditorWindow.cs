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
		private UnisavePreferences prefs;

		private string editorKey;

		[MenuItem("Window/Unisave")]
		public static void ShowWidnow()
		{
			EditorWindow.GetWindow(
				typeof(UnisaveEditorWindow),
				false,
				"Unisave"
			);
		}

		void OnGUI()
		{
			if (prefs == null)
			{
				PreparePreferencesFile();

				editorKey = EditorPrefs.GetString("unisave.editorKey", null);
			}

			GUILayout.Label("Server connection parameters", EditorStyles.boldLabel);
			prefs.serverApiUrl = EditorGUILayout.TextField("Server API URL", prefs.serverApiUrl);
			prefs.gameToken = EditorGUILayout.TextField("Game token", prefs.gameToken);
			editorKey = EditorGUILayout.TextField("Editor key", editorKey);

			/*GUILayout.Label("Interesting player data", EditorStyles.boldLabel);
			EditorGUILayout.TextField("Username Key", "");
			GUILayout.Label("Maybe generalize to hell?");*/
		}

		void OnLostFocus()
		{
			SaveChanges();
		}

		/////////////////
		// Preferences //
		/////////////////

		/// <summary>
		/// Checks for existance and, if needed, creates the preferences file
		/// </summary>
		void PreparePreferencesFile()
		{
			var path = "Assets/Unisave/Resources/" + UnisavePreferences.ResourceFileName + ".asset";
	
			prefs = AssetDatabase.LoadAssetAtPath<UnisavePreferences>(path);
	
			if (prefs == null)
			{
				Debug.Log("Couldn't find unisave preferences. Creating...");
				prefs = CreateInstance<UnisavePreferences>();
				AssetDatabase.CreateAsset(prefs, path);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
		}

		void SaveChanges()
		{
			EditorPrefs.SetString("unisave.editorKey", editorKey);

			EditorUtility.SetDirty(prefs);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}
}
