using TradingDashboard.Core.Entities;
using TradingDashboard.Core.Entities.Market;

namespace TradingDashboard.Core.Interfaces
{
    /// <summary>
    /// Contract for Monte Carlo path generation.
    /// Responsible for simulating underlying asset trajectories independently of the specific instrument or payoff.
    /// </summary>
    public interface IPathGenerator
    {
        /// <summary>
        /// Generates the tensor of simulated asset paths across all required factors, scenarios, and time steps.
        /// </summary>
        SimulatedPaths Generate(MarketEnvironment env, SimulationParameters parameters);

        /// <summary>Gets the total number of simulated stochastic factors.</summary>
        int NFactors { get; }

        /// <summary>
        /// Gets the mapping of simulated factors to their corresponding underlying assets.
        /// </summary>
        Underlying[] FactorMapping { get; }
    }
}
