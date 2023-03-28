using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Unisave.Facades;
using UnityEngine;

namespace Unisave.Facets
{
    /// <summary>
    /// Allows you to call facets
    /// </summary>
    public static class FacetClient
    {
        private static IApplicationLayerFacetCaller Caller
            => new LegacyAdapter(
                ClientFacade.ClientApp.Resolve<FacetCaller>()
            );

        public static FacetCall CallFacet<TFacet>(
            Expression<Action<TFacet>> lambda
        ) where TFacet : Facet
        {
            return CallFacet(null, lambda);
        }
        
        public static FacetCall<TReturn> CallFacet<TFacet, TReturn>(
            Expression<Func<TFacet, TReturn>> lambda
        ) where TFacet : Facet
        {
            return CallFacet(null, lambda);
        }
        
        public static FacetCall CallFacet<TFacet>(
            this MonoBehaviour caller,
            Expression<Action<TFacet>> lambda
        ) where TFacet : Facet
        {
            ArgumentException lambdaException = ParseLambda(
                lambda,
                out MethodInfo method,
                out object[] arguments
            );

            if (lambdaException != null)
                throw lambdaException;

            Task<object> task = Caller.CallFacetMethodAsync(method, arguments);

            return new FacetCall(caller, task);
        }
        
        public static FacetCall<TReturn> CallFacet<TFacet, TReturn>(
            this MonoBehaviour caller,
            Expression<Func<TFacet, TReturn>> lambda
        ) where TFacet : Facet
        {
            ArgumentException lambdaException = ParseLambda(
                lambda,
                out MethodInfo method,
                out object[] arguments
            );

            if (lambdaException != null)
                throw lambdaException;

            Task<object> task = Caller.CallFacetMethodAsync(method, arguments);

            return new FacetCall<TReturn>(caller, task);
        }

        private static ArgumentException ParseLambda(
            LambdaExpression lambda,
            out MethodInfo method,
            out object[] arguments
        )
        {
            method = null;
            arguments = null;
            
            if (lambda.Parameters.Count != 1)
                return Invalid("It needs to have exactly 1 parameter.");

            ParameterExpression parameter = lambda.Parameters[0];

            if (!typeof(Facet).IsAssignableFrom(parameter.Type))
                return Invalid($"The parameter {parameter.Name} has to " +
                               $"be a {nameof(Facet)}");

            var callExpression = lambda.Body as MethodCallExpression;

            if (callExpression == null)
                return Invalid("The body is not a single facet method call.");

            if (callExpression.Object != parameter)
                return Invalid($"You need to call a method on " +
                               $"the {parameter.Name} parameter.");

            method = callExpression.Method;
            arguments = new object[callExpression.Arguments.Count];

            for (int i = 0; i < arguments.Length; i++)
            {
                var argumentLambda = Expression.Lambda(callExpression.Arguments[i]);
                var argumentDelegate = argumentLambda.Compile();
                arguments[i] = argumentDelegate.DynamicInvoke();
            }
            
            return null;
        }

        private static ArgumentException Invalid(string reason)
        {
            return new ArgumentException(
                $"The {nameof(CallFacet)} argument has to be a lambda " +
                $"expression that calls one facet method. The current lambda " +
                $"is invalid because: " + reason,
                
                // ReSharper disable once NotResolvedInText
                "lambda"
            );
        }
        
        /// <summary>
        /// Connects the new application-level facet calling API
        /// with the legacy FacetCaller API
        ///
        /// This is a temporary solution as a proper transport-level API
        /// should be implemented instead.
        /// </summary>
        private class LegacyAdapter : IApplicationLayerFacetCaller
        {
            private readonly FacetCaller facetCaller;
            
            public LegacyAdapter(FacetCaller facetCaller)
            {
                this.facetCaller = facetCaller;
            }

            public Task<object> CallFacetMethodAsync(
                MethodInfo method,
                object[] arguments
            )
            {
                var source = new TaskCompletionSource<object>();

                if (method.ReturnType == typeof(void))
                {
                    facetCaller.CallFacetMethod(
                        method.DeclaringType,
                        method.Name,
                        arguments
                    )
                        .Then(() => {
                            source.SetResult(null);
                        })
                        .Catch(e => {
                            source.SetException(e);
                        });
                }
                else
                {
                    facetCaller.CallFacetMethod(
                        method.DeclaringType,
                        method.ReturnType,
                        method.Name,
                        arguments
                    )
                        .Then(r => {
                            source.SetResult(r);
                        })
                        .Catch(e => {
                            source.SetException(e);
                        });
                }

                return source.Task;
            }
        }
    }
}