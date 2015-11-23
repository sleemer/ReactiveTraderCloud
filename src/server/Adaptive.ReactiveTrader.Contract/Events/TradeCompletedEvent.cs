namespace Adaptive.ReactiveTrader.Contract.Events
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
