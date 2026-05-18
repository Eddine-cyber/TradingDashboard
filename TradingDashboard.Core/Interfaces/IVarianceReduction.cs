using TradingDashboard.Core.Entities.Market;

namespace TradingDashboard.Core.Interfaces
{
    public interface IVarianceReduction
    {
        SimulatedPaths Apply(SimulatedPaths rawPaths, MarketEnvironment env);
        string Name { get; }
    }
}
