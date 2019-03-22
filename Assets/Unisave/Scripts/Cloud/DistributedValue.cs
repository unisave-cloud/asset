using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unisave
{
	/// <summary>
	/// Holds a weak reference to a distributed value inside a mono behaviour
	/// Used for value collection before saving
	/// </summary>
	internal class DistributedValue
	{
		/// <summary>
		/// Weak reference to the mono behaviour
		/// </summary>
		public WeakReference behaviour;

		/// <summary>
		/// Description of the target field
		/// </summary>
		public FieldInfo fieldInfo;

		/// <summary>
		/// Key, under which the value is stored
		/// </summary>
		public string key;
	}
}
