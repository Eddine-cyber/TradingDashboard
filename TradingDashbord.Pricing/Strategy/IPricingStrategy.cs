using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Entities;

namespace TradingDashbord.Pricing.Strategy
{
    public interface IPricingStrategy
    {
        public Task<PricingResult> PriceAsync(Instrument instrument, MarketSnapshot snapshot, CancellationToken ct = default);
        public Task<double> PriceOnlyAsync(Instrument instrument, MarketSnapshot snapshot, CancellationToken ct = default);
    }
}
