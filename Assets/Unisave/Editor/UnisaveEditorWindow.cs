using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Unisave
{
	public class UnisaveEditorWindow : EditorWindow
	{
		private UnisavePreferences preferences;

		private string serverApiUrl;
		private string gameToken;

		[MenuItem("Window/Unisave")]
		public static void ShowWidnow()
		{
			EditorWindow.GetWindow(typeof(UnisaveEditorWindow));
		}

		void Awake()
		{
			GetPreferences();
			ReadPreferences();
		}

		void OnGUI()
		{
			GUILayout.Label("Cool label", EditorStyles.boldLabel);
			serverApiUrl = EditorGUILayout.TextField("Server API URL", serverApiUrl);
			gameToken = EditorGUILayout.TextField("Game token", gameToken);
		}

		void OnLostFocus()
		{
			WritePreferences();
		}

		// Preferences //

		void GetPreferences()
		{
			var path = "Assets/Unisave/Resources/";
			var objectName = "UnisavePreferencesInstance.asset";
	
			preferences = AssetDatabase.LoadAssetAtPath<UnisavePreferences>(path + objectName);
	
			if (preferences == null)
			{
				Debug.Log("Couldn't find unisave preferences. Creating...");
				preferences = CreateInstance<UnisavePreferences>();
				AssetDatabase.CreateAsset(preferences, path + objectName);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
		}

		void ReadPreferences()
		{
			if (preferences == null)
			{
				Debug.LogWarning("Reading from unisave preferences, but the reference is null.");
				return;
			}

			serverApiUrl = preferences.serverApiUrl;
			gameToken = preferences.gameToken;
		}

		void WritePreferences()
		{
			if (preferences == null)
			{
				Debug.LogWarning("Writing to unisave preferences, but the reference is null.");
				return;
			}

			preferences.serverApiUrl = serverApiUrl;
			preferences.gameToken = gameToken;

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}
}
