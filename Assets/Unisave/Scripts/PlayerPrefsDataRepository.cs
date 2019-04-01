using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightJson;
using LightJson.Serialization;

namespace Unisave
{
	/// <summary>
	/// Repository that stores the data immediately in player preferences
	/// </summary>
	public class PlayerPrefsDataRepository : IDataRepository
	{
		/// <summary>
		/// Prefix for keys in the player prefs
		/// </summary>
		public string Prefix { get; private set; }

		/// <summary>
		/// If true, the data is saved immediately after setting a single key
		/// </summary>
		public bool SaveImmediately { get; set; }

		/// <summary>
		/// Creates a repository instance that stores the data inside PlayerPrefs
		/// </summary>
		/// <param name="prefix">Prefix keys in PlayerPrefs</param>
		public PlayerPrefsDataRepository(string prefix)
		{
			this.Prefix = prefix;
		}

		public JsonValue Get(string key)
		{
			return JsonReader.Parse(PlayerPrefs.GetString(Prefix + key, "null"));
		}

		public void Set(string key, JsonValue value)
		{
			PlayerPrefs.SetString(Prefix + key, value.ToString());

			if (SaveImmediately)
				PlayerPrefs.Save();
		}

		/// <summary>
		/// Explicitly save player prefs now
		/// </summary>
		public void SavePrefs()
		{
			PlayerPrefs.Save();
		}

		public void Remove(string key)
		{
			PlayerPrefs.DeleteKey(Prefix + key);
		}

		public bool Has(string key)
		{
			return PlayerPrefs.HasKey(Prefix + key);
		}
	}
}
