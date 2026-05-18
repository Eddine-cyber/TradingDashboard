namespace TradingDashboard.Core.Entities.Market
{
    public record SimulationParameters(
        int NumberOfPaths = 100000,
        int Steps = 252,
        double MaturityInYears = 1.0,
        int? Seed = null
    );
}
