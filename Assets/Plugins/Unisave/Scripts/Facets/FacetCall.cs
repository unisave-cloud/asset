using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Unisave.Facets
{
    /// <summary>
    /// Represents a facet call that returns some value. It can either
    /// return a value or throw an exception.
    /// </summary>
    public class FacetCall<TReturn> : IEnumerator
    {
        /// <summary>
        /// The MonoBehaviour script that invoked the request,
        /// may be null for requests invoked outside Unity code
        /// </summary>
        private readonly MonoBehaviour caller;
        
        /// <summary>
        /// The facet call request in the application-level API
        /// </summary>
        private readonly Task<object> applicationLevelTask;

        /// <summary>
        /// True when the request finishes (returns or throws an exception)
        /// </summary>
        public bool IsDone { get; private set; }

        /// <summary>
        /// If the request finished successfully, the result will be here
        /// </summary>
        public TReturn Result { get; private set; }
        
        /// <summary>
        /// If the request resulted in an exception, it will be here
        /// </summary>
        public Exception Exception { get; private set; }

        public FacetCall(
            MonoBehaviour caller,
            Task<object> applicationLevelTask
        )
        {
            this.caller = caller;
            this.applicationLevelTask = applicationLevelTask;
            
            applicationLevelTask.ContinueWith(task => {
                try
                {
                    OnApplicationLevelTaskCompleted();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Called when the given application-level task completes,
        /// run immediately if the task is already completed
        /// </summary>
        private void OnApplicationLevelTaskCompleted()
        {
            TaskStatus status = applicationLevelTask.Status;

            if (status == TaskStatus.RanToCompletion)
            {
                Result = (TReturn) applicationLevelTask.Result;
                IsDone = true;
                
                InvokeThenCallback();
                CompleteExternalTask();
                
                return;
            }

            if (status == TaskStatus.Faulted)
            {
                // unwraps AggregateException if there is just one
                Exception = applicationLevelTask.Exception?.GetBaseException();
                IsDone = true;
                
                InvokeCatchCallback();
                FaultExternalTask();
                
                // nobody to catch the exception --> log it
                if (catchCallback == null && externalTcs == null)
                    Debug.LogException(Exception);
                
                return;
            }
            
            Debug.LogError(
                $"[Unisave] A facet call finished, but the task " +
                $"status was unexpected: {status}"
            );
        }

        /// <summary>
        /// If true, the request finalization should be ignored.
        /// </summary>
        private bool IsCallerDisabled()
        {
            // if there was no caller, then finalization always happens
            if (caller == null)
                return false;
            
            // if the game object isn't active in the hierarchy,
            // the caller is disabled
            if (!caller.gameObject.activeInHierarchy)
                return true;
            
            // if the component itself is disabled,
            // the caller is disabled
            if (!caller.enabled)
                return true;
            
            // otherwise the caller is enabled
            return false;
        }
        
        
        //////////////////
        // Callback API //
        //////////////////

        /// <summary>
        /// Function to be called on success
        /// </summary>
        private Action<TReturn> thenCallback;
        
        /// <summary>
        /// Function to be called on exception
        /// </summary>
        private Action<Exception> catchCallback;
        
        private void InvokeThenCallback()
        {
            // do nothing if the caller is disabled
            if (IsCallerDisabled())
                return;
            
            if (thenCallback == null)
                return;

            try
            {
                thenCallback.Invoke(Result);
            }
            catch (Exception callbackException)
            {
                Debug.LogException(callbackException);
            }
        }

        private void InvokeCatchCallback()
        {
            // do nothing if the caller is disabled
            if (IsCallerDisabled())
                return;
            
            if (catchCallback == null)
                return;

            try
            {
                catchCallback.Invoke(Exception);
            }
            catch (Exception callbackException)
            {
                Debug.LogException(callbackException);
            }
        }

        /// <summary>
        /// Register a function that will be called when the request succeeds
        /// </summary>
        public FacetCall<TReturn> Then(Action<TReturn> callback)
        {
            if (callback == null)
                return this;
            
            // allow only one callback
            if (thenCallback != null)
            {
                throw new InvalidOperationException(
                    $"You can only register one {nameof(Then)} callback. " +
                    $"For complicated use cases use the async-await API."
                );
            }
            
            // remember the callback
            thenCallback = callback;
            
            // invoke if already done
            if (IsDone && Exception == null)
                InvokeThenCallback();
            
            // chainable API
            return this;
        }

        /// <summary>
        /// Register a function that will be called when the request fails
        /// </summary>
        public FacetCall<TReturn> Catch(
            Action<Exception> callback
        )
        {
            if (callback == null)
                return this;
            
            // allow only one callback
            if (catchCallback != null)
            {
                throw new InvalidOperationException(
                    $"You can only register one {nameof(Catch)} callback. " +
                    $"For complicated use cases use the async-await API."
                );
            }

            // remember the callback
            catchCallback = callback;
            
            // invoke if already done
            if (IsDone && Exception != null)
                InvokeCatchCallback();
            
            // chainable API
            return this;
        }
        
        
        ///////////////////
        // Coroutine API //
        ///////////////////
        
        bool IEnumerator.MoveNext()
        {
            return !IsDone;
        }

        void IEnumerator.Reset() { }

        object IEnumerator.Current => !IsDone;
        
        
        ////////////////////
        // AsyncAwait API //
        ////////////////////

        /// <summary>
        /// Controls the task presented externally
        /// </summary>
        private TaskCompletionSource<TReturn> externalTcs;

        public Task<TReturn> Async()
        {
            if (externalTcs == null)
                externalTcs = new TaskCompletionSource<TReturn>();
            
            // complete if already done
            if (IsDone && Exception == null)
                CompleteExternalTask();
            
            // fault if already done
            if (IsDone && Exception != null)
                FaultExternalTask();

            return externalTcs.Task;
        }

        public TaskAwaiter<TReturn> GetAwaiter()
        {
            return Async().GetAwaiter();
        }

        private void CompleteExternalTask()
        {
            // do nothing if the caller is disabled
            if (IsCallerDisabled())
                return;

            // nothing to complete
            if (externalTcs == null)
                return;

            // invoke all awaiters
            externalTcs.TrySetResult(Result);
        }

        private void FaultExternalTask()
        {
            // do nothing if the caller is disabled
            if (IsCallerDisabled())
                return;
            
            // nothing to fault
            if (externalTcs == null)
                return;

            // invoke all awaiters
            externalTcs.TrySetException(Exception);
        }
    }
    
    
    ///////////////////////////////////////////
    // Void return type, non-generic variant //
    ///////////////////////////////////////////
    
    /// <summary>
    /// Represents a void-type facet call. It can either succeed or
    /// throw an exception.
    /// </summary>
    public class FacetCall : IEnumerator
    {
        private readonly FacetCall<object> innerRequest;

        /// <summary>
        /// True when the request finishes (returns or throws an exception)
        /// </summary>
        public bool IsDone => innerRequest.IsDone;

        /// <summary>
        /// If the request resulted in an exception, it will be here
        /// </summary>
        public Exception Exception => innerRequest.Exception;

        public FacetCall(
            MonoBehaviour caller,
            Task<object> applicationLevelTask
        )
        {
            innerRequest = new FacetCall<object>(caller, applicationLevelTask);
        }
        
        
        //////////////////
        // Callback API //
        //////////////////

        /// <summary>
        /// Register a function that will be called when the request succeeds
        /// </summary>
        public FacetCall Then(Action callback)
        {
            innerRequest.Then(_ => callback?.Invoke());
            return this;
        }

        /// <summary>
        /// Register a function that will be called when the request fails
        /// </summary>
        public FacetCall Catch(Action<Exception> callback)
        {
            innerRequest.Catch(callback);
            return this;
        }
        
        
        ///////////////////
        // Coroutine API //
        ///////////////////
        
        bool IEnumerator.MoveNext()
        {
            return !IsDone;
        }

        void IEnumerator.Reset() { }

        object IEnumerator.Current => !IsDone;
        
        
        ////////////////////
        // AsyncAwait API //
        ////////////////////

        public Task Async()
        {
            return innerRequest.Async();
        }

        public TaskAwaiter GetAwaiter()
        {
            return Async().GetAwaiter();
        }
    }
}