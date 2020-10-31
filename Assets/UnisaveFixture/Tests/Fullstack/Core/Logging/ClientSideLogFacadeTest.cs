using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unisave.Facades;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnisaveFixture.Tests.Core.Logging
{
    public class ClientSideLogFacadeTest
    {
        [Test]
        public void ItCanLogInfo()
        {
            // TODO: also log context and make it properly formatted
            // e.g.: [CLIENT.INFO] Lorem ipsum \n Context: 42
            
            LogAssert.Expect(
                LogType.Log,
                new Regex(
                    @"Hello world!",
                    RegexOptions.Multiline
                )
            );
            
            Log.Info("Hello world!", 42);
        }
    }
}