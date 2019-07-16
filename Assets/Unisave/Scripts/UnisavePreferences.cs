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
		
		/// <summary>
		/// URL of the unisave server api
		/// </summary>
		public string serverApiUrl = "https://unisave.cloud/api/game/v1.0/";

		/// <summary>
		/// Token that uniquely identifies this game (and it's developer) to unisave servers
		/// </summary>
		public string gameToken;

		/// <summary>
		/// Path (relative to the assets folder) to directory that contains
		/// backend related files, like facets, entities and config
		/// 
		/// Contents of this folder are uploaded to the server
		/// </summary>
		public string backendFolder = "Backend";

		//
		// Obsolete stuff:
		//

		public string localDebugPlayerEmail = "local";

		// running locally:

		public bool runAgainstLocalDatabase = true;
		public string localDatabaseName = "main"; // name of the local database
		public bool loginOnStart = false; // only works for local, because only then it can be synchronous
		public string loginOnStartEmail = "local";
	}
}
