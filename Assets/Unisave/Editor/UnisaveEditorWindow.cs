using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unisave.Editor.BackendFolders;
using Unisave.Editor.BackendUploading;
using Unisave.Foundation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Application = UnityEngine.Application;

namespace Unisave.Editor
{
	[Obsolete("Replaced by the UnisaveWindow namespace")]
	public class UnisaveEditorWindow : EditorWindow
	{
		// === Edited state ===
		
		private UnisavePreferences preferences;
		private BackendFolderDefinition[] backendFolders;

		// === GUI fields (GUI state, textures, styles) ===
		
		private Texture unisaveLogo;
		private Vector2 windowScroll = Vector2.zero;
		private bool backendFoldersExpanded = true;
		private bool disabledBackendFoldersExpanded = false;

		[MenuItem("Window/Unisave/Preferences", false, 1)]
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
			titleContent.image = AssetDatabase.LoadAssetAtPath<Texture>(
				EditorGUIUtility.isProSkin ?
				"Assets/Unisave/Images/WindowIconWhite.png" :
				"Assets/Unisave/Images/WindowIcon.png"
			);
		}

		void OnFocus()
		{
			// force the file to reload by forgetting it
			// (loading happens inside OnGUI)
			preferences = null;
			backendFolders = null;
		}

		// called by unity, when keyboard focus is lost
		// BUT ALSO by this window when mouse leaves the window
		void OnLostFocus()
		{
			if (preferences != null)
				preferences.Save();
		}

		void OnGUI()
		{
			if (preferences == null)
				preferences = UnisavePreferences.LoadOrCreate();

			if (backendFolders == null)
				backendFolders = BackendFolderDefinition.LoadAll();

			windowScroll = GUILayout.BeginScrollView(windowScroll);

			UnisaveLogoBlock();
			ServerConnectionBlock();
			GUILayout.Space(15f);
			BackendUploadingBlock();
			GUILayout.Space(15f);
			BackendFoldersBlock();
			GUILayout.Space(30f);
			GUILayout.Label("Changes to configuration are saved automatically.");
			GUILayout.Space(30f);
			EditorGUILayout.LabelField("Framework version", FrameworkMeta.Version);
			EditorGUILayout.LabelField("Asset version", AssetMeta.Version);
			
			GUILayout.EndScrollView();

			// detect mouse leave
			if (Event.current.type == EventType.MouseLeaveWindow)
				OnLostFocus();
		}
		
		
		//////////////////////
		// Interface Blocks //
		//////////////////////

		void UnisaveLogoBlock()
		{
			const float margin = 10f;
			const float maxWidth = 400f;

			if (unisaveLogo == null)
			{
				unisaveLogo = AssetDatabase.LoadAssetAtPath<Texture>(
					EditorGUIUtility.isProSkin ?
					"Assets/Unisave/Images/PropertiesLogoWhite.png" :
					"Assets/Unisave/Images/PropertiesLogo.png"
				);
			}

			float width = Mathf.Min(position.width, maxWidth) - 2 * margin;
			float height = width * (unisaveLogo.height / (float)unisaveLogo.width);

			GUI.DrawTexture(
				new Rect((position.width - width) / 2, margin, width, height),
				unisaveLogo
			);
			GUILayout.Space(height + 2 * margin);
		}

		void ServerConnectionBlock()
		{
			GUILayout.Label("Unisave server connection", EditorStyles.boldLabel);
			preferences.ServerUrl = EditorGUILayout.TextField("Server URL", preferences.ServerUrl);
			preferences.GameToken = EditorGUILayout.TextField("Game token", preferences.GameToken);
			preferences.EditorKey = EditorGUILayout.TextField("Editor key", preferences.EditorKey);
		}

		void BackendUploadingBlock()
		{
			GUILayout.Label("Backend uploading", EditorStyles.boldLabel);
			// preferences.BackendFolder = EditorGUILayout.TextField("Backend assets folder", preferences.BackendFolder);
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
			
//			GUILayout.Label("Environment configuration", EditorStyles.boldLabel);
//			preferences.DevelopmentEnv = (TextAsset) EditorGUILayout.ObjectField(
//				"Development", preferences.DevelopmentEnv, typeof(TextAsset), false
//			);
//			preferences.TestingEnv = (TextAsset) EditorGUILayout.ObjectField(
//				"Testing", preferences.TestingEnv, typeof(TextAsset), false
//			);
		}

		void BackendFoldersBlock()
		{
			const float itemInset = 20f;

			GUIStyle linkStyle = new GUIStyle(EditorStyles.linkLabel);
			linkStyle.hover.textColor = EditorStyles.label.normal.textColor;

			List<BackendFolderDefinition> enabledBackendFolders
				= backendFolders.Where(f => f.IsEligibleForUpload()).ToList();
			List<BackendFolderDefinition> disabledBackendFolders
				= backendFolders.Where(f => !f.IsEligibleForUpload()).ToList();
		
			// === Enabled backend folders ===
			
			backendFoldersExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(
				backendFoldersExpanded,
				$"Backend folders ({enabledBackendFolders.Count})"
			);
			
			if (backendFoldersExpanded)
			{
				if (enabledBackendFolders.Count == 0)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(itemInset);
					GUILayout.Label("none");
					EditorGUILayout.EndHorizontal();
				}
				
				foreach (var f in enabledBackendFolders)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(itemInset);
					GUILayout.Label(f.FolderPath);
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(itemInset);
					
					EditorGUI.BeginDisabledGroup(f.UploadBehaviour != UploadBehaviour.Always);
					if (GUILayout.Button("Disable", GUILayout.Width(60f)))
						DisableBackendFolder(f);
					EditorGUI.EndDisabledGroup();

					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.ObjectField(f, typeof(BackendFolderDefinition), false);
					EditorGUI.EndDisabledGroup();
					
					EditorGUILayout.EndHorizontal();
					
					GUILayout.Space(5f);
				}

				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(itemInset);
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Add Existing Folder...", GUILayout.Width(170f)))
					AddExistingBackendFolder();
				GUILayout.Space(itemInset);
				EditorGUILayout.EndHorizontal();
			}
			
			EditorGUILayout.EndFoldoutHeaderGroup();
			
			// === Disabled backend folders ===
			
			GUILayout.Space(15f);

			disabledBackendFoldersExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(
				disabledBackendFoldersExpanded,
				$"Disabled backend folders ({disabledBackendFolders.Count})"
			);

			if (disabledBackendFoldersExpanded)
			{
				if (disabledBackendFolders.Count == 0)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(itemInset);
					GUILayout.Label("none");
					EditorGUILayout.EndHorizontal();
				}
				
				foreach (var f in disabledBackendFolders)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(itemInset);
					GUILayout.Label(f.FolderPath);
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(itemInset);
					
					EditorGUI.BeginDisabledGroup(f.UploadBehaviour != UploadBehaviour.Never);
					if (GUILayout.Button("Enable", GUILayout.Width(60f)))
						EnableBackendFolder(f);
					EditorGUI.EndDisabledGroup();
					
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.ObjectField(f, typeof(BackendFolderDefinition), false);
					EditorGUI.EndDisabledGroup();
					
					EditorGUILayout.EndHorizontal();
					
					GUILayout.Space(5f);
				}
			}
			
			EditorGUILayout.EndFoldoutHeaderGroup();
		}
		
		
		////////////////////
		// Action Methods //
		////////////////////
		
		void RunManualCodeUpload()
		{
			Uploader
				.GetDefaultInstance()
				.UploadBackend(
					verbose: true,
					useAnotherThread: true // yes, here we can run in background
				);
		}

		void EnableBackendFolder(BackendFolderDefinition def)
		{
			def.UploadBehaviour = UploadBehaviour.Always;
			EditorUtility.SetDirty(def);
			
			HighlightBackendFolderInInspector(def);
		}
		
		void DisableBackendFolder(BackendFolderDefinition def)
		{
			def.UploadBehaviour = UploadBehaviour.Never;
			EditorUtility.SetDirty(def);
			
			HighlightBackendFolderInInspector(def);
		}

		void HighlightBackendFolderInInspector(BackendFolderDefinition def)
		{
			Selection.activeObject = def;
		}

		void AddExistingBackendFolder()
		{
			// display select folder dialog
			string selectedPath = EditorUtility.OpenFolderPanel(
				"Add Existing Backend Folder", "Assets", ""
			);

			// action cancelled
			if (string.IsNullOrEmpty(selectedPath))
				return;
			
			// get path inside the assets folder
			string assetsPath = Path.GetFullPath("Assets/");
			if (selectedPath.StartsWith(assetsPath))
			{
				selectedPath = "Assets/" + selectedPath.Substring(assetsPath.Length);
			}
			else
			{
				EditorUtility.DisplayDialog(
					title: "Action failed",
					message: "Selected folder is not inside the Assets " +
					         "folder of this Unity project. It cannot be added.",
					ok: "OK"
				);
				return;
			}
			
			BackendFolderUtility.CreateDefinitionFileInFolder(selectedPath);
		}
	}
}
