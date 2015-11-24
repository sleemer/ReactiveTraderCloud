using Adaptive.ReactiveTrader.Server.Blotter.EventStore;
using Adaptive.ReactiveTrader.Server.Blotter.TradeCache;
using Adaptive.ReactiveTrader.Server.Blotter.Wamp;
using Akka.Actor;
using Akka.Event;

namespace Adaptive.ReactiveTrader.Server.Blotter
{
    public class BootstrapActor : ReceiveActor
    {
        private readonly string _eventStoreUrl;
        private readonly string _wampUrl;
        private readonly ILoggingAdapter _log = Context.GetLogger();

        private ActorSelection _eventStoreActor;
        private IActorRef _blotterCacheActor;
        private IActorRef _wampActor;

        public BootstrapActor(string eventStoreUrl, string wampUrl)
        {
            _eventStoreUrl = eventStoreUrl;
            _wampUrl = wampUrl;
            Receive<EventStoreConnectedMessage>(_ => OnEventStoreActorConnected());
            Receive<BootstrapMessage>(_ => Bootstrap());
        }

        private void Bootstrap()
        {
            _eventStoreActor = Context.ActorSelection(ActorNames.EventStoreActor.Path);
            _eventStoreActor.Tell(new ConnectEventStoreMessage());

            _wampActor = Context.ActorOf(Props.Create(() => new WampActor(_wampUrl)), ActorNames.WampActor.Name);
        }

        private void OnEventStoreActorConnected()
        {
            _blotterCacheActor = Context.ActorOf(Props.Create(() => new TradeCacheActor()), ActorNames.TradeCacheActor.Name);
            _blotterCacheActor.Tell(new WarmUpCacheMessage(_eventStoreActor));
        }
    }
}