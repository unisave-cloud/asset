using System;
using System.Reflection;

namespace Unisave.Framework
{
    /// <summary>
    /// Holds the framework base to use, when using static facades of the framework
    /// </summary>
    public static class StaticBase
    {
        private static IFrameworkBase baseInstance;

        public static IFrameworkBase Base
        {
            get
            {
                if (baseInstance == null)
                    throw new UnisaveException("Requesting framework staic base, but no instance was set.");

                return baseInstance;
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                baseInstance = value;
            }
        }
    }
}
