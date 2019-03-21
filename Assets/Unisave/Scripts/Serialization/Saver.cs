using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightJson;
using LightJson.Serialization;

namespace Unisave.Serialization
{
	public static class Saver
	{
		public static string Save(object obj)
		{
			if (obj == null)
				return "null";

			Type type = obj.GetType();

			return SaveSimpleType(obj, type);
		}

		// Implementation details ==========================================

		private struct StringWrapper { public string s; }

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
