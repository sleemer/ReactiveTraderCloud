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

        public EventStoreActor()
        {
            Console.WriteLine("Creating event store actor");
            Receive<ConnectMessage>(async _ => {await Connect(); });
            Receive<GetTradesMessage>(_ => GetTrades());
        }

        private void GetTrades()
        {
            Console.WriteLine("Subscribing to all event store events");
            _subscription = _conn.SubscribeToAllFrom(Position.Start, false, OnEventAppeared, OnCaughtUp);
        }

        private void OnCaughtUp(EventStoreCatchUpSubscription obj)
        {
            Context.Sender.Tell(new BlotterEndOfSotwMessage());
        }

        private void OnEventAppeared(EventStoreCatchUpSubscription eventStoreCatchUpSubscription,
            ResolvedEvent resolvedEvent)
        {
            switch (resolvedEvent.Event.EventType)
            {
                default:
                    return;
                case TradeCreatedEvent:
                    Context.Sender.Tell(GetEvent<TradeCreatedEvent>(resolvedEvent.Event));
                    break;
                case TradeCompletedEvent:
                    Context.Sender.Tell(GetEvent<TradeCompletedEvent>(resolvedEvent.Event));
                    break;
                case TradeRejectedEvent:
                    Context.Sender.Tell(GetEvent<TradeRejectedEvent>(resolvedEvent.Event));
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