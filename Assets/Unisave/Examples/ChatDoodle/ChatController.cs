using System;
using System.Threading.Tasks;
using LightJson;
using Unisave.Broadcasting;
using UnityEngine;

namespace Unisave.Examples.ChatDoodle
{
    public class ChatController : UnisaveBroadcastingClient
    {
        async void Start()
        {
            await SubscribeToChannelAsync("some-channel");
        }

        protected override void OnDropletReceived(string channelId, JsonObject droplet)
        {
            // switch (droplet["$type"])
        }
    }
}