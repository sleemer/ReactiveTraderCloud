using System;
using Adaptive.ReactiveTrader.Server.Blotter.EventStore;
using Akka.Actor;

namespace Adaptive.ReactiveTrader.Server.Blotter
{
    public class Program
    {
        private static IActorRef _eventStoreActor;

        public static void Main(string[] args)
        {
            var eventStoreUrl = args.Length > 0 ? args[0] : "tcp://admin:changeit@127.0.0.1:1113";
            var wampUrl = args.Length > 1 ? args[1] : "ws://127.0.0.1:8080/ws";

            var system = ActorSystem.Create(ActorNames.ActorSystemName);

            _eventStoreActor = system.ActorOf(Props.Create(() => new EventStoreActor(eventStoreUrl)), ActorNames.EventStoreActor.Name);

            system.ActorOf(Props.Create(() => new BootstrapActor(eventStoreUrl, wampUrl)), ActorNames.BootstrapActor.Name)
                .Tell(new BootstrapMessage());

            system.AwaitTermination();
        }
    }
}
