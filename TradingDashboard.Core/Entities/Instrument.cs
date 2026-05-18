using System;
using System.Collections.Generic;
using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Entities
{
    public abstract record Instrument(
        Guid Id,
        string Ticker,
        DateOnly Maturity,
        Currency DomesticCurrency,   // payment currency
        ProductType ProductType)
    {
        public int DaysToMaturity =>
            Maturity.DayNumber - DateOnly.FromDateTime(DateTime.Now).DayNumber;
        public double YearsToMaturity =>
            DaysToMaturity / 252.0;
        public bool IsExpired =>
            Maturity <= DateOnly.FromDateTime(DateTime.Now);

        public virtual double? StrikeOrNull => null;

        public virtual int? ObservationCountOrNull => null;

        public abstract IEnumerable<string> GetTickers();

        public abstract void Validate();
        public abstract override string ToString();
    }
}
