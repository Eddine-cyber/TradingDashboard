using TradingDashboard.Core.Entities;
using TradingDashboard.Core.Entities.Market;

namespace TradingDashboard.Core.Interfaces
{
    /// <summary>
    /// Contract for the risk engine.
    /// Operates on the global market environment to compute PnL and aggregated sensitivities,
    /// accounting for cross-currency and quanto FX positions.
    /// </summary>
    public interface IRiskEngine
    {
        /// <summary>
        /// Calculates the portfolio's unrealized PnL within the provided market environment.
        /// The PnL is expressed in the domestic currency.
        /// </summary>
        Task<double> CalculatePnl(
            MarketEnvironment env,
            CancellationToken ct = default);

        /// <summary>
        /// Calculates the aggregated Greeks for all active positions.
        /// Requires the full market environment to handle quanto and cross-currency corrections.
        /// </summary>
        Task<Greeks> CalculateGreeks(
            MarketEnvironment env,
            CancellationToken ct = default);

        /// <summary>
        /// Verifies whether the portfolio breaches any predefined risk limits (e.g., DeltaLimit, DailyLossLimit).
        /// Returns a list of triggered alerts, or an empty list if no limits are exceeded.
        /// </summary>
        Task<IReadOnlyList<Alert>> VerifyRiskLimits(
            CancellationToken ct = default);
    }
}
