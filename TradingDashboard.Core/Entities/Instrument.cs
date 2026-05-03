using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.AccessControl;
using System.Text;
using TradingDashboard.Core.Enums;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TradingDashboard.Core.Entities
{
    public abstract record Instrument(
        Guid Id,
        string Ticker,
        Underlying Underlying,       // Underlying porte sa propre currency
        DateOnly Maturity,
        Currency DomesticCurrency,   // devise de règlement, EUR pour ton desk
        ProductType ProductType)
    {
        public int DaysToMaturity =>
            Maturity.DayNumber - DateOnly.FromDateTime(DateTime.Now).DayNumber;
        public double YearsToMaturity =>
            DaysToMaturity / 252.0;
        public bool IsExpired =>
            Maturity <= DateOnly.FromDateTime(DateTime.Now);

        /// <summary>
        /// Accès polymorphique au strike (nullable).
        /// Retourne le strike pour VanillaInstrument, AsianInstrument, LookbackInstrument,
        /// BasketInstrument, FxOptionInstrument, QuantoInstrument.
        /// Retourne null pour Future et Forward.
        /// Utilisation dans Pricing : instrument.StrikeOrNull!.Value
        /// </summary>
        public double? StrikeOrNull => this switch
        {
            VanillaInstrument  v => v.Strike,
            AsianInstrument    a => a.Strike,
            LookbackInstrument l => l.Strike,
            _ => null
        };

        /// <summary>
        /// Accès polymorphique à ObservationCount (nullable).
        /// Non-null uniquement pour AsianInstrument.
        /// </summary>
        public int? ObservationCountOrNull => this switch
        {
            AsianInstrument a => a.ObservationCount,
            _ => null
        };

        public abstract void Validate();
        public abstract override string ToString();
    }



    public record VanillaInstrument(
        Guid Id,
        string Ticker,
        Underlying Underlying,
        DateOnly Maturity,
        Currency DomesticCurrency,
        double Strike,
        ProductType ProductType)
        : Instrument(Id, Ticker, Underlying, Maturity, DomesticCurrency, ProductType)
    {
        public static VanillaInstrument Create(
            string ticker, Underlying underlying, DateOnly maturity,
            Currency domesticCurrency, double strike,
            ProductType productType, Guid id = default)
        {
            var inst = new VanillaInstrument(
                id == Guid.Empty ? Guid.NewGuid() : id,
                ticker, underlying, maturity, domesticCurrency, strike, productType);
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
        Underlying Underlying,
        DateOnly Maturity,
        Currency DomesticCurrency,
        double Strike,
        int ObservationCount,
        ProductType ProductType)
        : Instrument(Id, Ticker, Underlying, Maturity, DomesticCurrency, ProductType)
    {
        public static AsianInstrument Create(
            string ticker, Underlying underlying, DateOnly maturity,
            Currency domesticCurrency, double strike,
            int observationCount, ProductType productType, Guid id = default)
        {
            var inst = new AsianInstrument(
                id == Guid.Empty ? Guid.NewGuid() : id,
                ticker, underlying, maturity, domesticCurrency,
                strike, observationCount, productType);
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
        Underlying Underlying,
        DateOnly Maturity,
        Currency DomesticCurrency,
        double Strike,
        ProductType ProductType)
        : Instrument(Id, Ticker, Underlying, Maturity, DomesticCurrency, ProductType)
    {
        public static LookbackInstrument Create(
            string ticker, Underlying underlying, DateOnly maturity,
            Currency domesticCurrency, double strike,
            ProductType productType, Guid id = default)
        {
            var inst = new LookbackInstrument(
                id == Guid.Empty ? Guid.NewGuid() : id,
                ticker, underlying, maturity, domesticCurrency, strike, productType);
            inst.Validate();
            return inst;
        }

        public override void Validate()
        {
            if (ProductType != ProductType.LookbackCall && ProductType != ProductType.LookbackPut)
                throw new ArgumentException("ProductType must be LookbackCall or LookbackPut");
            if (Strike < 0)
                throw new ArgumentException("Strike must be positive");
        }

        public override string ToString()
            => $"{Ticker} | {ProductType} | Strike: {Strike} | Maturity: {Maturity:yyyy-MM-dd} | {Underlying.Name} ({Underlying.NaturalCurrency}) | Id: {Id}";
    }
}
