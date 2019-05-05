using System;
using System.Reflection;

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
        /// Identifying name of the controller for remote calls
        /// </summary>
        public string Name => this.GetType().Name;

        /// <summary>
        /// Forward action calls to a remote target if not null
        /// </summary>
        private RemoteControllerActionCallDelegate remoteCallTarget = null;

        public delegate void RemoteControllerActionCallDelegate(
            Type controller, string action, object[] arguments
        );

        /// <summary>
        /// Creates new instance of a controller
        /// </summary>
        public static T CreateInstance<T>(Player currentPlayer) where T : Controller, new()
        {
            return (T) CreateInstance(typeof(T), currentPlayer);
        }

        public static Controller CreateInstance(Type controllerType, Player currentPlayer)
        {
            if (!typeof(Controller).IsAssignableFrom(controllerType))
                throw new ArgumentException(
                    "Provided type does not inherit from controller type.",
                    nameof(controllerType)
                );

            ConstructorInfo ci = controllerType.GetConstructor(new Type[] {});

            if (ci == null)
            {
                throw new ArgumentException(
                    "Provided controller type " + controllerType + " lacks empty constructor.",
                    nameof(controllerType)
                );
            }

            Controller controller = (Controller)ci.Invoke(new object[] {});
            
            controller.CurrentPlayer = currentPlayer;

            return controller;
        }

        /// <summary>
        /// Creates a remote controller that redirects all action calls to a remote target
        /// </summary>
        public static T CreateRemoteControllerInstance<T>(RemoteControllerActionCallDelegate target)
            where T : Controller, new()
        {
            T controller = new T();

            controller.remoteCallTarget = target;

            return controller;
        }

        // Intercepts an action call
        // return true to exit from the called action
        protected bool CallAction(string actionName, object[] arguments)
        {
            // forward the call to a remote target
            if (remoteCallTarget != null)
            {
                remoteCallTarget(this.GetType(), actionName, arguments);
                return true;
            }

            return false;
        }
    }
}
