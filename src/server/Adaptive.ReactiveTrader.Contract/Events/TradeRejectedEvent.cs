namespace Adaptive.ReactiveTrader.Contract.Events
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