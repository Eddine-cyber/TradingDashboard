using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Entities;
using TradingDashboard.Core.Enums;
using TradingDashbord.Pricing.MonteCarlo;

namespace TradingDashbord.Pricing.Strategy
{
    public class PricingEngine
    {
        private BlackScholesPricingStrategy _bsStrategy;
        private MonteCarloPricingStrategy _mcStrategy;
        public PricingEngine(BlackScholesPricingStrategy bs = null, MonteCarloPricingStrategy mc = null, MonteCarloConfig config = null)
        {
            _bsStrategy = bs ?? new BlackScholesPricingStrategy();
            _mcStrategy = mc ?? new MonteCarloPricingStrategy(config);
        }

        private IPricingStrategy Resolve(Instrument instrument)
        {
            if (instrument.ProductType.IsVanilla()) return _bsStrategy;
            else if (instrument.ProductType.IsExotic()) return _mcStrategy;
            else throw new ArgumentException("Product not supported");
        }
        public Task<PricingResult> PriceAsync(Instrument instrument, MarketSnapshot snapshot, CancellationToken ct = default)
        {
            return Resolve(instrument).PriceAsync(instrument, snapshot, ct);
        }
        public Task<double> PriceOnlyAsync(Instrument instrument, MarketSnapshot snapshot, CancellationToken ct = default)
        {
            return Resolve(instrument).PriceOnlyAsync(instrument, snapshot, ct);
        }
        public async Task<IReadOnlyList<PricingResult>> PricePortfolioAsync(IReadOnlyList<Instrument> instruments, MarketSnapshot snapshot, CancellationToken ct = default)
        {
            var tasks = instruments.Select(instrument => Resolve(instrument).PriceAsync(instrument, snapshot, ct));
            var results = await Task.WhenAll(tasks);
            return results;
        }
        public void SwitchStrategy(MonteCarloPricingStrategy newStrategy)
        {
            // Only Mc strategy can be switched because it is the only custumizable strategy
            // Strategy = newStrategy; false because it is not thread safe :
            // this field should be visible by all the threads after the switch (no race condition)
            Interlocked.Exchange(ref _mcStrategy, newStrategy); // Guarantees that the write is immediately visible to all threads,
                                                             // with no possibility of reading a stale or outdated value.
        }
    }
}
