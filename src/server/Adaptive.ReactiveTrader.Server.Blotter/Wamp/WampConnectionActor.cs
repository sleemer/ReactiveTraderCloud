using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using Adaptive.ReactiveTrader.Messaging;
using Akka.Actor;
using Akka.Event;

namespace Adaptive.ReactiveTrader.Server.Blotter.Wamp
{
    public class WampConnectionActor : ReceiveActor
    {
        #region messages
        public class ConnectWampMessage
        {
        }
        #endregion

        private readonly ILoggingAdapter _log = Context.GetLogger();
        private const string Realm = "com.weareadaptive.reactivetrader";

        private IBroker _broker;

        private readonly Dictionary<string, IActorRef> _children = new Dictionary<string, IActorRef>();

        public WampConnectionActor(string wampUrl)
        {
            Receive<ConnectWampMessage>(_ =>
            {
                var self = Self;
                _broker = BrokerFactory.Create(wampUrl, Realm).Result;
                _broker.RegisterCall("blotter.getTradesStream", (__, message) =>  self.Ask(message)).Wait();
            });

            Receive<IMessage>(message => GetBlotterTrades(message));
        }

        private Task GetBlotterTrades(IMessage message)
        {
            // spawn / get child actor
            IActorRef child;
            if (!_children.TryGetValue(message.ReplyTo.ToString(), out child))
            {
                child = Context.ActorOf(Props.Create(() => new WampServiceCallHandlerActor(_broker)));
                _children.Add(message.ReplyTo.ToString(), child);
            }
            child.Tell(new WampServiceCallHandlerActor.CreateEndpointMessage(message.ReplyTo));

            return Task.FromResult(Unit.Default);
        }
    }
}