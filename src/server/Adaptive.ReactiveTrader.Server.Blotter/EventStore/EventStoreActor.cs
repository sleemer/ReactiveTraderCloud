using System;
using System.Text;
using System.Threading.Tasks;
using Adaptive.ReactiveTrader.Contract.Events;
using Adaptive.ReactiveTrader.Server.Blotter.TradeCache;
using Akka.Actor;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Adaptive.ReactiveTrader.Server.Blotter.EventStore
{
    public class EventStoreActor : ReceiveActor
    {
        private const string TradeCompletedEvent = "TradeCompletedEvent";
        private const string TradeRejectedEvent = "TradeRejectedEvent";
        private const string TradeCreatedEvent = "TradeCreatedEvent";

        private IEventStoreConnection _conn;
        private EventStoreAllCatchUpSubscription _subscription;
        private IActorRef _cacheActor;

        public EventStoreActor()
        {
            Console.WriteLine("Creating event store actor");
            Receive<ConnectMessage>(async _ => {await Connect(); });
            Receive<GetTradesMessage>(_ => GetTrades());
        }

        private void GetTrades()
        {
            Console.WriteLine("Subscribing to all event store events");
            _cacheActor = Context.Sender;

            _subscription = _conn.SubscribeToAllFrom(Position.Start, false, OnEventAppeared, OnCaughtUp);
        }

        private void OnCaughtUp(EventStoreCatchUpSubscription obj)
        {
            _cacheActor.Tell(new BlotterEndOfSotwMessage());
        }

        private void OnEventAppeared(EventStoreCatchUpSubscription eventStoreCatchUpSubscription,
            ResolvedEvent resolvedEvent)
        {
            switch (resolvedEvent.Event.EventType)
            {
                default:
                    return;
                case TradeCreatedEvent:
                    //Console.WriteLine("Trade created event appeared: " + resolvedEvent.Event.EventId);
                    _cacheActor.Tell(GetEvent<TradeCreatedEvent>(resolvedEvent.Event));
                    break;
                case TradeCompletedEvent:
                    //Console.WriteLine("Trade completed event appeared: " + resolvedEvent.Event.EventId);
                    _cacheActor.Tell(GetEvent<TradeCompletedEvent>(resolvedEvent.Event));
                    break;
                case TradeRejectedEvent:
                    //Console.WriteLine("Trade rejected event appeared: " + resolvedEvent.Event.EventId);
                    _cacheActor.Tell(GetEvent<TradeRejectedEvent>(resolvedEvent.Event));
                    break;
            }
        }

        private async Task Connect()
        {
            Console.WriteLine("Connecting to event store...");

            var sender = Context.Sender;

            var connectionSettings = ConnectionSettings.Create(); //.KeepReconnecting(); // todo: reconnecting logic

            var uri = new Uri("tcp://admin:changeit@127.0.0.1:1113");
            _conn = EventStoreConnection.Create(connectionSettings, uri);
            await _conn.ConnectAsync();

            Console.WriteLine("Connected to event store");
            sender.Tell(new ConnectedMessage(), Self);
        }

        private static T GetEvent<T>(RecordedEvent evt)
        {
            var eventString = Encoding.UTF8.GetString(evt.Data);
            return JsonConvert.DeserializeObject<T>(eventString);
        }
    }
}