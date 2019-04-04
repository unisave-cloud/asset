using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
		public const string ResourceFileName = "UnisavePreferencesFile";

		/// <summary>
		/// Loads preferences from the file
		/// </summary>
		public static UnisavePreferences LoadPreferences()
		{
			var preferences = Resources.Load<UnisavePreferences>(ResourceFileName);

			if (preferences == null)
			{
				// return default preferences
				preferences = ScriptableObject.CreateInstance<UnisavePreferences>();
				Debug.LogWarning("Unisave preferences not found. Server connection will not work.");
			}

			return preferences;
		}

		/////////////////
		// Preferences //
		/////////////////
		
		public string serverApiUrl = "https://unisave.cloud/api/game/v1.0/";

		public string gameToken;

		public string editorKey;

		public string localDebugPlayerEmail = "local";
	}
}
