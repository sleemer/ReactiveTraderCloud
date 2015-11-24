namespace Adaptive.ReactiveTrader.Server.Blotter
{
    public static class ActorNames
    {
        public const string ActorSystemName = "BlotterService";

        public static readonly ActorData ConsoleWriterActor = new ActorData("consoleWriter", $"akka://{ActorSystemName}/user"); // /user/consoleWriter
        public static readonly ActorData EventStoreActor = new ActorData("eventStore", $"akka://{ActorSystemName}/user"); // /user/eventStore
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