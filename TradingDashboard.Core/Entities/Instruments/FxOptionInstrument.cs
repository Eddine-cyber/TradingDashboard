using System;
using System.Collections.Generic;
using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Entities.Instruments
{
    /// <summary>
    /// European FX option priced with the Garman-Kohlhagen model.
    ///
    /// STATIC CONTRACT DATA:
    ///   - Strike     : exercise price in EUR (domestic currency)
    ///   - Underlying : FX rate identifier (e.g. Underlying("USD", EUR, FX))
    ///
    /// MARKET DATA (stored in MarketEnvironment, not here):
    ///   - FX spot     → MarketEnvironment.Snapshots["USD"].SpotPrice
    ///   - FX vol      → MarketEnvironment.Snapshots["USD"].ImpliedVolatility
    ///   - Foreign rate → MarketEnvironment.ForeignRates[Currency.USD]
    /// </summary>
    public record FxOptionInstrument(
        Guid Id,
        string Ticker,
        DateOnly Maturity,
        Currency DomesticCurrency,
        ProductType ProductType,
        double Strike,
        Underlying Underlying)
        : Instrument(Id, Ticker, Maturity, DomesticCurrency, ProductType)
    {
        /// <summary>Foreign currency of the option, taken from the FX underlying.</summary>
        public Currency ForeignCurrency => Underlying.NaturalCurrency;

        public override double? StrikeOrNull => Strike;

        public override IEnumerable<string> GetTickers()
        {
            yield return Underlying.Name;
        }

        public static FxOptionInstrument Create(
            string ticker,
            string fxTicker,
            Currency foreignCurrency,
            DateOnly maturity,
            Currency domesticCurrency,
            ProductType productType,
            double strike,
            Guid id = default)
        {
            // The FX rate is an Underlying of asset class FX
            var fxUnderlying = new Underlying(fxTicker, foreignCurrency, AssetClass.FX);
            var inst = new FxOptionInstrument(
                id == Guid.Empty ? Guid.NewGuid() : id,
                ticker, maturity, domesticCurrency,
                productType, strike, fxUnderlying);
            inst.Validate();
            return inst;
        }

        public override void Validate()
        {
            if (ProductType != ProductType.FxCall && ProductType != ProductType.FxPut
                && ProductType != ProductType.FxAsianCall && ProductType != ProductType.FxAsianPut
                && ProductType != ProductType.FxBarrierCall && ProductType != ProductType.FxBarrierPut)
                throw new ArgumentException("ProductType must be an FX option type.");
            if (Strike <= 0)
                throw new ArgumentException("FX option Strike must be strictly positive.");
            if (ForeignCurrency == DomesticCurrency)
                throw new ArgumentException(
                    $"ForeignCurrency ({ForeignCurrency}) and DomesticCurrency ({DomesticCurrency}) must differ.");
            if (Underlying.AssetClass != AssetClass.FX)
                throw new ArgumentException("FxOptionInstrument Underlying must have AssetClass.FX.");
        }

        public override string ToString()
            => $"{Ticker} | {ProductType} | Strike: {Strike} | " +
               $"FX: {ForeignCurrency}/{DomesticCurrency} | Maturity: {Maturity:yyyy-MM-dd} | Id: {Id}";
    }
}
