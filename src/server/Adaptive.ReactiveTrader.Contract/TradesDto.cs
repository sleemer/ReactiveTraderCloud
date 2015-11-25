using System.Collections.Generic;

namespace Adaptive.ReactiveTrader.Contract
{
    public class TradesDto
    {
        public IEnumerable<TradeDto> Trades { get; set; }
    }
}