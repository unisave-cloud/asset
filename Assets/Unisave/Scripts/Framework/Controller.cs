using System;

namespace Unisave.Framework
{
    /// <summary>
    /// Controller is the server entrance point
    /// Game client calls actions, which are methods callable by the client
    /// </summary>
    public class Controller
    {
        /// <summary>
        /// Currently logged-in player.
        /// Always has a value when executing an action.
        /// </summary>
        protected Player CurrentPlayer { get; private set; }

        /// <summary>
        /// Creates new instance of a controller
        /// </summary>
        public static T CreateInstance<T>(Player currentPlayer) where T : Controller, new()
        {
            T controller = new T();

            controller.CurrentPlayer = currentPlayer;

            return controller;
        }

        // return true to exit from the called action
        protected bool CallAction(string actionName, object[] arguments)
        {
            //UnityEngine.Debug.Log("CallAction method has been called!");

            return false;
        }
    }
}
