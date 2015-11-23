using System;
using System.Collections.Generic;
using Adaptive.ReactiveTrader.Contract.Events;
using Adaptive.ReactiveTrader.Server.Blotter.EventStore;
using Akka.Actor;

namespace Adaptive.ReactiveTrader.Server.Blotter.TradeCache
{
    public class TradeCacheActor : ReceiveActor
    {
        private TradeSubscriptionStates _tradeSubscriptionState = TradeSubscriptionStates.Unsubscribed;

        private readonly Dictionary<long, TradeDto> _trades = new Dictionary<long, TradeDto>();

        public TradeCacheActor()
        {
            Console.WriteLine("Created trade cache actor");
            Receive<WarmUpCacheMessage>(msg => WarmUpCache(msg.EventStoreActorRef));
            // todo consider mapping to message
            Receive<TradeCreatedEvent>(e => OnTradeCreatedEvent(e));
            Receive<TradeCompletedEvent>(e => OnTradeCompletedEvent(e));
            Receive<TradeRejectedEvent>(e => OnTradeRejectedEvent(e));
            Receive<BlotterEndOfSotwMessage>(_ => OnBlotterEndOfSotw());
        }

        private void WarmUpCache(IActorRef eventStoreActorRef)
        {
            Console.WriteLine("Warming up trade cache");
            _tradeSubscriptionState = TradeSubscriptionStates.ReceivingSotw;
            eventStoreActorRef.Tell(new GetTradesMessage(), Self);
        }

        private void OnTradeCreatedEvent(TradeCreatedEvent tradeCreatedEvent)
        {
            Console.WriteLine("Trade created: " + tradeCreatedEvent.TradeId);
            switch (_tradeSubscriptionState)
            {
                case TradeSubscriptionStates.Unsubscribed:
                    Console.WriteLine("Received created when unsubscribed");
                    break;
                case TradeSubscriptionStates.ReceivingSotw:
                    AddTradeCreated(tradeCreatedEvent);
                    break;
                case TradeSubscriptionStates.ReceivingUpdates:
                    AddTradeCreated(tradeCreatedEvent);
                    Console.WriteLine("Publishing created trade");
                    // todo publish trade
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnTradeCompletedEvent(TradeCompletedEvent tradeCompletedEvent)
        {
            Console.WriteLine("Trade completed: " + tradeCompletedEvent.TradeId);
            HandleTradeStatusUpdate(tradeCompletedEvent.TradeId, TradeStatusDto.Done);
        }

        private void OnTradeRejectedEvent(TradeRejectedEvent tradeRejectedEvent)
        {
            Console.WriteLine("Trade rejected: " + tradeRejectedEvent.TradeId);
            HandleTradeStatusUpdate(tradeRejectedEvent.TradeId, TradeStatusDto.Rejected);
        }

        private void OnBlotterEndOfSotw()
        {
            Console.WriteLine("Blotter publishing sotw"); // todo
            _tradeSubscriptionState = TradeSubscriptionStates.ReceivingUpdates;

            foreach (var tradeDto in _trades)
            {
                Console.WriteLine(tradeDto.ToString());
            }
        }

        private void HandleTradeStatusUpdate(long tradeId, TradeStatusDto status)
        {
            switch (_tradeSubscriptionState)
            {
                case TradeSubscriptionStates.Unsubscribed:
                    Console.WriteLine($"Received {status} trade when unsubscribed");
                    break;
                case TradeSubscriptionStates.ReceivingSotw:
                    SetTradeStatus(tradeId, status);
                    break;
                case TradeSubscriptionStates.ReceivingUpdates:
                    SetTradeStatus(tradeId, status);
                    Console.WriteLine($"Publishing {status} trade");
                    // todo publish trade
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetTradeStatus(long tradeId, TradeStatusDto tradeStatus)
        {
            TradeDto trade;
            if (_trades.TryGetValue(tradeId, out trade))
            {
                trade.Status = tradeStatus;
            }
            else
            {
                Console.WriteLine("Warning - received completed event for unknown trade: " + tradeId);
            }
        }

        private void AddTradeCreated(TradeCreatedEvent tradeCreatedEvent)
        {
            var dto = tradeCreatedEvent.ToDto();
            var key = dto.TradeId;
            if (_trades.ContainsKey(key))
            {
                Console.WriteLine("Create trade - already has trade id: " + key);
            }
            _trades[key] = dto;
        }
    }
}