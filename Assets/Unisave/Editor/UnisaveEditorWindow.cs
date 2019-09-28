using System.Collections;
using System.Collections.Generic;
using Unisave.Editor.BackendUploading;
using UnityEngine;
using UnityEditor;
using Unisave.Serialization;
using Unisave.Editor.JsonEditor;

namespace Unisave
{
	public class UnisaveEditorWindow : EditorWindow
	{
		/// <summary>
		/// Reference to the preferences file
		/// </summary>
		private UnisavePreferences preferences;

		private Texture unisaveLogo;

		// editor for autoreg arguments
		private JsonEditor autoRegistrationArguments;

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

		void OnFocus()
		{
			// force the file to reload by forgetting it
			// (loading happens inside OnGUI)
			preferences = null;
		}

		// called by unity, when keyboard focus is lost
		// BUT ALSO by this window when mouse leaves the window
		void OnLostFocus()
		{
			if (preferences != null)
			{
				BeforePreferencesSave();
				preferences.Save();
			}
		}

		/// <summary>
		/// Called when preferences get reloaded
		/// (usually on focus or creation)
		/// </summary>
		private void OnPrefencesLoaded()
		{
			// load autoreg arguments
			if (autoRegistrationArguments == null)
			{
				autoRegistrationArguments = new JsonEditor();

				autoRegistrationArguments.SetValue(preferences.AutoRegisterArguments);

				// save on change
				// (because focus loosing works in a strange way...)
				autoRegistrationArguments.OnChange += () => {
					if (preferences != null && autoRegistrationArguments != null)
						preferences.AutoRegisterArguments = autoRegistrationArguments.GetValue();
				};
			}
			else
			{
				autoRegistrationArguments.SetValue(preferences.AutoRegisterArguments);
			}
		}

		/// <summary>
		/// Called just before preferences get saved
		/// (usually on lost focus)
		/// </summary>
		private void BeforePreferencesSave()
		{
			// save autoreg arguments
			preferences.AutoRegisterArguments = autoRegistrationArguments.GetValue();
		}

		void OnGUI()
		{
			if (preferences == null)
			{
				preferences = UnisavePreferences.LoadOrCreate();
				OnPrefencesLoaded();
			}

			windowScroll = GUILayout.BeginScrollView(windowScroll);

			DrawUnisaveLogo();

			GUILayout.Label("Unisave server connection", EditorStyles.boldLabel);
			preferences.ServerUrl = EditorGUILayout.TextField("Server URL", preferences.ServerUrl);
			preferences.GameToken = EditorGUILayout.TextField("Game token", preferences.GameToken);
			preferences.EditorKey = EditorGUILayout.TextField("Editor key", preferences.EditorKey);

			GUILayout.Label("Backend folder uploading", EditorStyles.boldLabel);
			preferences.BackendFolder = EditorGUILayout.TextField("Backend assets folder", preferences.BackendFolder);
			preferences.AutomaticBackendUploading = EditorGUILayout.Toggle("Automatic", preferences.AutomaticBackendUploading);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Manual", GUILayout.Width(EditorGUIUtility.labelWidth - 4));
			if (GUILayout.Button("Upload", GUILayout.Width(50f)))
				RunManualCodeUpload();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Last upload at", GUILayout.Width(EditorGUIUtility.labelWidth - 4));
			EditorGUILayout.LabelField(preferences.LastBackendUploadAt?.ToString("yyyy-MM-dd H:mm:ss") ?? "Never");
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Backend hash", GUILayout.Width(EditorGUIUtility.labelWidth - 4));
			EditorGUILayout.LabelField(
				string.IsNullOrWhiteSpace(preferences.BackendHash)
					? "<not computed yet>"
					: preferences.BackendHash
			);
			EditorGUILayout.EndHorizontal();

			GUILayout.Label("Database emulation", EditorStyles.boldLabel);
			preferences.EmulatedDatabaseName = EditorGUILayout.TextField("Emulated database name", preferences.EmulatedDatabaseName);
			preferences.AlwaysEmulate = EditorGUILayout.Toggle("Always emulate", preferences.AlwaysEmulate);

			GUILayout.Label("Auto-login", EditorStyles.boldLabel);
			preferences.AutoLoginPlayerEmail = EditorGUILayout.TextField("Auto-login email", preferences.AutoLoginPlayerEmail);
			preferences.AutoRegisterPlayer = EditorGUILayout.Toggle("Auto-register", preferences.AutoRegisterPlayer);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Auto-reg arguments", GUILayout.Width(EditorGUIUtility.labelWidth - 4));
			autoRegistrationArguments.OnGUI();
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(30f);

			GUILayout.Label("Changes to configuration are saved automatically.");

			GUILayout.Space(30f);

			GUILayout.Label("Unisave asset version: " + AssetMeta.Version);
			GUILayout.Label("Unisave framework version: " + FrameworkMeta.Version);

			GUILayout.EndScrollView();

			// detect mouse leave
			if (Event.current.type == EventType.MouseLeaveWindow)
				OnLostFocus();
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
			Uploader
				.GetDefaultInstance()
				.Run(
					isEditor: true,
					verbose: true,
					useAnotherThread: true // yes, here we can run in background
				);
		}
	}
}
