using System;
using Adaptive.ReactiveTrader.Contract.Events;

namespace Adaptive.ReactiveTrader.Contract
{
    public class TradeDto
    {
        public long TradeId { get; set; }
        public string TraderName { get; set; }
        public string CurrencyPair { get; set; }
        public long Notional { get; set; }
        public string DealtCurrency { get; set; }
        public DirectionDto Direction { get; set; }
        public decimal SpotRate { get; set; }
        public DateTime TradeDate { get; set; }
        public DateTime ValueDate { get; set; }
        public TradeStatusDto Status { get; set; }

        public override string ToString()
        {
            return
                $"TradeId: {TradeId}, TraderName: {TraderName}, CurrencyPair: {CurrencyPair}, Notional: {Notional}, Direction: {Direction}, SpotRate: {SpotRate}, TradeDate: {TradeDate}, ValueDate: {ValueDate}, Status: {Status}, DealtCurrency: {DealtCurrency}";
        }
    }

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