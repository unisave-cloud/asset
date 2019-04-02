using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightJson;

namespace Unisave
{
	/// <summary>
	/// Contains some operations with IDataRepository instances
	/// </summary>
	public static class DataRepositoryHelper
	{
		/// <summary>
		/// Insert items of a json object into the repo as individual records
		/// </summary>
		public static void InsertJsonObject(IDataRepository repository, JsonObject json)
		{
			if (json == null)
				return;

			foreach (KeyValuePair<string, JsonValue> pair in json)
				repository.Set(pair.Key, pair.Value);
		}

		/// <summary>
		/// Return the repo content as a json object
		/// </summary>
		public static JsonObject ToJsonObject(IDataRepository repository)
		{
			JsonObject json = new JsonObject();

			foreach (string key in repository.AllKeys())
				json.Add(key, repository.Get(key));

			return json;
		}

		/// <summary>
		/// Remove all items from the repo
		/// </summary>
		public static void Clear(IDataRepository repository)
		{
			// ToArray() to prevent modification while iterating (to cache keys here)
			foreach (string key in repository.AllKeys().ToArray())
				repository.Remove(key);
		}
	}
}
