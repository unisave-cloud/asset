using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Unisave.Examples.Tests
{
    public class NewsStreamTest
    {
        [UnityTest]
        public IEnumerator ItLoadsNews()
        {
            SceneManager.LoadScene("Unisave/Examples/NewsStream/NewsStream");
            yield return null;
            
            string text = GameObject
                .Find("NewsText")
                .GetComponent<Text>()
                .text;
            
            StringAssert.Contains("Welcome to an example news stream", text);
        }
    }
}
