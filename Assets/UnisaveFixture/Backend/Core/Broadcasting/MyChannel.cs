using Unisave.Broadcasting;

namespace UnisaveFixture.Backend.Core.Broadcasting
{
    public class MyChannel : BroadcastingChannel
    {
        public SpecificChannel WithParameters(string parameter)
        {
            return SpecificChannel.From<MyChannel>(parameter);
        }
    }
}