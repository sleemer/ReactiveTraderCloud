namespace Adaptive.ReactiveTrader.Contract.Events
{
    public class TradeCompletedEvent
    {
        public int TradeId { get; }
​
        public TradeCompletedEvent(int tradeId)
        {
            TradeId = tradeId;
        }
    }
}