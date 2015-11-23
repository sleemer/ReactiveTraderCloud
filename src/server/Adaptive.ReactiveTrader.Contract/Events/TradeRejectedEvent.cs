namespace Adaptive.ReactiveTrader.Contract.Events
{
    public class TradeRejectedEvent
    {
        public int TradeId { get; }
        public string Reason { get; }
​
        public TradeRejectedEvent(int tradeId, string reason)
        {
            TradeId = tradeId;
            Reason = reason;
        }
    }
}