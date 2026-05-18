using System;
using System.Collections.Generic;
using System.Linq;
using TradingDashboard.Core.Entities.Schedule;
using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Entities.Instruments
{
    /// <summary>
    /// Multi-assets instrument (Basket, WorstOf, BestOf, Spread, Outperformance).
    /// </summary>
    public record MultiAssetInstrument(
        Guid Id,
        string Ticker,
        DateOnly Maturity,
        Currency DomesticCurrency,
        ProductType ProductType,
        double Strike,
        IReadOnlyList<AssetComponent> Components,
        FixingSchedule? Schedule = null)
        : Instrument(Id, Ticker, Maturity, DomesticCurrency, ProductType)
    {
        public override double? StrikeOrNull => Strike;

        public override IEnumerable<string> GetTickers()
        {
            return Components.Select(c => c.Underlying.Name);
        }

        public static MultiAssetInstrument Create(
            string ticker,
            DateOnly maturity,
            Currency domesticCurrency,
            ProductType productType,
            double strike,
            IReadOnlyList<AssetComponent> components,
            FixingSchedule? schedule = null,
            Guid id = default)
        {
            var inst = new MultiAssetInstrument(
                id == Guid.Empty ? Guid.NewGuid() : id,
                ticker, maturity, domesticCurrency,
                productType, strike, components, schedule);
            inst.Validate();
            return inst;
        }

        public override void Validate()
        {
            if (!ProductType.IsMultiAsset())
                throw new ArgumentException("ProductType must be a multi-asset option.");
            if (Components.Count < 2)
                throw new ArgumentException("A multi-asset instrument must have at least 2 components.");
            
            if (ProductType == ProductType.BasketCall || ProductType == ProductType.BasketPut)
            {
                double weightSum = Components.Sum(c => c.Weight);
                if (Math.Abs(weightSum - 1.0) > 1e-6)
                    throw new ArgumentException($"Basket weights must sum to 1. Got {weightSum:F6}.");
            }
        }

        public override string ToString()
            => $"{Ticker} | {ProductType} | Strike: {Strike} | " +
               $"Components: {Components.Count} | Maturity: {Maturity:yyyy-MM-dd} | " +
               $"Domestic: {DomesticCurrency} | Id: {Id}";
    }

    /// <summary>
    /// Component of a multi-assets instrument.
    /// </summary>
    public record AssetComponent(
        Underlying Underlying,
        double Weight);
}
