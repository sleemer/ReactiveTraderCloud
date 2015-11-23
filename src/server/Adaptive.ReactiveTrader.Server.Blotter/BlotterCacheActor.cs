using System;
using System.Collections.Generic;
using Adaptive.ReactiveTrader.Contract.Events;
using Akka.Actor;

namespace Adaptive.ReactiveTrader.Server.Blotter
{
    public class BlotterCacheActor : ReceiveActor
    {
        private TradeSubscriptionStates _tradeSubscriptionState = TradeSubscriptionStates.Unsubscribed;

        private Dictionary<string, TradeDto> _trades = new Dictionary<string, TradeDto>();

        public BlotterCacheActor()
        {
            Receive<WarmUpCacheMessage>(msg => WarmUpCache(msg.EventStoreActorRef));
            Receive<TradeCreatedEvent>(e => OnTradeCreatedEvent(e));
            Receive<TradeCompletedEvent>(e => OnTradeCompletedEvent(e));
            Receive<TradeRejectedEvent>(e => OnTradeRejectedEvent(e);
            Receive<BlotterEndOfSotwMessage>(_ => OnBlotterEndOfSotw());
        }

        private void OnBlotterEndOfSotw()
        {
            Console.WriteLine("Blotter publishing sotw");
        }

        private void OnTradeRejectedEvent(TradeRejectedEvent tradeRejectedEvent)
        {
            
        }

        private void OnTradeCompletedEvent(TradeCompletedEvent tradeCompletedEvent)
        {
            
        }

        private void OnTradeCreatedEvent(TradeCreatedEvent tradeCreatedEvent)
        {
            switch (_tradeSubscriptionState
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

    public class TradeDto
    {
        public long TradeId { get; set; }
        public string TraderName { get; set; }
        public string CurrencyPair { get; set; }
        public long Notional { get; set; }
        public string DealtCurrency { get; set; }
        public DirectionDto Direction { get; set; }
        public decimal SpotRate { get; set; }
        public DateTime TradeDate { get; set; }
        public DateTime ValueDate { get; set; }
        public TradeStatusDto Status { get; set; }

        public override string ToString()
        {
            return string.Format("TradeId: {0}, TraderName: {1}, CurrencyPair: {2}, Notional: {3}, Direction: {4}, SpotRate: {5}, TradeDate: {6}, ValueDate: {7}, Status: {8}, DealtCurrency: {9}", TradeId, TraderName, CurrencyPair, Notional, Direction, SpotRate, TradeDate, ValueDate, Status, DealtCurrency);
        }
    }
    public enum TradeStatusDto
    {
        Pending,
        Done,
        Rejected
    }
}