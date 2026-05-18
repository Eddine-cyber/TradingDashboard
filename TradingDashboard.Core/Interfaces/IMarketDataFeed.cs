using TradingDashboard.Core.Entities.Market;

namespace TradingDashboard.Core.Interfaces
{
    /// <summary>
    /// Contract for the real-time market data feed.
    /// Publishes a comprehensive, synchronized market environment (including equities, FX, and correlations)
    /// at every market tick.
    /// </summary>
    public interface IMarketDataFeed : IObservable<MarketEnvironment>
    {
        /// <summary>
        /// Retrieves a single, on-demand market snapshot for a specified ticker.
        /// </summary>
        Task<MarketSnapshot> GetSnapshotAsync(
            string ticker,
            CancellationToken ct = default);

        /// <summary>
        /// Retrieves on-demand market snapshots for a specified collection of tickers.
        /// </summary>
        Task<IReadOnlyList<MarketSnapshot>> GetSnapshotsAsync(
            IEnumerable<string> tickers,
            CancellationToken ct = default);

        /// <summary>
        /// Starts the real-time publishing loop.
        /// Pushes a synchronized global market environment on every tick until canceled.
        /// </summary>
        Task StartAsync(CancellationToken ct);
    }
}
