using System;
using System.Collections;
using System.Collections.Generic;

namespace Unisave
{
	/// <summary>
	/// Marked public field becomes persisted on a storage under the provided key.
	/// Marked field participates both in loading and saving.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class SavedAsAttribute : Attribute
	{
		public string Key { get; private set; }

		public SavedAsAttribute(string key)
		{
			this.Key = key;
		}
	}
}
