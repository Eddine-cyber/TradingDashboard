using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Entities;
using TradingDashboard.Core.Interfaces;
using TradingDashbord.Pricing.BlackScholes;
using TradingDashbord.Pricing.MonteCarlo;

namespace TradingDashbord.Pricing.Strategy
{
    public class MonteCarloPricingStrategy : IPricingStrategy
    {
        private readonly MonteCarloConfig _config;
        public MonteCarloPricingStrategy(MonteCarloConfig config = null)
        {
            _config = config ?? new MonteCarloConfig();
        }
        public async Task<PricingResult> PriceAsync(Instrument instrument, MarketSnapshot snapshot, CancellationToken ct = default)
        {
            IPayoff payoff = PayoffFactory.Creat(instrument);
            IPricer<PricingResult> pricer = new MonteCarloOptionPricer(instrument.ProductType, payoff, _config);
            return await pricer.CalculatePrice(instrument, snapshot);
        }
        public async Task<double> PriceOnlyAsync(Instrument instrument, MarketSnapshot snapshot, CancellationToken ct = default)
        {
            IPayoff payoff = PayoffFactory.Creat(instrument);
            IPricer<PricingResult> pricer = new MonteCarloOptionPricer(instrument.ProductType, payoff, _config);
            return await pricer.CalculatePriceOnly(instrument, snapshot);
        }
    }
}
