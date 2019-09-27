using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unisave.Serialization;
using LightJson;
using LightJson.Serialization;
using UnityEngine.Serialization;

namespace Unisave
{
	/// <summary>
	/// Holds all preferences of Unisave
	/// </summary>
	public class UnisavePreferences : ScriptableObject
	{
		/// <summary>
		/// Name of the preferences asset inside a Resources folder (without extension)
		/// </summary>
		public const string PreferencesFileName = "UnisavePreferencesFile";

		/// <summary>
		/// Loads preferences from the file
		/// </summary>
		public static UnisavePreferences LoadOrCreate()
		{
			// try to load
			var preferences = Resources.Load<UnisavePreferences>(PreferencesFileName);

			// load failed, create them instead (if inside editor)
			if (preferences == null)
			{
				#if UNITY_EDITOR
					var path = "Assets/Unisave/Resources/" + PreferencesFileName + ".asset";

					preferences = ScriptableObject.CreateInstance<UnisavePreferences>();
					UnityEditor.AssetDatabase.CreateAsset(preferences, path);
					UnityEditor.AssetDatabase.SaveAssets();
					UnityEditor.AssetDatabase.Refresh();
				#else
					throw new InvalidOperationException(
						"Unisave preferences have not been found. " +
						"Make sure you configure unisave before building your game."
					);
				#endif
			}

			return preferences;
		}

		/// <summary>
		/// Saves preferences. Callable only from inside the editor
		/// </summary>
		public void Save()
		{
			#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(this);
				UnityEditor.AssetDatabase.SaveAssets();
				UnityEditor.AssetDatabase.Refresh();

				// note that editor key is saved continuously
			#else
				throw new InvalidOperationException(
					"You can save Unsiave preferences only when running inside the editor."
				);
			#endif
		}

		/////////////////
		// Preferences //
		/////////////////
		
		/// <summary>
		/// URL of the Unisave server
		/// </summary>
		public string ServerUrl
		{
			get => serverUrl;
			set => serverUrl = value;
		}
		
		[SerializeField]
		private string serverUrl = "https://unisave.cloud/";

		/// <summary>
		/// Token that uniquely identifies your game
		/// </summary>
		public string GameToken
		{
			get => gameToken;
			set => gameToken = value;
		}

		[SerializeField]
		private string gameToken;

		/// <summary>
		/// Returns a prefix for keys stored inside editor prefs
		/// That makes sure editor prefs are not shared between projects
		/// </summary>
		private string KeySuffix => gameToken ?? "null";

		/// <summary>
		/// Authentication key for Unity editor. This is to make sure noone else
		/// who knows the game token can mess with your game.
		/// 
		/// The editor is actually not stored inside preferences file, but in EditorPrefs
		/// instead to prevent accidental leakage by releasing your game.
		/// </summary>
		public string EditorKey
		{
			get
			{
				#if UNITY_EDITOR
					if (!editorKeyCacheActive)
					{
						editorKeyCache = UnityEditor.EditorPrefs.GetString(
							"unisave.editorKey:" + KeySuffix, null
						);
						editorKeyCacheActive = true;
					}
					return editorKeyCache;
				#else
					return null;
				#endif
			}

			set
			{
				#if UNITY_EDITOR
					if (value == EditorKey)
						return;

					UnityEditor.EditorPrefs.SetString(
						"unisave.editorKey:" + KeySuffix, value
					);
					
					editorKeyCache = value;
					editorKeyCacheActive = true;
				#else
					throw new InvalidOperationException(
						"You cannot access editor key during runtime."
					);
				#endif
			}
		}
		
		#if UNITY_EDITOR
			
			[NonSerialized]
			private string editorKeyCache;

			[NonSerialized]
			private bool editorKeyCacheActive;
		
		#endif

		/// <summary>
		/// Path (relative to the assets folder) to directory that contains
		/// backend related files, like facets, entities and config
		/// 
		/// Contents of this folder are uploaded to the server
		/// </summary>
		public string BackendFolder
		{
			get => backendFolder;
			set => backendFolder = value;
		}

		[SerializeField]
		private string backendFolder = "Backend";

		/// <summary>
		/// Upload backend automatically after compilation finishes
		/// </summary>
		public bool AutomaticBackendUploading
		{
			get => automaticBackendUploading;
			set => automaticBackendUploading = value;
		}

		[SerializeField]
		private bool automaticBackendUploading = true;

		/// <summary>
		/// Last time backend uploading took place
		/// </summary>
		public DateTime? LastBackendUploadAt
		{
			get
			{
				#if UNITY_EDITOR
					var data = UnityEditor.EditorPrefs.GetString(
						"unisave.lastBackendUploadAt:" + KeySuffix, null
					);

					if (String.IsNullOrEmpty(data))
						return null;

					return Serializer.FromJsonString<DateTime>(data);
				#else
					return null;
				#endif
			}

			set
			{
				#if UNITY_EDITOR
					UnityEditor.EditorPrefs.SetString(
						"unisave.lastBackendUploadAt:" + KeySuffix,
						Serializer.ToJson(value).ToString()
					);
				#endif
			}
		}
		
		/// <summary>
		/// Hash of the backend folder
		/// Important, it's used to identify clients
		/// </summary>
		public string BackendHash
		{
			get => backendHash;
			set => backendHash = value;
		}

		[SerializeField]
		private string backendHash;

		/// <summary>
		/// Name of the emulated database to use
		/// </summary>
		public string EmulatedDatabaseName
		{
			get => emulatedDatabaseName;
			set => emulatedDatabaseName = value;
		}
		
		[SerializeField]
		private string emulatedDatabaseName = "main";

		/// <summary>
		/// Emulate even if the scene does not require it
		/// </summary>
		public bool AlwaysEmulate
		{
			get => alwaysEmulate;
			set => alwaysEmulate = value;
		}
		
		[SerializeField]
		private bool alwaysEmulate;

		/// <summary>
		/// Email of the player used for autologin
		/// </summary>
		public string AutoLoginPlayerEmail
		{
			get => autoLoginPlayerEmail;
			set => autoLoginPlayerEmail = value;
		}

		[SerializeField]
		private string autoLoginPlayerEmail = "john@doe.com";

		/// <summary>
		/// Automatically register a player when not present to be logged in
		/// </summary>
		public bool AutoRegisterPlayer
		{
			get => autoRegisterPlayer;
			set => autoRegisterPlayer = value;
		}

		[SerializeField]
		private bool autoRegisterPlayer = true;

		/// <summary>
		/// Arguments for the automatic registration
		/// </summary>
		public JsonObject AutoRegisterArguments
		{
			get => JsonReader.Parse(autoRegisterArguments);
			set => autoRegisterArguments = value.ToString();
		}

		[SerializeField]
		private string autoRegisterArguments = @"{""name"":""John""}";
	}
}
