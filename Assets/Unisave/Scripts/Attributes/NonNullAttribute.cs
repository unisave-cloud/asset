using System;
using System.Collections;
using System.Collections.Generic;

namespace Unisave
{
	/// <summary>
	/// Never load a null value into this field. The field has to
	/// have a non-null default value otherwise a warning is shown.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class NonNullAttribute : Attribute
	{
		public NonNullAttribute() { }
	}
}
