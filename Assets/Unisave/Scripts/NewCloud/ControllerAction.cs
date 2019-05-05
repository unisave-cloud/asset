using System;
using Unisave.Framework;

namespace Unisave
{
    /// <summary>
    /// Facade for calling actions on server controller
    /// </summary>
    public static class ControllerAction
    {
        public static T On<T>() where T : Controller, new()
        {
            return Controller.CreateInstance<T>(UnisaveCloud.Player);
        }
    }
}
