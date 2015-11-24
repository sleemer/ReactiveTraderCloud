using System.Text;
using Adaptive.ReactiveTrader.Contract;
using Adaptive.ReactiveTrader.Messaging;
using Akka.Actor;
using Akka.Event;
using Newtonsoft.Json;

namespace Adaptive.ReactiveTrader.Server.Blotter.Wamp
{
    public class WampActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private const string Realm = "com.weareadaptive.reactivetrader";

        private IBroker _channel;

        public WampActor(string wampUrl)
        {
            Receive<ConnectWampMessage>(_ =>
            {
                _channel = BrokerFactory.Create(wampUrl, Realm).Result;
                _channel.RegisterCall("reference.getBlotterTrades", GetBlotterTrades).Wait();
            });
        }

        private void GetBlotterTrades(IRequestContext requestContext, IMessage message)
        {
            // todo: is this needed? payload only required if we're doing client specific blotter responses
            var payload = JsonConvert.DeserializeObject<NothingDto>(Encoding.UTF8.GetString(message.Payload));
            var replyTo = message.ReplyTo;

            var responseChannel = _channel.CreateChannelAsync<TradeDto>(replyTo).Result;
        }
    }
}