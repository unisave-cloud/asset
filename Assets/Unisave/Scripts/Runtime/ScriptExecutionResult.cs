using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightJson;
using Unisave.Exceptions;

namespace Unisave.Runtime
{
    public class ScriptExecutionResult
    {
        public enum ResultType
        {
            OK,
            Exception,
            InvalidMethodParameters,
            Error
        }

        public ResultType type;
        public JsonValue methodResponse;
        public Exception exception;
        public string message;
        public string errorMessage;

        /// <summary>
        /// Is the result type OK?
        /// </summary>
        public bool IsOK => type == ResultType.OK;

        public static ScriptExecutionResult Ok(JsonValue methodResponse)
        {
            var result = new ScriptExecutionResult();
            result.type = ResultType.OK;
            result.methodResponse = methodResponse;
            return result;
        }

        public static ScriptExecutionResult Exception(Exception e)
        {
            var result = new ScriptExecutionResult();
            result.type = ResultType.Exception;
            result.exception = e;
            return result;
        }

        public static ScriptExecutionResult InvalidMethodParameters(string message)
        {
            var result = new ScriptExecutionResult();
            result.type = ResultType.InvalidMethodParameters;
            result.message = message;
            return result;
        }

        public static ScriptExecutionResult Error(string errorMessage)
        {
            var result = new ScriptExecutionResult();
            result.type = ResultType.Error;
            result.errorMessage = errorMessage;
            return result;
        }

        /// <summary>
        /// If the result is not OK, then it will result in a promise exception
        /// This method will construct the exception to be returned
        /// </summary>
        public Exception TransformNonOkResultToFinalException()
        {
            switch (type)
            {
                case ResultType.Exception:
                    return exception;

                case ResultType.InvalidMethodParameters:
                    return new UnisaveException("Script execution problem: " + message);

                case ResultType.Error:
                    return new UnisaveException("Script execution problem: " + errorMessage);
            }

            return null;
        }
    }
}
