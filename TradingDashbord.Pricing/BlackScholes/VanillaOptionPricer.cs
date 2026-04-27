using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using TradingDashboard.Core.Entities;
using TradingDashboard.Core.Enums;
using TradingDashboard.Core.Interfaces;
using TradingDashbord.Pricing.Greeks;
using Entities = TradingDashboard.Core.Entities;


namespace TradingDashbord.Pricing.BlackScholes
{
    public class VanillaOptionPricer : IPricer<decimal>
    {
        private ProductType _supportedproduct;
        // MarketSnapshot(string Ticker, decimal SpotPrice, double ImpliedVolatility, double RiskFreeRate, DateTimeOffset Timestamp, string Source)
        public Task<decimal> CalculatePrice(Entities.Instrument instrument, MarketSnapshot SnapShot)
        {
            if (! (this.SupportedProduct == instrument.ProductType))
                throw new Exception("the instrument typed is not the one supported by the pricer");
            // instrument.Strike is not null because we know that instrument is an option
            double YearsToMaturity = ((instrument.Maturity).DayNumber - (DateOnly.FromDateTime(SnapShot.Timestamp.DateTime)).DayNumber) / 365.25;
            double D1 = MathExtensions.d1(SnapShot.SpotPrice, instrument.Strike.Value, SnapShot.RiskFreeRate, SnapShot.ImpliedVolatility, YearsToMaturity);
            double D2 = MathExtensions.d2(SnapShot.SpotPrice, instrument.Strike.Value, SnapShot.RiskFreeRate, SnapShot.ImpliedVolatility, YearsToMaturity);
            decimal CallPrice = SnapShot.SpotPrice * (decimal)D1.NormalCDF() - instrument.Strike.Value*(decimal)Math.Exp(-SnapShot.RiskFreeRate * YearsToMaturity)* (decimal)D2.NormalCDF();
            return (this.SupportedProduct == ProductType.Call) ? Task.FromResult(CallPrice) : Task.FromResult(CallPrice - SnapShot.SpotPrice + instrument.Strike.Value*(decimal)Math.Exp(-SnapShot.RiskFreeRate* YearsToMaturity));
        }
        public Task<Entities.Greeks> CalculateGreeks(Entities.Instrument instrument, MarketSnapshot SnapShot)
        {
            double YearsToMaturity = ((instrument.Maturity).DayNumber - (DateOnly.FromDateTime(SnapShot.Timestamp.DateTime)).DayNumber) / 365.25;
            return Task.FromResult(GreeksCalculator.Compute(instrument.ProductType, (double)SnapShot.SpotPrice, (double)instrument.Strike.Value, SnapShot.RiskFreeRate, SnapShot.ImpliedVolatility, YearsToMaturity));
        }
        public ProductType SupportedProduct {
            get => _supportedproduct;
            set {
                if(!value.IsOption())
                    throw new Exception("ProductType must be Call or Put");
                _supportedproduct = value;
            }
        }

    }
}
