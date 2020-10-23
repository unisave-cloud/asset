using System;
using System.Collections;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

namespace UnisaveFixture.Tests.Core.Broadcasting
{
    public class MyDownloadHandler : DownloadHandlerScript
    {
        protected override void ReceiveContentLength(int contentLength)
        {
            Debug.Log("Content length: " + contentLength);
            
            base.ReceiveContentLength(contentLength);
        }

        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            Debug.Log("Receive data: " + Encoding.UTF8.GetString(data));
            
            return base.ReceiveData(data, dataLength);
        }
    }
    
    public class BroadcastingTest
    {
        [Test]
        public void DoodleSimpleTest()
        {
            var socket = new ClientWebSocket();
            socket.ConnectAsync(new Uri("wss://..."), CancellationToken.None);
            //socket.SendAsync(null, )
            
            //
        }

        [UnityTest]
        public IEnumerator DoodleEnumeratorTest()
        {
            using (var request = new UnityWebRequest(
                    "http://localhost:3000/events",
                    "GET",
                    new MyDownloadHandler(),
                    null
                )
            )
            {
                request.SetRequestHeader("Accept", "text/event-stream");
                
                Debug.Log("STARTING");
                
                yield return request.SendWebRequest();
                
                Debug.Log("DONE");
            }
        }
    }
}