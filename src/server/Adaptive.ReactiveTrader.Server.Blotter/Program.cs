using System;
using Adaptive.ReactiveTrader.Server.Blotter.EventStore;
using Adaptive.ReactiveTrader.Server.Blotter.TradeCache;
using Adaptive.ReactiveTrader.Server.Blotter.Wamp;
using Akka.Actor;

namespace Adaptive.ReactiveTrader.Server.Blotter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var eventStoreUrl = args.Length > 0 ? args[0] : "tcp://admin:changeit@127.0.0.1:1113";
            var wampUrl = args.Length > 1 ? args[1] : "ws://127.0.0.1:8080/ws";

            var system = ActorSystem.Create(ActorNames.ActorSystemName);

            var tradeCacheActor = system.ActorOf(Props.Create(() => new TradeCacheActor()), ActorNames.TradeCacheActor.Name);

            var wampActor = system.ActorOf(Props.Create(() => new WampActor(wampUrl)), ActorNames.WampActor.Name);
            wampActor.Tell(new ConnectWampMessage());

            var eventStoreActor = system.ActorOf(Props.Create(() => new EventStoreActor(eventStoreUrl)), ActorNames.EventStoreActor.Name);
            eventStoreActor.Tell(new ConnectEventStoreMessage());

            system.AwaitTermination();
        }
    }
}
