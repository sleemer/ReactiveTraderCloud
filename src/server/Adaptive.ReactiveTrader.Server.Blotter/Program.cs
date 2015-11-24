using System;
using Akka.Actor;

namespace Adaptive.ReactiveTrader.Server.Blotter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var eventStoreUrl = args.Length > 0 ? args[0] : "tcp://admin:changeit@127.0.0.1:1113";

            var system = ActorSystem.Create(ActorNames.ActorSystemName);

            system.ActorOf(Props.Create(() => new BootstrapActor(eventStoreUrl)), ActorNames.BootstrapActor.Name)
                .Tell(new BootstrapMessage());

            system.AwaitTermination();
        }
    }
}
