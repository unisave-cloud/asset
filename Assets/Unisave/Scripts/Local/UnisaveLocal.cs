using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unisave.Serialization;
using LightJson.Serialization;

namespace Unisave
{
	/// <summary>
	/// Provides data storage locally
	/// </summary>
	public static class UnisaveLocal
	{
		/// <summary>
		/// Prefix for keys in PlayerPrefs
		/// </summary>
		public const string PlayerPrefsKeyPrefix = "unisave:";

		/// <summary>
		/// Loads marked fields from local storage
		/// </summary>
		/// <param name="behaviour">Your script, containing marked fields</param>
		public static void Load(MonoBehaviour behaviour)
		{
			ReflectionHelper.WriteFields(behaviour, (key, set) => {
				if (PlayerPrefs.HasKey(PlayerPrefsKeyPrefix + key))
					set(
						JsonReader.Parse(PlayerPrefs.GetString(PlayerPrefsKeyPrefix + key))
					);
			});
		}

		/// <summary>
		/// Saves marked fields to local storage
		/// </summary>
		/// <param name="behaviour">Your script, containing marked fields</param>
		public static void Save(MonoBehaviour behaviour)
		{
			foreach (var pair in ReflectionHelper.ReadFields(behaviour))
			{
				PlayerPrefs.SetString(
					PlayerPrefsKeyPrefix + pair.Key,
					pair.Value.ToString()
				);
			}

			PlayerPrefs.Save();
		}
	}
}
