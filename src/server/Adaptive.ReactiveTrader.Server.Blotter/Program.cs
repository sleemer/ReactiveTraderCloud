using Akka.Actor;

namespace Adaptive.ReactiveTrader.Server.Blotter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var system = ActorSystem.Create("BlotterService");

            var bootstrapActor = system.ActorOf<BootstrapActor>();
        }
    }
}
