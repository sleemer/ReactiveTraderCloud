using System;
using System.Text;
using System.Threading.Tasks;
using Adaptive.ReactiveTrader.Contract.Events;
using Akka.Actor;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Adaptive.ReactiveTrader.Server.Blotter
{
    public class EventStoreActor : ReceiveActor
    {
        private const string TradeCompletedEvent = "TradeCompletedEvent";
        private const string TradeRejectedEvent = "TradeRejectedEvent";
        private const string TradeCreatedEvent = "TradeCreatedEvent";

        private IEventStoreConnection _conn;

        public EventStoreActor()
        {
            Receive<ConnectMessage>(async _ => {await Connect(); });
            Receive<GetTradesMessage>(_ => GetTrades());
        }

        private void GetTrades()
        {
            var subscription = _conn.SubscribeToAllFrom(Position.Start, false, OnEventAppeared, OnCaughtUp);
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
            var sender = Context.Sender;

            var connectionSettings = ConnectionSettings.Create().KeepReconnecting(); // todo: reconnecting logic

            var uri = new Uri("tcp://admin:changeit@127.0.0.1:1113");
            _conn = EventStoreConnection.Create(connectionSettings, uri);
            await _conn.ConnectAsync();

            sender.Tell(new ConnectedMessage(), Self);
        }

        private static T GetEvent<T>(RecordedEvent evt)
        {
            var eventString = Encoding.UTF8.GetString(evt.Data);
            return JsonConvert.DeserializeObject<T>(eventString);
        }
    }

    internal class BlotterEndOfSotwMessage
    {
    }

    internal class ConnectedMessage
    {
    }

    public class ConnectMessage
    {
        
    }
}