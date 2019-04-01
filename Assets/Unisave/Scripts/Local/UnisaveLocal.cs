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
		/// The underlying manager instance
		/// </summary>
		private static LocalManager manager = LocalManager.CreateDefaultInstance();

		/// <summary>
		/// Loads marked fields from local storage
		/// </summary>
		/// <param name="target">Your script, containing marked fields</param>
		public static void Load(object target)
		{
			manager.Load(target);
		}

		/// <summary>
		/// Saves marked fields to local storage
		/// </summary>
		/// <param name="target">Your script, containing marked fields</param>
		public static void Save(object target)
		{
			manager.Save(target);
		}

		/// <summary>
		/// Saves all data in all scripts that have been loaded at some point
		/// </summary>
		public static void Save()
		{
			manager.Save();
		}
	}
}
