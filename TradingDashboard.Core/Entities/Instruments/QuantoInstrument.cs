using System;
using System.Collections.Generic;
using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Entities.Instruments
{
    public record QuantoInstrument(
        Guid Id,
        string Ticker,
        DateOnly Maturity,
        Currency DomesticCurrency,
        ProductType ProductType,
        double Strike,
        double FxQuantoRate,
        Underlying Underlying)
        : Instrument(Id, Ticker, Maturity, DomesticCurrency, ProductType)
    {
        public override double? StrikeOrNull => Strike;

        public override IEnumerable<string> GetTickers()
        {
            yield return Underlying.Name;
        }

        public static QuantoInstrument Create(
            string ticker,
            Underlying foreignUnderlying,
            DateOnly maturity,
            Currency domesticCurrency,
            ProductType productType,
            double strike,
            double fxQuantoRate,
            Guid id = default)
        {
            var inst = new QuantoInstrument(
                id == Guid.Empty ? Guid.NewGuid() : id,
                ticker, maturity, domesticCurrency,
                productType, strike, fxQuantoRate, foreignUnderlying);
            inst.Validate();
            return inst;
        }

        public override void Validate()
        {
            if (!ProductType.IsOption())
                throw new ArgumentException("QuantoInstrument ProductType must be an option.");
            if (Strike < 0)
                throw new ArgumentException("Strike must be non-negative.");
            if (FxQuantoRate <= 0)
                throw new ArgumentException("FxQuantoRate must be strictly positive.");
        }

        public override string ToString()
            => $"{Ticker} | QUANTO | {ProductType} | Strike: {Strike} | " +
               $"Underlying: {Underlying.Name} ({Underlying.NaturalCurrency}) | " +
               $"Paid in: {DomesticCurrency} | Maturity: {Maturity:yyyy-MM-dd} | Id: {Id}";
    }
}
