using System.Collections;
using System.Text.RegularExpressions;
using AcceptanceTests.Backend.Logging;
using NUnit.Framework;
using Unisave.Facades;
using UnityEngine;
using UnityEngine.TestTools;

namespace AcceptanceTests
{
    public class LoggingTest
    {
        [UnityTest]
        public IEnumerator InfoCanBeLogged()
        {
            yield return OnFacet<LogFacet>.Call(
                nameof(LogFacet.LogInfo)
            ).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator WarningCanBeLogged()
        {
            yield return OnFacet<LogFacet>.Call(
                nameof(LogFacet.LogWarning)
            ).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator ErrorCanBeLogged()
        {
            LogAssert.ignoreFailingMessages = true;
            
            yield return OnFacet<LogFacet>.Call(
                nameof(LogFacet.LogError)
            ).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator CriticalCanBeLogged()
        {
            LogAssert.ignoreFailingMessages = true;
            
            yield return OnFacet<LogFacet>.Call(
                nameof(LogFacet.LogCritical)
            ).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator DebugLogCanBeUsed()
        {
            yield return OnFacet<LogFacet>.Call(
                nameof(LogFacet.DebugLog)
            ).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator DebugLogWarningCanBeUsed()
        {
            yield return OnFacet<LogFacet>.Call(
                nameof(LogFacet.DebugLogWarning)
            ).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator DebugLogErrorCanBeUsed()
        {
            LogAssert.ignoreFailingMessages = true;
            
            yield return OnFacet<LogFacet>.Call(
                nameof(LogFacet.DebugLogError)
            ).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator DebugLogExceptionCanBeUsed()
        {
            LogAssert.ignoreFailingMessages = true;
            
            yield return OnFacet<LogFacet>.Call(
                nameof(LogFacet.DebugLogException)
            ).AsCoroutine();
        }
    }
}