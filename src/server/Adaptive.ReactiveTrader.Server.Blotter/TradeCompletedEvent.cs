namespace Adaptive.ReactiveTrader.Server.Blotter
{
    public class TradeCompletedEvent
    {
        public TradeCompletedEvent(long tradeId)
        {
            TradeId = tradeId;
        }

        public long TradeId { get; }
    }
}
