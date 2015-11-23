using System;
using Akka.Actor;

namespace Adaptive.ReactiveTrader.Server.Blotter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var system = ActorSystem.Create("BlotterService");

            var bootstrapActor = system.ActorOf<BootstrapActor>("Bootstrapper");

            bootstrapActor.Tell(new BootstrapMessage());

            Console.WriteLine("Press a key to exit Blotter service");
            Console.ReadKey();
        }
    }
}
