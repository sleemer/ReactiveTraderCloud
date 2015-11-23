using Adaptive.ReactiveTrader.Server.Blotter.EventStore;
using Adaptive.ReactiveTrader.Server.Blotter.TradeCache;
using Akka.Actor;
using Akka.Event;

namespace Adaptive.ReactiveTrader.Server.Blotter
{
    public class BootstrapActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();

        private IActorRef _eventStoreActor;
        private IActorRef _blotterCacheActor;

        public BootstrapActor()
        {
            Receive<ConnectedMessage>(_ => OnEventStoreActorConnected());
            Receive<BootstrapMessage>(_ => Bootstrap());

            _log.Info("Bootstrap ctor");
        }

        private void Bootstrap()
        {
            _eventStoreActor = Context.ActorOf<EventStoreActor>("EventStore");
            _eventStoreActor.Tell(new ConnectMessage(), Self);
        }

        private void OnEventStoreActorConnected()
        {
            _blotterCacheActor = Context.ActorOf<TradeCacheActor>("TradeCache");
            _blotterCacheActor.Tell(new WarmUpCacheMessage(_eventStoreActor));
        }
    }
}