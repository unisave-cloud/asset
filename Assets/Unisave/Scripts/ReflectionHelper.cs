using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightJson;
using Unisave.Serialization;

namespace Unisave
{
	/// <summary>
	/// Encapsulates iteration over marked fields
	/// </summary>
	internal static class ReflectionHelper
	{
		/// <summary>
		/// Returns all marked fields in a MonoBehaviour instance
		/// </summary>
		private static IEnumerable<KeyValuePair<string, FieldInfo>> IterateMarkedFields(
			MonoBehaviour behaviour
		)
		{
			foreach (FieldInfo fieldInfo in behaviour.GetType().GetFields())
			{
				if (!fieldInfo.IsPublic || fieldInfo.IsStatic)
					continue;

				object[] marks = fieldInfo.GetCustomAttributes(
					typeof(SavedAsAttribute),
					false
				);
				
				if (marks.Length == 0)
					continue;

				SavedAsAttribute savedAs = (SavedAsAttribute)marks[0];

				yield return new KeyValuePair<string, FieldInfo>(
					savedAs.Key, fieldInfo
				);
			}
		}

		/// <summary>
		/// Returns keys and json serialized values of all marked fields
		/// </summary>
		public static IEnumerable<KeyValuePair<string, JsonValue>> ReadFields(
			MonoBehaviour behaviour
		)
		{
			foreach (var pair in IterateMarkedFields(behaviour))
			{
				yield return new KeyValuePair<string, JsonValue>(
					pair.Key,
					Saver.Save(pair.Value.GetValue(behaviour))
				);
			}
		}

		/// <summary>
		/// Loads values into marked fields
		/// </summary>
		/// <param name="behaviour">Behaviour with marked fields</param>
		/// <param name="action">Gets a key and a "setValue" callback</param>
		public static void WriteFields(
			MonoBehaviour behaviour,
			Action<string, Action<JsonValue>> action
		)
		{
			foreach (var pair in IterateMarkedFields(behaviour))
			{
				action(
					pair.Key,
					(value) => { // set callback
						pair.Value.SetValue(
							behaviour,
							Loader.Load(value, pair.Value.FieldType)
						);
					}
				);
			}
		}
	}
}
