using System.Collections;
using System.Text.RegularExpressions;
using Unisave.Facades;
using UnisaveFixture.Backend.Core.Logging;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnisaveFixture.Tests.Core.Logging
{
    public class LoggingTest
    {
        [UnityTest]
        public IEnumerator InfoCanBeLogged()
        {
            LogAssert.Expect(
                LogType.Log,
                new Regex(
                    @"SERVER\.INFO.*Hello world![\s\S]*Context: 42",
                    RegexOptions.Multiline
                )
            );
            
            yield return OnFacet<LogFacet>.Call(
                nameof(LogFacet.LogInfo)
            ).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator WarningCanBeLogged()
        {
            LogAssert.Expect(
                LogType.Warning,
                new Regex(
                    @"SERVER\.WARNING.*Hello world![\s\S]*Context: 42",
                    RegexOptions.Multiline
                )
            );
            
            yield return OnFacet<LogFacet>.Call(
                nameof(LogFacet.LogWarning)
            ).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator ErrorCanBeLogged()
        {
            LogAssert.ignoreFailingMessages = true;
            
            LogAssert.Expect(
                LogType.Error,
                new Regex(
                    @"SERVER\.ERRO.*Hello world![\s\S]*Context: 42",
                    RegexOptions.Multiline
                )
            );
            
            yield return OnFacet<LogFacet>.Call(
                nameof(LogFacet.LogError)
            ).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator CriticalCanBeLogged()
        {
            LogAssert.ignoreFailingMessages = true;
            
            LogAssert.Expect(
                LogType.Error,
                new Regex(
                    @"SERVER\.CRITICAL.*Hello world![\s\S]*Context: 42",
                    RegexOptions.Multiline
                )
            );
            
            yield return OnFacet<LogFacet>.Call(
                nameof(LogFacet.LogCritical)
            ).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator DebugLogCanBeUsed()
        {
            LogAssert.Expect(
                LogType.Log,
                new Regex(
                    @"Hello world!",
                    RegexOptions.Multiline
                )
            );
            
            yield return OnFacet<LogFacet>.Call(
                nameof(LogFacet.DebugLog)
            ).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator DebugLogWarningCanBeUsed()
        {
            LogAssert.Expect(
                LogType.Warning,
                new Regex(
                    @"Hello world!",
                    RegexOptions.Multiline
                )
            );
            
            yield return OnFacet<LogFacet>.Call(
                nameof(LogFacet.DebugLogWarning)
            ).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator DebugLogErrorCanBeUsed()
        {
            LogAssert.ignoreFailingMessages = true;
            
            LogAssert.Expect(
                LogType.Error,
                new Regex(
                    @"Hello world!",
                    RegexOptions.Multiline
                )
            );
            
            yield return OnFacet<LogFacet>.Call(
                nameof(LogFacet.DebugLogError)
            ).AsCoroutine();
        }
        
        [UnityTest]
        public IEnumerator DebugLogExceptionCanBeUsed()
        {
            LogAssert.ignoreFailingMessages = true;
            
            LogAssert.Expect(
                LogType.Error,
                new Regex(
                    @"Some exception.",
                    RegexOptions.Multiline
                )
            );
            
            yield return OnFacet<LogFacet>.Call(
                nameof(LogFacet.DebugLogException)
            ).AsCoroutine();
        }
    }
}