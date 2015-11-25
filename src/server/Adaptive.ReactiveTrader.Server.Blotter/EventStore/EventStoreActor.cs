using System;
using System.Text;
using System.Threading.Tasks;
using Adaptive.ReactiveTrader.Contract.Events;
using Adaptive.ReactiveTrader.Server.Blotter.TradeCache;
using Akka.Actor;
using Akka.Event;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Adaptive.ReactiveTrader.Server.Blotter.EventStore
{
    public class EventStoreActor : ReceiveActor
    {
        #region messages
        internal class ConnectEventStoreMessage
        {
        }

        internal class GetTradesMessage
        {
        }
        #endregion

        private const string TradeCompletedEvent = "TradeCompletedEvent";
        private const string TradeRejectedEvent = "TradeRejectedEvent";
        private const string TradeCreatedEvent = "TradeCreatedEvent";

        private readonly ILoggingAdapter _log = Context.GetLogger();
        private IEventStoreConnection _conn;
        private IActorRef _cacheActor;
        private EventStoreCatchUpSubscription _subscription;

        public EventStoreActor(string eventStoreUrl)
        {
            _log.Info("Creating event store actor");

            Receive<ConnectEventStoreMessage>(_ =>
            {
                _log.Info("Connecting to event store...");

                var tradeCacheActor = Context.ActorSelection(ActorNames.TradeCacheActor.Path);

                var connectionSettings = ConnectionSettings.Create(); //.KeepReconnecting(); // todo: reconnecting logic

                var uri = new Uri(eventStoreUrl);
                _conn = EventStoreConnection.Create(connectionSettings, uri);

                _conn.ConnectAsync()
                    .ContinueWith(__ => new TradeCacheActor.WarmUpCacheMessage(),
                        TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously)
                    .PipeTo(tradeCacheActor);

                _log.Info("Connected to event store");
            });

            Receive<GetTradesMessage>(_ => GetTrades());
        }

        private void GetTrades()
        {
            _log.Info("Subscribing to all event store events");
            _cacheActor = Context.Sender;
            _subscription = _conn.SubscribeToAllFrom(Position.Start, false, OnEventAppeared, OnCaughtUp);
        }

        private void OnCaughtUp(EventStoreCatchUpSubscription obj)
        {
            _cacheActor.Tell(new TradeCacheActor.BlotterEndOfSotwMessage());
        }

        private void OnEventAppeared(EventStoreCatchUpSubscription eventStoreCatchUpSubscription, ResolvedEvent resolvedEvent)
        {
            switch (resolvedEvent.Event.EventType)
            {
                default:
                    return;
                case TradeCreatedEvent:
                    _cacheActor.Tell(GetEvent<TradeCreatedEvent>(resolvedEvent.Event));
                    break;
                case TradeCompletedEvent:
                    _cacheActor.Tell(GetEvent<TradeCompletedEvent>(resolvedEvent.Event));
                    break;
                case TradeRejectedEvent:
                    _cacheActor.Tell(GetEvent<TradeRejectedEvent>(resolvedEvent.Event));
                    break;
            }
        }

        private static T GetEvent<T>(RecordedEvent evt)
        {
            var eventString = Encoding.UTF8.GetString(evt.Data);
            return JsonConvert.DeserializeObject<T>(eventString);
        }
    }
}