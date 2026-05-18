using System;
using System.Collections.Generic;
using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Entities.Instruments
{
    public record VanillaInstrument(
        Guid Id,
        string Ticker,
        DateOnly Maturity,
        Currency DomesticCurrency,
        ProductType ProductType,
        double Strike,
        Underlying Underlying
        )
        : Instrument(Id, Ticker, Maturity, DomesticCurrency, ProductType)
    {
        public override double? StrikeOrNull => Strike;

        public override IEnumerable<string> GetTickers()
        {
            yield return Underlying.Name;
        }

        public static VanillaInstrument Create(
            string ticker, Underlying underlying, DateOnly maturity,
            Currency domesticCurrency, ProductType productType, double strike, Guid id = default)
        {
            var inst = new VanillaInstrument(
                id == Guid.Empty ? Guid.NewGuid() : id,
                ticker, maturity, domesticCurrency, productType, strike, underlying);
            inst.Validate();
            return inst;
        }

        public override void Validate()
        {
            if (!ProductType.IsVanilla())
                throw new ArgumentException("ProductType must be Call or Put");
            if (Strike < 0)
                throw new ArgumentException("Strike must be positive");
        }

        public override string ToString()
            => $"{Ticker} | {ProductType} | Strike: {Strike} | Maturity: {Maturity:yyyy-MM-dd} | {Underlying.Name} ({Underlying.NaturalCurrency}) | Domestic: {DomesticCurrency} | Id: {Id}";
    }

    public record AsianInstrument(
        Guid Id,
        string Ticker,
        DateOnly Maturity,
        Currency DomesticCurrency,
        ProductType ProductType,
        double Strike,
        int ObservationCount,
        Underlying Underlying)
        : Instrument(Id, Ticker, Maturity, DomesticCurrency, ProductType)
    {
        public override double? StrikeOrNull => Strike;
        public override int? ObservationCountOrNull => ObservationCount;

        public override IEnumerable<string> GetTickers()
        {
            yield return Underlying.Name;
        }

        public static AsianInstrument Create(
            string ticker, Underlying underlying, DateOnly maturity,
            Currency domesticCurrency, ProductType productType, double strike,
            int observationCount, Guid id = default)
        {
            var inst = new AsianInstrument(
                id == Guid.Empty ? Guid.NewGuid() : id,
                ticker, maturity, domesticCurrency,
                productType, strike, observationCount, underlying);
            inst.Validate();
            return inst;
        }

        public override void Validate()
        {
            if (ProductType != ProductType.AsianCall && ProductType != ProductType.AsianPut)
                throw new ArgumentException("ProductType must be AsianCall or AsianPut");
            if (Strike < 0)
                throw new ArgumentException("Strike must be positive");
            if (ObservationCount <= 0)
                throw new ArgumentException("ObservationCount must be positive");
        }

        public override string ToString()
            => $"{Ticker} | {ProductType} | Strike: {Strike} | Obs: {ObservationCount} | Maturity: {Maturity:yyyy-MM-dd} | {Underlying.Name} ({Underlying.NaturalCurrency}) | Id: {Id}";
    }

    public record LookbackInstrument(
        Guid Id,
        string Ticker,
        DateOnly Maturity,
        Currency DomesticCurrency,
        ProductType ProductType,
        double Strike,
        Underlying Underlying)
        : Instrument(Id, Ticker, Maturity, DomesticCurrency, ProductType)
    {
        public override double? StrikeOrNull => Strike;

        public override IEnumerable<string> GetTickers()
        {
            yield return Underlying.Name;
        }

        public static LookbackInstrument Create(
            string ticker, Underlying underlying, DateOnly maturity,
            Currency domesticCurrency, ProductType productType, double strike, Guid id = default)
        {
            var inst = new LookbackInstrument(
                id == Guid.Empty ? Guid.NewGuid() : id,
                ticker, maturity, domesticCurrency, productType, strike, underlying);
            inst.Validate();
            return inst;
        }

        public override void Validate()
        {
            if (ProductType != ProductType.LookbackCall && ProductType != ProductType.LookbackPut && 
                ProductType != ProductType.FloatingLookbackCall && ProductType != ProductType.FloatingLookbackPut)
                throw new ArgumentException("ProductType must be a Lookback variant");
            if (Strike < 0)
                throw new ArgumentException("Strike must be positive");
        }

        public override string ToString()
            => $"{Ticker} | {ProductType} | Strike: {Strike} | Maturity: {Maturity:yyyy-MM-dd} | {Underlying.Name} ({Underlying.NaturalCurrency}) | Id: {Id}";
    }

    public record BarrierInstrument(
        Guid Id,
        string Ticker,
        DateOnly Maturity,
        Currency DomesticCurrency,
        ProductType ProductType,
        double Strike,
        double BarrierLevel,
        Underlying Underlying)
        : Instrument(Id, Ticker, Maturity, DomesticCurrency, ProductType)
    {
        public override double? StrikeOrNull => Strike;

        public override IEnumerable<string> GetTickers()
        {
            yield return Underlying.Name;
        }

        public override void Validate()
        {
            if (!ProductType.IsBarrier())
                throw new ArgumentException("ProductType must be a Barrier option");
            if (Strike < 0)
                throw new ArgumentException("Strike must be positive");
            if (BarrierLevel < 0)
                throw new ArgumentException("BarrierLevel must be positive");
        }

        public override string ToString()
            => $"{Ticker} | {ProductType} | Strike: {Strike} | Barrier: {BarrierLevel} | Maturity: {Maturity:yyyy-MM-dd} | {Underlying.Name} | Id: {Id}";
    }

    public record DigitalInstrument(
        Guid Id,
        string Ticker,
        DateOnly Maturity,
        Currency DomesticCurrency,
        ProductType ProductType,
        double Strike,
        double CashPayoff,
        Underlying Underlying)
        : Instrument(Id, Ticker, Maturity, DomesticCurrency, ProductType)
    {
        public override double? StrikeOrNull => Strike;

        public override IEnumerable<string> GetTickers()
        {
            yield return Underlying.Name;
        }

        public override void Validate()
        {
            if (!ProductType.IsDigital())
                throw new ArgumentException("ProductType must be a Digital option");
            if (Strike < 0)
                throw new ArgumentException("Strike must be positive");
        }

        public override string ToString()
            => $"{Ticker} | {ProductType} | Strike: {Strike} | Payoff: {CashPayoff} | Maturity: {Maturity:yyyy-MM-dd} | {Underlying.Name} | Id: {Id}";
    }

    public record VarianceSwapInstrument(
        Guid Id,
        string Ticker,
        DateOnly Maturity,
        Currency DomesticCurrency,
        ProductType ProductType,
        double VarianceStrike,
        double Notional,
        Underlying Underlying)
        : Instrument(Id, Ticker, Maturity, DomesticCurrency, ProductType)
    {
        public override IEnumerable<string> GetTickers()
        {
            yield return Underlying.Name;
        }

        public override void Validate()
        {
            if (ProductType != ProductType.VarianceSwap && ProductType != ProductType.VolatilitySwap)
                throw new ArgumentException("ProductType must be VarianceSwap or VolatilitySwap");
            if (VarianceStrike < 0)
                throw new ArgumentException("VarianceStrike must be positive");
            if (Notional <= 0)
                throw new ArgumentException("Notional must be positive");
        }

        public override string ToString()
            => $"{Ticker} | {ProductType} | Strike: {VarianceStrike} | Notional: {Notional} | Maturity: {Maturity:yyyy-MM-dd} | {Underlying.Name} | Id: {Id}";
    }
}
