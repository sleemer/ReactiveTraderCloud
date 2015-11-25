using System.Threading.Tasks;
using Adaptive.ReactiveTrader.Contract;
using Adaptive.ReactiveTrader.Messaging;
using Adaptive.ReactiveTrader.Server.Blotter.TradeCache;
using Akka.Actor;
using Akka.Event;

namespace Adaptive.ReactiveTrader.Server.Blotter.Wamp
{
    public class WampServiceCallHandlerActor : ReceiveActor
    {
        #region messages
        public class CreateEndpointMessage
        {
            public ITransientDestination ReplyTo { get; }

            public CreateEndpointMessage(ITransientDestination replyTo)
            {
                ReplyTo = replyTo;
            }
        }

        private class EndpointCreatedMessage
        {
            public IPrivateEndPoint<TradesDto> Endpoint { get; }

            public EndpointCreatedMessage(IPrivateEndPoint<TradesDto> endpoint)
            {
                Endpoint = endpoint;
            }
        }
        #endregion

        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly IBroker _broker;

        public WampServiceCallHandlerActor(IBroker broker)
        {
            _broker = broker;
            Receive<CreateEndpointMessage>(message =>
            {
                _log.Info("Handling service call");

                var replyTo = message.ReplyTo;

                _broker.GetPrivateEndPoint<TradesDto>(replyTo)
                    .ContinueWith(endpointTask => new EndpointCreatedMessage(endpointTask.Result),
                        TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously)
                    .PipeTo(Self);
            });

            Receive<EndpointCreatedMessage>(message =>
            {
                var clientSubscriptionActor = Context.ActorOf(Props.Create(() => new WampClientSubscriptionActor(message.Endpoint))); // todo name

                var tradeCacheActor = Context.ActorSelection(ActorNames.TradeCacheActor.Path);

                tradeCacheActor.Ask<TradesDto>(new TradeCacheActor.SotwRequestMessage())
                    .ContinueWith(tradesTask => new WampClientSubscriptionActor.SendTradesMessage(tradesTask.Result),
                        TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously)
                    .PipeTo(clientSubscriptionActor);
            });
        }
    }
}