using System;
using System.Collections.Generic;
using Adaptive.ReactiveTrader.Contract;
using Adaptive.ReactiveTrader.Contract.Events;
using Adaptive.ReactiveTrader.Server.Blotter.EventStore;
using Akka.Actor;
using Akka.Event;

namespace Adaptive.ReactiveTrader.Server.Blotter.TradeCache
{
    public class TradeCacheActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private TradeSubscriptionStates _tradeSubscriptionState = TradeSubscriptionStates.Unsubscribed;

        private readonly Dictionary<long, TradeDto> _trades = new Dictionary<long, TradeDto>();

        public TradeCacheActor()
        {
            _log.Info("Created trade cache actor");
            Receive<WarmUpCacheMessage>(msg => WarmUpCache(msg.EventStoreActorRef));
            // todo consider mapping to message
            Receive<TradeCreatedEvent>(e => OnTradeCreatedEvent(e));
            Receive<TradeCompletedEvent>(e => OnTradeCompletedEvent(e));
            Receive<TradeRejectedEvent>(e => OnTradeRejectedEvent(e));
            Receive<BlotterEndOfSotwMessage>(_ => OnBlotterEndOfSotw());
        }

        private void WarmUpCache(ICanTell eventStoreActorRef)
        {
            _log.Info("Warming up trade cache");
            _tradeSubscriptionState = TradeSubscriptionStates.ReceivingSotw;

            Context.ActorSelection(ActorNames.EventStoreActor.Path).Tell(new GetTradesMessage());
            //eventStoreActorRef.Tell(new GetTradesMessage());
        }

        private void OnTradeCreatedEvent(TradeCreatedEvent tradeCreatedEvent)
        {
            _log.Info("Trade created: " + tradeCreatedEvent.TradeId);
            switch (_tradeSubscriptionState)
            {
                case TradeSubscriptionStates.Unsubscribed:
                    _log.Warning("Received created when unsubscribed");
                    break;
                case TradeSubscriptionStates.ReceivingSotw:
                    AddTradeCreated(tradeCreatedEvent);
                    break;
                case TradeSubscriptionStates.ReceivingUpdates:
                    AddTradeCreated(tradeCreatedEvent);
                    _log.Info("Publishing created trade");
                    // todo publish trade
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnTradeCompletedEvent(TradeCompletedEvent tradeCompletedEvent)
        {
            _log.Info("Trade completed: " + tradeCompletedEvent.TradeId);
            HandleTradeStatusUpdate(tradeCompletedEvent.TradeId, TradeStatusDto.Done);
        }

        private void OnTradeRejectedEvent(TradeRejectedEvent tradeRejectedEvent)
        {
            _log.Info("Trade rejected: " + tradeRejectedEvent.TradeId);
            HandleTradeStatusUpdate(tradeRejectedEvent.TradeId, TradeStatusDto.Rejected);
        }

        private void OnBlotterEndOfSotw()
        {
            _log.Info("Blotter publishing sotw"); // todo
            _tradeSubscriptionState = TradeSubscriptionStates.ReceivingUpdates;

            foreach (var tradeDto in _trades)
            {
                _log.Info(tradeDto.ToString());
            }
        }

        private void HandleTradeStatusUpdate(long tradeId, TradeStatusDto status)
        {
            switch (_tradeSubscriptionState)
            {
                case TradeSubscriptionStates.Unsubscribed:
                    _log.Warning($"Received {status} trade when unsubscribed");
                    break;
                case TradeSubscriptionStates.ReceivingSotw:
                    SetTradeStatus(tradeId, status);
                    break;
                case TradeSubscriptionStates.ReceivingUpdates:
                    SetTradeStatus(tradeId, status);
                    _log.Info($"Publishing {status} trade");
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
                _log.Warning("Received completed event for unknown trade: " + tradeId);
            }
        }

        private void AddTradeCreated(TradeCreatedEvent tradeCreatedEvent)
        {
            var dto = tradeCreatedEvent.ToDto();
            var key = dto.TradeId;
            if (_trades.ContainsKey(key))
            {
                _log.Warning("Create trade already has trade id: " + key);
            }
            _trades[key] = dto;
        }
    }
}