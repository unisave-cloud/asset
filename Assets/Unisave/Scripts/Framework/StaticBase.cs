using System;
using System.Reflection;

namespace Unisave.Framework
{
    /// <summary>
    /// Holds the framework base to use, when using static facades of the framework
    /// </summary>
    public static class StaticBase
    {
        public static IFrameworkBase Base { get; set; } = new PunishingFrameworkBase();

        /// <summary>
        /// Override the base value during execution of some method
        /// </summary>
        public static void OverrideBase(IFrameworkBase newBase, Action action)
        {
            IFrameworkBase oldBase = Base;

            Base = newBase;
            action.Invoke();

            Base = oldBase;
        }
    }
}
