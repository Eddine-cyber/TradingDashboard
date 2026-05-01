using System;
using System.Collections.Generic;
using System.Text;
using Entities = TradingDashboard.Core.Entities;

namespace TradingDashbord.Pricing
{
    public record class PricingResult
    {
        internal decimal TheoreticalPrice { get; }
        internal Entities.Greeks Greeks { get; }
        internal DateTimeOffset ComputedAt { get; }
        internal string PricingMethod { get; }
        internal double? ConfidenceInterval { get; }

        internal PricingResult(decimal _theoreticalPrice, Entities.Greeks _greeks, string _pricingMethod, double? _confidenceInterval) {
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
