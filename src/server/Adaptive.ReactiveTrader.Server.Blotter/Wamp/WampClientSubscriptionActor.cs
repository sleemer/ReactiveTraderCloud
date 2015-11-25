using Adaptive.ReactiveTrader.Contract;
using Adaptive.ReactiveTrader.Messaging;
using Akka.Actor;
using Akka.Event;

namespace Adaptive.ReactiveTrader.Server.Blotter.Wamp
{
    public class WampClientSubscriptionActor : ReceiveActor
    {
        #region messages
        public class SendTradesMessage
        {
            public TradesDto Trades { get; }

            public SendTradesMessage(TradesDto trades)
            {
                Trades = trades;
            }
        }
        #endregion

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
}