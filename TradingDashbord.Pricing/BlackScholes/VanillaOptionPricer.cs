using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using TradingDashboard.Core.Entities;
using TradingDashboard.Core.Enums;
using TradingDashboard.Core.Interfaces;
using Entities = TradingDashboard.Core.Entities;


namespace TradingDashbord.Pricing.BlackScholes
{
    public class VanillaOptionPricer : IPricer<double>
    {
        private ProductType _supportedproduct;

        public VanillaOptionPricer(ProductType supportedproduct)
        {
            if (!supportedproduct.IsVanilla())
                throw new Exception("ProductType must be Call or Put");
            _supportedproduct = supportedproduct;
        }
        // MarketSnapshot(string Ticker, double SpotPrice, double ImpliedVolatility, double RiskFreeRate, DateTimeOffset Timestamp, string Source)
        public Task<double> CalculatePrice(Instrument instrument, MarketSnapshot SnapShot)
        {
            if (! (this.SupportedProduct == instrument.ProductType))
                throw new Exception("the instrument typed is not the one supported by the pricer");
            // instrument.Strike is not null because we know that instrument is an option
            double D1 = MathExtensions.d1(SnapShot.SpotPrice, instrument.Strike.Value, SnapShot.RiskFreeRate, SnapShot.ImpliedVolatility, instrument.YearsToMaturity);
            double D2 = MathExtensions.d2(SnapShot.SpotPrice, instrument.Strike.Value, SnapShot.RiskFreeRate, SnapShot.ImpliedVolatility, instrument.YearsToMaturity);
            double CallPrice = SnapShot.SpotPrice * D1.NormalCDF() - instrument.Strike.Value*Math.Exp(-SnapShot.RiskFreeRate * instrument.YearsToMaturity) * D2.NormalCDF();
            return (this.SupportedProduct == ProductType.Call) ? Task.FromResult(CallPrice) : Task.FromResult(CallPrice - SnapShot.SpotPrice + instrument.Strike.Value*Math.Exp(-SnapShot.RiskFreeRate* instrument.YearsToMaturity));
        }
        public Task<Greeks> CalculateGreeks(Instrument instrument, MarketSnapshot SnapShot)
        {
            return Task.FromResult(GreeksCalculator.Compute(instrument.ProductType, SnapShot.SpotPrice, instrument.Strike.Value, SnapShot.RiskFreeRate, SnapShot.ImpliedVolatility, instrument.YearsToMaturity));
        }
        public ProductType SupportedProduct { get => _supportedproduct;}

    }
}
