namespace Adaptive.ReactiveTrader.Server.Blotter
{
    public static class ActorNames
    {
        public const string ActorSystemName = "BlotterService";

        public static readonly ActorData ConsoleWriterActor = new ActorData("consoleWriter", $"akka://{ActorSystemName}/user"); // /user/consoleWriter
        public static readonly ActorData BootstrapActor = new ActorData("bootstrap", $"akka://{ActorSystemName}/user"); // /user/bootstrap
        public static readonly ActorData EventStoreActor = new ActorData("eventStore", $"akka://{ActorSystemName}/user"); // /user/eventStore
        public static readonly ActorData WampActor = new ActorData("wampActor", $"akka://{ActorSystemName}/user"); // /user/wampActor
        public static readonly ActorData WampChildActor = new ActorData("wampChildActor", $"akka://{ActorSystemName}/user/wampActor/"); // /user/wampActor/wampChildActor
        public static readonly ActorData TradeCacheActor = new ActorData("tradeCache", $"akka://{ActorSystemName}/user"); // /user/tradeCache
    }

    public class ActorData
    {
        public ActorData(string name, string parent)
        {
            Path = parent + "/" + name;
            Name = name;
        }

        public ActorData(string name)
        {
            Path = $"akka://{ActorNames.ActorSystemName}/{name}";
            Name = name;
        }

        public string Name { get; }
        public string Path { get; }
    }
}