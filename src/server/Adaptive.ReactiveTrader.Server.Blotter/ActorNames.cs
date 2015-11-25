namespace Adaptive.ReactiveTrader.Server.Blotter
{
    public static class ActorNames
    {
        public const string ActorSystemName = "BlotterService";

        public static readonly ActorData EventStoreActor = new ActorData("eventStore", $"akka://{ActorSystemName}/user"); // /user/eventStore
        public static readonly ActorData WampConnectionActor = new ActorData("wampConnectionActor", $"akka://{ActorSystemName}/user"); // /user/wampConnectionActor
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