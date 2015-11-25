using Adaptive.ReactiveTrader.Contract.Events;

namespace Adaptive.ReactiveTrader.Contract
{
    public static class Mappers
    {
        public static TradeDto ToDto(this TradeCreatedEvent e)
        {
            return new TradeDto
            {
                TradeId = e.TradeId,
                TraderName = "Trader1", // todo
                CurrencyPair = e.CurrencyPair,
                Notional = e.Notional,
                DealtCurrency = e.DealtCurrency,
                Direction = e.Direction, // todo
                SpotRate = e.SpotRate,
                TradeDate = e.TradeDate,
                ValueDate = e.ValueDate,
                Status = TradeStatusDto.Pending
            };
        }
    }
}