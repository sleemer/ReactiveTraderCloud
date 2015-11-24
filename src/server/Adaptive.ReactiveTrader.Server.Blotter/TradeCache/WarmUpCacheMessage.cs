using Akka.Actor;

namespace Adaptive.ReactiveTrader.Server.Blotter.TradeCache
{
    internal class WarmUpCacheMessage
    {
        public ICanTell EventStoreActorRef { get; }

        public WarmUpCacheMessage(ICanTell eventStoreActorRef)
        {
            EventStoreActorRef = eventStoreActorRef;
        }
    }
}