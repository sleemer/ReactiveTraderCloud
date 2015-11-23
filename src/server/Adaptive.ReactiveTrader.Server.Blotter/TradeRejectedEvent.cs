namespace Adaptive.ReactiveTrader.Server.Blotter
{
    public class TradeRejectedEvent
    {
        public long TradeId { get; }

        public TradeRejectedEvent(long tradeId)
        {
            TradeId = tradeId;
        }
    }
}