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

		private string localDatabase = "";

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

			GUILayout.Label("Debugging", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox(
				"Use this for development of scenes that require a logged-in player " +
				"or whenever you don't want to make network calls to the Unisave servers.",
				MessageType.Info
			);
			prefs.runAgainstLocalDatabase = EditorGUILayout.Toggle("Run against local database", prefs.runAgainstLocalDatabase);
			if (prefs.runAgainstLocalDatabase)
			{
				prefs.loginOnStart = EditorGUILayout.Toggle("Auto-login", prefs.loginOnStart);
				
				if (prefs.loginOnStart)
				{
					prefs.loginOnStartEmail = EditorGUILayout.TextField("With email", prefs.loginOnStartEmail);
				}
			}

			GUILayout.Label("Local database", EditorStyles.boldLabel);
			if (GUILayout.Button("Clear database"))
			{
				PlayerPrefs.DeleteKey("unisave-local-database:" + prefs.localDatabaseName);
				PlayerPrefs.Save();
				localDatabase = "{}";
			}
			EditorGUILayout.LabelField(localDatabase, GUILayout.ExpandHeight(true));
		}

		void OnFocus()
		{
			localDatabase = PlayerPrefs.GetString("unisave-local-database:" + prefs.localDatabaseName, "{}");
			localDatabase = LightJson.Serialization.JsonReader.Parse(localDatabase).ToString(true);
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
