using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Adaptive.ReactiveTrader.Contract;
using Adaptive.ReactiveTrader.Messaging;
using Adaptive.ReactiveTrader.Server.Blotter.TradeCache;
using Akka.Actor;
using Akka.Event;
using Newtonsoft.Json;

namespace Adaptive.ReactiveTrader.Server.Blotter.Wamp
{
    public class WampConnectionActor : ReceiveActor
    {
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
                _broker.RegisterCall("blotter.getTradesStream", (ctx, message) =>  GetBlotterTrades(self, ctx, message)).Wait();
            });

            Receive<BlotterTradeUpdateMessage>(message =>
            {
                foreach (var child in _children.Values)
                {
                    child.Tell(message);
                }
            });

            Receive<IMessage>(message => GetBlotterTradesInvoke(message));
        }

        private Task GetBlotterTrades(IActorRef refer, IRequestContext _, IMessage message)
        {
            refer.Tell(message);
            return Task.FromResult(Unit.Default);
        }

        private Task GetBlotterTradesInvoke(IMessage message)
        {
            // spawn / get child actor
            IActorRef child;
            if (!_children.TryGetValue(message.ReplyTo.ToString(), out child))
            {
                child = Context.ActorOf(Props.Create(() => new WampServiceCallHandlerActor(message.ReplyTo.ToString(), _broker)));
                _children.Add(message.ReplyTo.ToString(), child);
            }
            child.Tell(message);

            return Task.FromResult(Unit.Default);
        }

        // resiliency strategy
    }

    public class WampServiceCallHandlerActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();

        private readonly string _sessionId;
        private readonly IBroker _broker;

        public WampServiceCallHandlerActor(string sessionId, IBroker broker)
        {
            _sessionId = sessionId;
            _broker = broker;
            Receive<IMessage>(message =>
            {
                _log.Info("Handling service call");

                var payload = JsonConvert.DeserializeObject<NothingDto>(Encoding.UTF8.GetString(message.Payload));
                var replyTo = message.ReplyTo;

                _broker.GetPrivateEndPoint<TradesDto>(replyTo)
                    .ContinueWith(endpointTask => new EndpointCreatedMessage(endpointTask.Result),
                        TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously)
                    .PipeTo(Self);

                //_service.GetCurrencyPairUpdatesStream(context, payload)
                //    .TakeUntil(endPoint.TerminationSignal)
                //    .Subscribe(endPoint);
            });

            Receive<EndpointCreatedMessage>(message =>
            {
                var clientSubscriptionActor = Context.ActorOf(Props.Create(() => new WampClientSubscriptionActor(message.Endpoint))); // todo name

                var tradeCacheActor = Context.ActorSelection(ActorNames.TradeCacheActor.Path);

                tradeCacheActor.Ask<TradesDto>(new TradeCacheActor.SotwRequestMessage())
                    .ContinueWith(tradesTask => new SendTradesMessage(tradesTask.Result),
                        TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously)
                    .PipeTo(clientSubscriptionActor);
            });
        }
    }

    public class WampClientSubscriptionActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly IPrivateEndPoint<TradesDto> _endpoint;

        protected override void PreStart()
        {
            Context.System.EventStream.Subscribe(Self, typeof (SendTradesMessage));
        }

        public WampClientSubscriptionActor(IPrivateEndPoint<TradesDto> endpoint)
        {
            _endpoint = endpoint;
            Receive<SendTradesMessage>(msg =>
            {
                _log.Info("Sending response to client");
                _endpoint.PushMessage(msg.Trades);
            });

            // todo: kill this actor
            // _endpoint.TerminationSignal.Subscribe()
        }
    }

    public class EndpointCreatedMessage
    {
        public IPrivateEndPoint<TradesDto> Endpoint { get; }

        public EndpointCreatedMessage(IPrivateEndPoint<TradesDto> endpoint)
        {
            Endpoint = endpoint;
        }
    }
    public class SendTradesMessage
    {
        public TradesDto Trades { get; }

        public SendTradesMessage(TradesDto trades)
        {
            Trades = trades;
        }
    }

    public class BlotterSotwMessage
    {
        public IEnumerable<TradeDto> Trades { get; }

        public BlotterSotwMessage(IEnumerable<TradeDto> trades)
        {
            Trades = trades;
        }
    }

    public class BlotterTradeUpdateMessage
    {
        public TradeDto Trade { get; }

        public BlotterTradeUpdateMessage(TradeDto trade)
        {
            Trade = trade;
        }
    }
}