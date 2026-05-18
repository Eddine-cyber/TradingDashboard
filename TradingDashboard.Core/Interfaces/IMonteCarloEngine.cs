using TradingDashboard.Core.Entities;
using TradingDashboard.Core.Entities.Market;

namespace TradingDashboard.Core.Interfaces
{
    /// <summary>
    /// Monte Carlo pricing engine responsible for orchestrating path generation,
    /// variance reduction techniques, and payoff evaluation to estimate the discounted expectation.
    /// </summary>
    public interface IMonteCarloEngine
    {
        /// <summary>
        /// Computes the Monte Carlo price of an instrument.
        /// Returns the discounted expected price along with the standard error and the 95% confidence interval.
        /// </summary>
        /// <param name="gen">The path generator simulating the underlying asset dynamics.</param>
        /// <param name="payoff">The payoff function to evaluate across the generated paths.</param>
        /// <param name="env">The current market environment containing spot prices, rates, and correlations.</param>
        /// <param name="sim">The simulation parameters (number of paths, time steps, maturity).</param>
        Task<PricingResult> PriceAsync(
            IPathGenerator gen,
            IPayoff payoff,
            MarketEnvironment env,
            SimulationParameters sim,
            CancellationToken ct = default);

        /// <summary>
        /// Computes the instrument's Greeks using central finite differences with common random numbers.
        /// The bumps (Delta, Gamma, Vega, Theta, Rho) are evaluated concurrently.
        /// </summary>
        Task<Greeks> ComputeGreeksAsync(
            IPathGenerator gen,
            IPayoff payoff,
            MarketEnvironment env,
            SimulationParameters sim,
            CancellationToken ct = default);
    }
}
