using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unisave.Framework;
using LightJson;

namespace Unisave.Framework.Endpoints
{
    /// <summary>
    /// Here's where the action call ends-up on the server side
    /// </summary>
    public class CallActionEndpoint
    {
        private IFrameworkBase frameworkBase;

        public CallActionEndpoint(IFrameworkBase frameworkBase)
        {
            this.frameworkBase = frameworkBase;
        }

        /// <summary>
        /// Call action based on the data that was received in the request
        /// </summary>
        public void CallAction(string controller, string action, string playerID, JsonArray arguments)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Call action when we know the controller type and we have all arguments deserialized
        /// </summary>
        public void CallAction(Type controllerType, string action, Player caller, object[] arguments)
        {
            MethodInfo mi = controllerType.GetMethod(action);

            if (mi == null)
            {
                throw new ArgumentException(
                    "Provided controller " + controllerType + " lacks method: " + action,
                    nameof(controllerType)
                );
            }

            Controller ctrl = Controller.CreateInstance(controllerType, caller);

            mi.Invoke(ctrl, arguments);
        }
    }
}
