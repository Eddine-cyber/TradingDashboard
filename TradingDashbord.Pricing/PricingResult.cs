using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Entities;

namespace TradingDashbord.Pricing
{
    public record class PricingResult
    {
        public double TheoreticalPrice { get; }
        public Greeks Greeks { get; }
        public DateTimeOffset ComputedAt { get; }
        public string PricingMethod { get; }
        public double? ConfidenceInterval { get; }

        public PricingResult(double _theoreticalPrice, Greeks _greeks, string _pricingMethod, double? _confidenceInterval) {
            TheoreticalPrice = _theoreticalPrice;
            Greeks = _greeks;
            ComputedAt = DateTimeOffset.UtcNow;

            if (_pricingMethod != "BlackScholes" && _pricingMethod != "MonteCarlo")
            {
                throw new Exception("PricingMethod should be either BlackScholes or MonteCarlo");
            }
            PricingMethod = _pricingMethod;

            if (PricingMethod != "MonteCarlo") ConfidenceInterval = null;
            else ConfidenceInterval = _confidenceInterval;
        }
    }

}
