using System;

namespace Unisave.Framework
{
    /// <summary>
    /// Controller is the server entrance point
    /// Game client calls actions, which are methods callable by the client
    /// </summary>
    public class Controller
    {
        public static T OfType<T>() where T : Controller, new()
        {
            return new T();
        }

        // return true to exit from the called action
        protected bool CallAction(string actionName, object[] arguments)
        {
            //UnityEngine.Debug.Log("CallAction method has been called!");

            return false;
        }
    }
}
