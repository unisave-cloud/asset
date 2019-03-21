using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unisave.Serialization
{
	public static class Serializer
	{
		public static T Load<T>(string json)
		{
			return (T)Load(json, typeof(T));
		}

		public static object Load(string json, Type type)
		{
			return LoadSimpleType(json, type);
		}

		public static string Save(object obj)
		{
			if (obj == null)
				return "null";

			Type type = obj.GetType();

			return SaveSimpleType(obj, type);
		}

		// Implementation details ==========================================

		private struct StringWrapper { public string s; }

		private static object LoadSimpleType(string json, Type type)
		{
			if (type == typeof(int))
				return int.Parse(json);
			if (type == typeof(float))
				return float.Parse(json);
			if (type == typeof(string))
				return JsonUtility.FromJson<StringWrapper>("{\"s\":" + json + "}").s;

			return JsonUtility.FromJson(json, type);
		}

		// not generic, nor array
		private static string SaveSimpleType(object obj, Type type)
		{
			if (type == typeof(int))
				return obj.ToString();
			if (type == typeof(float))
				return obj.ToString();
			if (type == typeof(string))
			{
				string wrapped = JsonUtility.ToJson(new StringWrapper() { s = (string)obj });
				return wrapped.Substring(5, wrapped.Length - 6);
			}

			return JsonUtility.ToJson(obj);
		}
	}
}
