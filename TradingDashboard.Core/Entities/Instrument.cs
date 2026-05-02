using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.AccessControl;
using System.Text;
using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Entities
{
    public record Instrument(Currency Currency, string Underlying, DateOnly Maturity, double? Strike, int? ObservationCount, Guid Id, string Ticker, ProductType ProductType)
    {
        public static Instrument Create(Currency currency, string underlying, DateOnly maturity, double? strike, int? observationCount, Guid id, string ticker, ProductType productType)
        {
            var instrument = new Instrument(
                currency, underlying, maturity, strike,
                observationCount,
                id == Guid.Empty ? Guid.NewGuid() : id,
                ticker, productType);

            instrument.Validate();
            return instrument;
        }

        public int DaysToMaturity => this.Maturity.DayNumber - DateOnly.FromDateTime(DateTime.Now).DayNumber;

        public double YearsToMaturity => (this.Maturity.DayNumber - DateOnly.FromDateTime(DateTime.Now).DayNumber)/252;
        public bool IsExpired => this.Maturity <= DateOnly.FromDateTime(DateTime.Now);

        public void Validate()
        {
            if (Strike != null && Strike < 0)
                throw new Exception("Strike must be positive");

            if ((ProductType == ProductType.AsianCall || ProductType == ProductType.AsianPut)
                && ObservationCount == null)
                throw new Exception("Asian option must have an ObservationCount");

            if ((ProductType != ProductType.AsianCall && ProductType != ProductType.AsianPut)
                && ObservationCount != null)
                throw new Exception($"{ProductType} cannot have ObservationCount");

            if (ProductType == ProductType.LookbackCall || ProductType == ProductType.LookbackPut)
            {
                if (ObservationCount != null)
                    Console.WriteLine($"[Warning] {Ticker} : ObservationCount has no meaning for Lookback products");
            }
        }

        public override string ToString()
            => $"{Ticker} | {ProductType} | Strike: {(Strike.HasValue ? Strike.Value.ToString() : "N/A")} | Maturity: {Maturity:yyyy-MM-dd} | Underlying: {Underlying} | Currency: {Currency} | Id: {Id}";
    }
}