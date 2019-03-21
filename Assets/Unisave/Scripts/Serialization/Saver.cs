using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightJson;
using LightJson.Serialization;

namespace Unisave.Serialization
{
	public static class Saver
	{
		/// <summary>
		/// Saves a given object into json recursively
		/// Unknown types will be saved as null
		/// </summary>
		public static JsonValue Save(object instance)
		{
			if (instance == null)
				return JsonValue.Null;

			Type type = instance.GetType();

			// primitives
			if (type == typeof(long))
				return (int)instance;
			else if (type == typeof(int))
				return (int)instance;
			else if (type == typeof(short))
				return (int)instance;
			else if (type == typeof(byte))
				return (int)instance;
			else if (type == typeof(bool))
				return (bool)instance;
			else if (type == typeof(double))
				return (double)instance;
			else if (type == typeof(float))
				return (float)instance;
			else if (type == typeof(string))
				return (string)instance;

			// arrays
			if (type.IsArray)
			{
				//type.GetElementType();
				throw new UnisaveException("Arrays cannot be saved yet. Planned feature.");
			}

			if (type.IsGenericType)
			{
				// lists
				if (type.GetGenericTypeDefinition() == typeof(List<>))
					return SaveList(instance);

				// dictionaries
				if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
					return SaveDictionary(instance, type);
			}

			// custom classes
			return SaveCustomClass(instance, type);
		}

		private static JsonValue SaveList(object instance)
		{
			JsonArray jsonArray = new JsonArray();

			IList list = (IList)instance;
			
			foreach (object item in list)
				jsonArray.Add(Save(item));

			return jsonArray;
		}

		private static JsonValue SaveDictionary(object instance, Type type)
		{
			Type keyType = type.GetGenericArguments()[0];

			if (keyType != typeof(string))
				throw new UnisaveException("Non-string key dictionaries not supported yet.");

			JsonObject jsonObject = new JsonObject();

			IDictionary dictionary = (IDictionary)instance;

			foreach (KeyValuePair<object, object> pair in dictionary)
				jsonObject.Add((string)pair.Key, Save(pair.Value));

			return jsonObject;
		}

		private static JsonValue SaveCustomClass(object instance, Type type)
		{
			JsonObject jsonObject = new JsonObject();

			// get public non-static fields
			foreach (FieldInfo fi in type.GetFields())
			{
				if (fi.IsPublic && !fi.IsStatic)
				{
					jsonObject.Add(fi.Name, Save(fi.GetValue(instance)));
				}
			}

			return jsonObject;
		}
	}
}
