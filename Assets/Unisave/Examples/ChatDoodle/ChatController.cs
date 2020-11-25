using System.Collections;
using Unisave.Broadcasting;
using Unisave.Examples.ChatDoodle.Backend;
using Unisave.Facades;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unisave.Examples.ChatDoodle
{
    public class ChatController : UnisaveBroadcastingClient
    {
        private async void OnEnable()
        {
            var subscription = await OnFacet<ChatFacet>
                .CallAsync<ChannelSubscription>(nameof(ChatFacet.JoinRoom));
            
            FromSubscription(subscription)
                .Forward<ChatMessage>(ChatMessageReceived)
                .Forward<PlayerJoinedMessage>(PlayerJoined)
                .ElseLogWarning();

            //StartCoroutine(Foo());
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            
            StopAllCoroutines();
        }

        void ChatMessageReceived(ChatMessage msg)
        {
            Debug.Log($"[{msg.nickname}]: {msg.message}");
        }

        void PlayerJoined(PlayerJoinedMessage msg)
        {
            Debug.Log("Someone joined the room.");
        }
        
        private IEnumerator Foo()
        {
            yield return new WaitForSeconds(3);

            SceneManager.LoadScene(1);

            /*yield return new WaitForSeconds(1);
            
            while (true)
            {
                yield return OnFacet<ChatFacet>.Call(
                    nameof(ChatFacet.SendMessage), "<nickname>", "Hello people!"
                ).AsCoroutine();
                Debug.Log("Message sent");
                
                yield return new WaitForSeconds(10);
            }*/
        }
    }
}