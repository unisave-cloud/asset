using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightJson;

namespace Unisave
{
	/// <summary>
	/// Repository that stores the data in memory in a dictionary
	/// </summary>
	public class InMemoryDataRepository : IDataRepository
	{
		private Dictionary<string, JsonValue> data = new Dictionary<string, JsonValue>();

		public JsonValue Get(string key)
		{
			JsonValue value = JsonValue.Null;
			data.TryGetValue(key, out value);
			return value;
		}

		public void Set(string key, JsonValue value)
		{
			data[key] = value;
		}

		public void Remove(string key)
		{
			data.Remove(key);
		}

		public bool Has(string key)
		{
			return data.ContainsKey(key);
		}

		public void Save()
		{
			// no saving needed
		}
	}
}
