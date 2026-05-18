using TradingDashboard.Core.Entities;
using TradingDashboard.Core.Entities.Market;
using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Interfaces
{
    /// <summary>
    /// Unified contract for pricing financial instruments.
    /// Supports a wide range of products including single-asset, multi-asset, FX, and quanto options.
    /// </summary>
    /// <typeparam name="TResult">The type of the pricing result containing price, Greeks, and metadata.</typeparam>
    public interface IPricer<TResult>
    {
        /// <summary>
        /// Calculates the comprehensive price of the instrument, including its Greeks and associated metrics.
        /// </summary>
        Task<TResult> CalculatePrice(
            Instrument instrument,
            MarketEnvironment env,
            CancellationToken ct = default);

        /// <summary>
        /// Calculates only the theoretical price of the instrument.
        /// This is a faster operation that bypasses Greek sensitivities computation.
        /// </summary>
        Task<double> CalculatePriceOnly(
            Instrument instrument,
            MarketEnvironment env,
            CancellationToken ct = default);

        /// <summary>
        /// Calculates the instrument's Greeks (Delta, Gamma, Vega, Theta, Rho) using finite differences.
        /// For multi-currency products, the Greeks are aggregated and expressed in the domestic currency.
        /// </summary>
        Task<Greeks> CalculateGreeks(
            Instrument instrument,
            MarketEnvironment env,
            CancellationToken ct = default);

        /// <summary>
        /// Gets the list of product types supported by this specific pricer implementation.
        /// Used by the pricing factory to route pricing requests dynamically.
        /// </summary>
        IReadOnlyList<ProductType> SupportedProducts { get; }
    }
}
