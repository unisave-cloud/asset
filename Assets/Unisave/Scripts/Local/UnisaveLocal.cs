using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unisave;
using Unisave.Serialization;

public static class UnisaveLocal
{
	/// <summary>
	/// Prefix for keys in PlayerPrefs
	/// </summary>
	private const string PLAYER_PREFS_KEY_PREFIX = "unisave:";

	/// <summary>
	/// Loads marked fields from local storage
	/// </summary>
	/// <param name="behaviour">Your script, containing marked fields</param>
	public static void Load(MonoBehaviour behaviour)
	{
		foreach (FieldInfo fieldInfo in behaviour.GetType().GetFields())
		{
			object[] attributes = fieldInfo.GetCustomAttributes(typeof(SavedAsAttribute), false);
			
			if (attributes.Length == 0)
				continue;

			SavedAsAttribute savedAs = (SavedAsAttribute)attributes[0];

			string key = PLAYER_PREFS_KEY_PREFIX + savedAs.Key;
			
			if (!PlayerPrefs.HasKey(key))
				continue;
			
			string json = PlayerPrefs.GetString(key);
			object val = Loader.Load(json, fieldInfo.FieldType);
			fieldInfo.SetValue(behaviour, val);
		}
	}

	/// <summary>
	/// Saves marked fields to local storage
	/// </summary>
	/// <param name="behaviour">Your script, containing marked fields</param>
	public static void Save(MonoBehaviour behaviour)
	{
		foreach (FieldInfo fieldInfo in behaviour.GetType().GetFields())
		{
			object[] attributes = fieldInfo.GetCustomAttributes(typeof(SavedAsAttribute), false);
			
			if (attributes.Length == 0)
				continue;

			SavedAsAttribute savedAs = (SavedAsAttribute)attributes[0];

			string key = PLAYER_PREFS_KEY_PREFIX + savedAs.Key;
			string json = Saver.Save(fieldInfo.GetValue(behaviour));
			PlayerPrefs.SetString(key, json);
		}

		PlayerPrefs.Save();
	}
}
