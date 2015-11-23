namespace Adaptive.ReactiveTrader.Contract.Events
{
    public class TradeCompletedEvent
    {
        public long TradeId { get; }
​
        public TradeCompletedEvent(long tradeId)
        {
            TradeId = tradeId;
        }
    }
}