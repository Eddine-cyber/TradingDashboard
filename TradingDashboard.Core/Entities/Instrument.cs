using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.AccessControl;
using System.Text;
using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Entities
{
    public class Instrument{
        private decimal? _strike; // nullable car un future n'a pas de strike

        public Guid Id { get; init; }
        public string Ticker { get; set; }
        public ProductType ProductType { get; set; }
        public decimal? Strike
        {
            get => _strike;
            set {
                if (value < 0)
                    throw new Exception("Strike must be positive");
                _strike = value;
            }
        }
        public DateOnly Maturity { get; set; }
        public string Underlying { get; set; }
        public Currency Currency { get; set; }
        public int DaysToMaturity => this.Maturity.DayNumber - DateOnly.FromDateTime(DateTime.Now).DayNumber;

        public bool IsExpired => this.Maturity < DateOnly.FromDateTime(DateTime.Now);

        public override string ToString() => $"{Ticker} | {ProductType} | Strike: {(Strike.HasValue ? Strike.Value.ToString() : "N/A")} | Maturity: {Maturity:yyyy-MM-dd} | Underlying: {Underlying} | Currency: {Currency} | Id: {Id}";
    }
}
