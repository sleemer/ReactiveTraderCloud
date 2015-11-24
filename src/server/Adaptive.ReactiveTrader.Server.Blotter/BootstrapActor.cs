using Adaptive.ReactiveTrader.Server.Blotter.EventStore;
using Adaptive.ReactiveTrader.Server.Blotter.TradeCache;
using Akka.Actor;
using Akka.Event;

namespace Adaptive.ReactiveTrader.Server.Blotter
{
    public class BootstrapActor : ReceiveActor
    {
        private readonly string _eventStoreUrl;
        private readonly ILoggingAdapter _log = Context.GetLogger();

        private IActorRef _eventStoreActor;
        private IActorRef _blotterCacheActor;

        public BootstrapActor(string eventStoreUrl)
        {
            _eventStoreUrl = eventStoreUrl;
            Receive<ConnectedMessage>(_ => OnEventStoreActorConnected());
            Receive<BootstrapMessage>(_ => Bootstrap());

            _log.Info("Bootstrap ctor");
        }

        private void Bootstrap()
        {
            _eventStoreActor = Context.ActorOf(Props.Create(() => new EventStoreActor(_eventStoreUrl)), ActorNames.EventStoreActor.Name);
            _eventStoreActor.Tell(new ConnectMessage(), Self);
        }

        private void OnEventStoreActorConnected()
        {
            _blotterCacheActor = Context.ActorOf(Props.Create(() => new TradeCacheActor()), ActorNames.TradeCacheActor.Name);
            _blotterCacheActor.Tell(new WarmUpCacheMessage(_eventStoreActor));
        }
    }
}