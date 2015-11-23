using Adaptive.ReactiveTrader.Contract.Events;
using Akka.Actor;

namespace Adaptive.ReactiveTrader.Server.Blotter
{
    public class BlotterCacheActor : ReceiveActor
    {
        private TradeSubscriptionStates _tradeSubscriptionStates = TradeSubscriptionStates.Unsubscribed;

        public BlotterCacheActor()
        {
            Receive<WarmUpCacheMessage>(msg => WarmUpCache(msg.EventStoreActorRef));
            Receive<TradeCreatedEvent>(e => OnTradeCreatedEvent());
            Receive<TradeCompletedEvent>(e => OnTradeCompletedEvent());
            Receive<TradeRejectedEvent>(e => OnTradeRejectedEvent());
            Receive<BlotterEndOfSotwMessage>(e => OnBlotterEndOfSotw());
        }

        private void OnBlotterEndOfSotw()
        {
            
        }

        private void OnTradeRejectedEvent()
        {
            
        }

        private void OnTradeCompletedEvent()
        {
            
        }

        private void OnTradeCreatedEvent()
        {
            
        }

        private void WarmUpCache(IActorRef eventStoreActorRef)
        {
            eventStoreActorRef.Tell(new GetTradesMessage(), Self);
        }
    }

    public enum TradeSubscriptionStates
    {
        Unsubscribed,
        ReceivingSotw,
        ReceivingUpdates
    }
}