using System.Threading.Tasks;
using Adaptive.ReactiveTrader.Server.Blotter.EventStore;
using Adaptive.ReactiveTrader.Server.Blotter.TradeCache;
using Akka.Actor;

namespace Adaptive.ReactiveTrader.Server.Blotter
{
    public class BootstrapActor : ReceiveActor
    {
        private IActorRef _eventStoreActor;
        private IActorRef _blotterCacheActor;

        public BootstrapActor()
        {
            Receive<ConnectedMessage>(_ => OnEventStoreActorConnected());

            _eventStoreActor = Context.ActorOf<EventStoreActor>();
            
            _eventStoreActor.Tell(new ConnectMessage(), Self);
        }

        private void OnEventStoreActorConnected()
        {
            _blotterCacheActor = Context.ActorOf<TradeCacheActor>();
            _blotterCacheActor.Tell(new WarmUpCacheMessage(_eventStoreActor));
        }
    }
}