using TradingDashboard.Core.Entities.Market;

namespace TradingDashboard.Core.Interfaces
{
    public interface IPayoff
    {
        double[] Compute(SimulatedPaths paths);
        string[] RequiredAssets { get; }
        string Name { get; }
    }
}
