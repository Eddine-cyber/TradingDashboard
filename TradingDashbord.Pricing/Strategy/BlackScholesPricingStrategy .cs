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
            IPricer<decimal> pricer = new VanillaOptionPricer(instrument.ProductType);
            decimal theoreticalPrice = await pricer.CalculatePrice(instrument, snapshot);
            Greeks greeks = await pricer.CalculateGreeks(instrument, snapshot);
            PricingResult res  = new PricingResult(theoreticalPrice, greeks, "BlackScholes", null);
            return res;
        }

    }
}
