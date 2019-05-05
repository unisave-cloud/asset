using System;

namespace Unisave.Framework
{
    [System.Serializable]
    public class UnisaveException : System.Exception
    {
        public UnisaveException() { }
        public UnisaveException(string message) : base(message) { }
        public UnisaveException(string message, System.Exception inner) : base(message, inner) { }
        protected UnisaveException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
