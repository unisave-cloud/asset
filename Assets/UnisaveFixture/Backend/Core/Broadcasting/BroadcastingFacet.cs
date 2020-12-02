using System.Threading;
using Unisave.Broadcasting;
using Unisave.Facades;
using Unisave.Facets;

namespace UnisaveFixture.Backend.Core.Broadcasting
{
    public class BroadcastingFacet : Facet
    {
        public ChannelSubscription SubscribeToMyChannel(
            string parameter,
            bool sendMessageAfterSubscribing
        )
        {
            var sub = Broadcast.Channel<MyChannel>()
                .WithParameters(parameter)
                .CreateSubscription();

            if (sendMessageAfterSubscribing)
            {
                Broadcast.Channel<MyChannel>()
                    .WithParameters(parameter)
                    .Send(new MyMessage {
                        foo = "Message after subscribing"
                    });

                // make sure the server really delivers the message
                // before a subscription handler can be registered
                Thread.Sleep(2000);
            }

            return sub;
        }

        public void SendMyMessage(string channelParameter, string foo)
        {
            Broadcast.Channel<MyChannel>()
                .WithParameters(channelParameter)
                .Send(new MyMessage {
                    foo = foo
                });
        }
        
        public void SendMyOtherMessage(string channelParameter, int bar)
        {
            Broadcast.Channel<MyChannel>()
                .WithParameters(channelParameter)
                .Send(new MyOtherMessage {
                    bar = bar
                });
        }
    }
}