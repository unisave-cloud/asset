using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unisave.Serialization;

namespace Unisave
{
	public class UnisaveEditorWindow : EditorWindow
	{
		/// <summary>
		/// Reference to the preferences file
		/// </summary>
		private UnisavePreferences preferences;

		private Texture unisaveLogo;

		private JsonEditor autoRegistrationArguments = new JsonEditor();

		private readonly string frameworkVersion = typeof(Entity).Assembly.GetName().Version.ToString(3);

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

			GUILayout.Label("Code uploading", EditorStyles.boldLabel);
			preferences.BackendFolder = EditorGUILayout.TextField("Backend assets folder", preferences.BackendFolder);
			preferences.AutomaticCodeUploading = EditorGUILayout.Toggle("Automatic", preferences.AutomaticCodeUploading);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Manual", GUILayout.Width(EditorGUIUtility.labelWidth - 4));
			if (GUILayout.Button("Upload", GUILayout.Width(50f)))
				RunManualCodeUpload();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Last upload at", GUILayout.Width(EditorGUIUtility.labelWidth - 4));
			EditorGUILayout.LabelField(preferences.LastCodeUploadAt?.ToString("yyyy-MM-dd H:mm:ss") ?? "Never");
			EditorGUILayout.EndHorizontal();

			GUILayout.Label("Database emulation", EditorStyles.boldLabel);
			preferences.EmulatedDatabaseName = EditorGUILayout.TextField("Emulated database name", preferences.EmulatedDatabaseName);
			preferences.AlwaysEmulate = EditorGUILayout.Toggle("Always emulate", preferences.AlwaysEmulate);

			GUILayout.Label("Auto-login", EditorStyles.boldLabel);
			preferences.AutoLoginPlayerEmail = EditorGUILayout.TextField("Auto-login email", preferences.AutoLoginPlayerEmail);
			preferences.AutoRegisterPlayer = EditorGUILayout.Toggle("Auto-register", preferences.AutoRegisterPlayer);
			GUILayout.Label("Auto-registration arguments");
			preferences.AutoRegisterArguments = autoRegistrationArguments.OnGUI(preferences.AutoRegisterArguments);

			GUILayout.Space(30f);

			GUILayout.Label("Changes to configuration are saved automatically.");

			GUILayout.Space(30f);

			GUILayout.Label("Unisave asset version: " + UnisaveServer.AssetVersion);
			GUILayout.Label("Unisave framework version: " + frameworkVersion);

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

		void RunManualCodeUpload()
		{
			Debug.Log("CodeUploader: Starting the upload...");
			var uploader = CodeUploader.Uploader.CreateDefaultInstance();
			uploader.Run();
			Debug.Log("CodeUploader: Done.");
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
