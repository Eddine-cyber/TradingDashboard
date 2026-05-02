using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.AccessControl;
using System.Text;
using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Entities
{
    public class Instrument
    {
        public int? ObservationCount { get; init; }
        public Guid Id { get; init; }
        public string Ticker { get; set; }
        public ProductType ProductType { get; set; }
        public decimal? Strike { get; set; }
        public DateOnly Maturity { get; set; }
        public string Underlying { get; set; }
        public Currency Currency { get; set; }

        public Instrument(Currency _currency, string _underlying, DateOnly _maturity, decimal? _strike, int? _observationCount, Guid _id, string _ticker, ProductType _productType)
        {
            Currency = _currency;
            Underlying = _underlying;
            Maturity = _maturity;
            ProductType = _productType;
            Ticker = _ticker;
            Strike = _strike;
            ObservationCount = _observationCount;
            // Id généré automatiquement si non fourni
            Id = _id == Guid.Empty ? Guid.NewGuid() : _id;
            // validation metier
            Validate();
        }

        public int DaysToMaturity
            => this.Maturity.DayNumber - DateOnly.FromDateTime(DateTime.Now).DayNumber;

        public bool IsExpired
            => this.Maturity < DateOnly.FromDateTime(DateTime.Now);

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