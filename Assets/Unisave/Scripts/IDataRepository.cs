using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightJson;

namespace Unisave
{
	/// <summary>
	/// Represents a place, where key --> JsonValue data can be stored
	/// </summary>
	public interface IDataRepository
	{
		/// <summary>
		/// Returns the value under a given key or null if no such key exists
		/// Null however can actually be stored. Differentiate by the "has" method
		/// </summary>
		JsonValue Get(string key);

		/// <summary>
		/// Sets the value for a given key
		/// </summary>
		void Set(string key, JsonValue value);

		/// <summary>
		/// Remove a key from the repository
		/// </summary>
		void Remove(string key);

		/// <summary>
		/// Returns true if the repository contains given key
		/// </summary>
		bool Has(string key);
	}
}
