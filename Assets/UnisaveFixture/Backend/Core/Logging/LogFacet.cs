using System;
using Unisave.Facades;
using Unisave.Facets;
using UnityEngine;

namespace AcceptanceTests.Backend.Logging
{
    public class LogFacet : Facet
    {
        public void LogInfo()
        {
            Log.Info("Hello world!", 42);
        }
        
        public void LogWarning()
        {
            Log.Warning("Hello world!", 42);
        }
        
        public void LogError()
        {
            Log.Error("Hello world!", 42);
        }
        
        public void LogCritical()
        {
            Log.Critical("Hello world!", 42);
        }

        public void DebugLog()
        {
            Debug.Log("Hello world!");
        }
        
        public void DebugLogWarning()
        {
            Debug.LogWarning("Hello world!");
        }
        
        public void DebugLogError()
        {
            Debug.LogError("Hello world!");
        }
        
        public void DebugLogException()
        {
            try
            {
                throw new Exception("Some exception.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}