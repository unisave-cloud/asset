using System;

namespace Unisave
{
	/// <summary>
	/// Parent call for all Unisave exceptions
	/// </summary>
	[Serializable]
	public class UnisaveException : Exception
	{
		public UnisaveException() { }
		public UnisaveException(string message) : base(message) { }
		public UnisaveException(string message, Exception inner) : base(message, inner) { }
		protected UnisaveException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
