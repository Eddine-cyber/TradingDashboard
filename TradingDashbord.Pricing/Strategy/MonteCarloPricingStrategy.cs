using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Entities;

namespace TradingDashbord.Pricing.Strategy
{
    public class MonteCarloPricingStrategy : IPricingStrategy
    {
        public async Task<PricingResult> PriceAsync(Instrument instrument, MarketSnapshot snapshot, CancellationToken ct = default)
        {
            throw new NotImplementedException("MonteCarlo not implemented yet");
        }

    }
}
