using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Entities;
using TradingDashbord.Pricing.Strategy;

namespace TradingDashbord.Pricing
{
    public class PricingEngine
    {
        private IPricingStrategy _strategy;
        public IPricingStrategy Strategy => _strategy;
        public PricingEngine(IPricingStrategy strategy)
        {
            _strategy = strategy;
        }
        public Task<PricingResult> PriceAsync(Instrument instrument, MarketSnapshot snapshot, CancellationToken ct = default)
        {
            return Strategy.PriceAsync(instrument, snapshot, ct);
        }
        public async Task<IReadOnlyList<PricingResult>> PricePortfolioAsync(IReadOnlyList<Instrument> instruments, MarketSnapshot snapshot, CancellationToken ct = default)
        {
            var tasks = instruments.Select(instrument => Strategy.PriceAsync(instrument, snapshot, ct));
            var results = await Task.WhenAll(tasks);
            return results;
        }
        void SwitchStrategy(IPricingStrategy newStrategy)
        {
            // Strategy = newStrategy; false because it is not thread safe :
            // this field should be visible by all the threads after the switch (no race condition)
            Interlocked.Exchange(ref _strategy, newStrategy); // Guarantees that the write is immediately visible to all threads,
                                                             // with no possibility of reading a stale or outdated value.
        }
    }
}
