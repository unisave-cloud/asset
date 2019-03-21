using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightJson;
using LightJson.Serialization;

namespace Unisave.Serialization
{
	/// <summary>
	/// Loads JSON into C# type instances
	/// </summary>
	public class Loader
	{
		public static T Load<T>(string json)
		{
			object instance = Load(json, typeof(T));
			
			if (instance == null)
				return default(T);
			else
				return (T)instance;
		}

		public static object Load(string json, Type type)
		{
			return Load(JsonReader.Parse(json), type);
		}

		/// <summary>
		/// Loads the json into a new instance of the given type recursively
		/// If the json has incorrect format for the type, null, zero or false is returned
		/// </summary>
		public static object Load(JsonValue json, Type type)
		{
			// primitives
			if (type == typeof(long))
				return (long)json.AsInteger;
			else if (type == typeof(int))
				return json.AsInteger;
			else if (type == typeof(short))
				return (short)json.AsInteger;
			else if (type == typeof(byte))
				return (byte)json.AsInteger;
			else if (type == typeof(bool))
				return json.AsBoolean;
			else if (type == typeof(double))
				return json.AsNumber;
			else if (type == typeof(float))
				return (float)json.AsNumber;
			else if (type == typeof(string))
				return json.AsString;

			// not a primitive, may be null
			if (json.IsNull)
				return null;

			// unity math stuff
			if (
				type == typeof(Vector2) ||
				type == typeof(Vector3) ||
				type == typeof(Vector2Int) ||
				type == typeof(Vector3Int)
			)
				return JsonUtility.FromJson(json.ToString(), type);

			// arrays
			if (type.IsArray)
			{
				//type.GetElementType();
				throw new UnisaveException("Arrays cannot be loaded yet. Planned feature.");
			}

			if (type.IsGenericType)
			{
				// lists
				if (type.GetGenericTypeDefinition() == typeof(List<>))
					return LoadList(json, type);

				// dictionaries
				if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
					return LoadDictionary(json, type);
			}

			// custom classes
			return LoadCustomClass(json, type);
		}

		private static object LoadList(JsonValue json, Type type)
		{
			Type itemType = type.GetGenericArguments()[0];

			object list = type.GetConstructor(new Type[] {}).Invoke(new object[] {});

			JsonArray jsonArray = json.AsJsonArray;
			if (jsonArray == null)
				return null;

			foreach (JsonValue item in jsonArray)
				type.GetMethod("Add").Invoke(list, new object[] {
					Load(item, itemType)
				});

			return list;
		}

		private static object LoadDictionary(JsonValue json, Type type)
		{
			Type[] typeArguments = type.GetGenericArguments();
			Type keyType = typeArguments[0];
			Type valueType = typeArguments[0];
			
			if (keyType != typeof(string))
				throw new UnisaveException("Dictionaries with non-string keys are not supported.");

			object dictionary = type.GetConstructor(new Type[] {}).Invoke(new object[] {});

			JsonObject jsonObject = json.AsJsonObject;
			if (jsonObject == null)
				return null;

			foreach (KeyValuePair<string, JsonValue> item in jsonObject)
				type.GetMethod("Add").Invoke(dictionary, new object[] {
					item.Key,
					Load(item.Value, valueType)
				});

			return dictionary;
		}

		private static object LoadCustomClass(JsonValue json, Type type)
		{
			JsonObject jsonObject = json.AsJsonObject;
			if (jsonObject == null)
				return null;

			ConstructorInfo ci = type.GetConstructor(new Type[] {});
			if (ci == null)
			{
				Debug.LogWarning("Loading as null, since the type "
					+ type.ToString() + " is missing a public parameterless constructor.");
				return null;
			}

			object instance = ci.Invoke(new object[] {});

			// set public non-static fields
			foreach (FieldInfo fi in type.GetFields())
			{
				if (fi.IsPublic && !fi.IsStatic)
				{
					fi.SetValue(instance, Load(jsonObject[fi.Name], fi.FieldType));
				}
			}

			return instance;
		}
	}
}
