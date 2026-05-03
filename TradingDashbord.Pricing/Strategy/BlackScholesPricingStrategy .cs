using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Entities;
using TradingDashboard.Core.Interfaces;
using TradingDashbord.Pricing.BlackScholes;

namespace TradingDashbord.Pricing.Strategy
{
    public class BlackScholesPricingStrategy : IPricingStrategy
    {
        public async Task<PricingResult> PriceAsync(Instrument instrument, MarketSnapshot snapshot, CancellationToken ct = default)
        {
            IPricer<PricingResult> pricer = new VanillaOptionPricer(instrument.ProductType);
            return await pricer.CalculatePrice(instrument, snapshot);
        }
        public async Task<double> PriceOnlyAsync(Instrument instrument, MarketSnapshot snapshot, CancellationToken ct = default)
        {
            IPricer<PricingResult> pricer = new VanillaOptionPricer(instrument.ProductType);
            return await pricer.CalculatePriceOnly(instrument, snapshot);
        }

    }
}
