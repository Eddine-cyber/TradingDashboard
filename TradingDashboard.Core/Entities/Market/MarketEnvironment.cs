using System;
using System.Collections.Generic;
using System.Linq;
using TradingDashboard.Core.Enums;
using TradingDashboard.Core.Exceptions;

namespace TradingDashboard.Core.Entities.Market
{
    /// <summary>
    /// Global market environment encompassing all risk factors.
    ///
    /// PRINCIPLES:
    ///   FX exchange rates are treated as standard stochastic underlyings.
    ///   For example, Underlying("USD", EUR, FX) -> SpotPrice = 0.92, ImpliedVol = 0.08 in Snapshots["USD"].
    ///   There is no separate FX rates dictionary.
    ///
    /// DOMESTIC CURRENCY: EUR (fixed). DomesticRate corresponds to the EUR risk-free rate.
    ///   ForeignRates[Currency.USD] holds the USD risk-free rate (e.g., Fed Funds rate), not the FX spot.
    ///
    /// RX DATA FLOW:
    ///   IMarketDataFeed publishes this MarketEnvironment on every market tick.
    ///   -> Extract(tickers) filters the environment for a specific instrument or portfolio.
    ///   -> IPathGenerator simulates only the extracted sub-correlation matrix.
    ///
    /// MODEL CALIBRATION (Heston, Local Vol, Local Stochastic Vol):
    ///   Calibrated parameters are stored in the database and injected directly into the PathGenerator
    ///   during construction, ensuring a strict separation between real-time data and static model calibration.
    /// </summary>
    public record MarketEnvironment
    {
        /// <summary>The domestic EUR risk-free rate, serving as the risk-neutral reference measure.</summary>
        public double DomesticRate { get; init; }

        /// <summary>
        /// Risk-free rates for foreign currencies.
        /// Example: ForeignRates[Currency.USD] = 0.04 (Fed Funds rate).
        /// This is distinct from the FX spot price, which is stored in Snapshots["USD"].SpotPrice.
        /// </summary>
        public IReadOnlyDictionary<Currency, double> ForeignRates { get; init; }

        /// <summary>
        /// Market snapshots mapped by ticker (Underlying.Name).
        /// Contains all risk factors: equity assets and FX rates treated as underlyings.
        /// Example: Snapshots["AAPL"] (equity), Snapshots["USD"] (EUR/USD exchange rate).
        /// </summary>
        public IReadOnlyDictionary<string, MarketSnapshot> Snapshots { get; init; }

        /// <summary>
        /// Global correlation matrix spanning all risk factors (equity and FX).
        /// The Cholesky decomposition is pre-computed during initialization.
        /// </summary>
        public GlobalCorrelationMatrix Correlation { get; init; }

        /// <summary>The timestamp indicating when the environment was observed.</summary>
        public DateTimeOffset AsOf { get; init; }

        public MarketEnvironment(
            double domesticRate,
            IReadOnlyDictionary<Currency, double> foreignRates,
            IReadOnlyDictionary<string, MarketSnapshot> snapshots,
            GlobalCorrelationMatrix correlation,
            DateTimeOffset asOf)
        {
            DomesticRate = domesticRate;
            ForeignRates = foreignRates ?? throw new ArgumentNullException(nameof(foreignRates));
            Snapshots    = snapshots    ?? throw new ArgumentNullException(nameof(snapshots));
            Correlation  = correlation  ?? throw new ArgumentNullException(nameof(correlation));
            AsOf         = asOf;
        }

        // -------------------------------------------------------------------------
        // Factory methods
        // -------------------------------------------------------------------------

        /// <summary>
        /// Constructs a single-asset domestic (EUR) environment from a snapshot.
        /// Maintained for backward compatibility with older single-asset pricers.
        /// </summary>
        /// <param name="snap">The asset snapshot. Asset.NaturalCurrency is automatically inferred.</param>
        /// <param name="domesticRate">The EUR risk-free rate (defaults to 5%).</param>
        public static MarketEnvironment FromSingleAsset(
            MarketSnapshot snap,
            double domesticRate = 0.05)
        {
            var assetMapping = new[] { snap.Asset };
            var corr         = GlobalCorrelationMatrix.Identity(assetMapping);

            return new MarketEnvironment(
                domesticRate: domesticRate,
                foreignRates: new Dictionary<Currency, double>(),
                snapshots:    new Dictionary<string, MarketSnapshot> { { snap.Ticker, snap } },
                correlation:  corr,
                asOf:         snap.Timestamp
            );
        }

        /// <summary>
        /// Constructs a multi-asset environment containing only domestic equities (no FX).
        /// </summary>
        public static MarketEnvironment FromSnapshots(
            IEnumerable<MarketSnapshot> snaps,
            double domesticRate,
            GlobalCorrelationMatrix corr)
        {
            var list = snaps.ToList();
            if (list.Count == 0)
                throw new ArgumentException("At least one snapshot is required.", nameof(snaps));

            return new MarketEnvironment(
                domesticRate: domesticRate,
                foreignRates: new Dictionary<Currency, double>(),
                snapshots:    list.ToDictionary(s => s.Ticker, s => s),
                correlation:  corr,
                asOf:         list[0].Timestamp
            );
        }

        // -------------------------------------------------------------------------
        // Extract — filtrage pour pricing instrument / portefeuille
        // -------------------------------------------------------------------------

        /// <summary>
        /// Extracts a sub-environment containing only the requested assets.
        ///
        /// Used in the Rx flow to isolate data for specific pricing requests:
        ///   env.Extract(instrument.RequiredAssets) -> Pricing a single instrument.
        ///   env.Extract(portfolio.AllTickers) -> Joint pricing of a portfolio.
        ///
        /// Performance Optimization:
        ///   The PathGenerator receives the smallest necessary correlation matrix, improving CPU/L1 cache locality.
        ///   For example, a 3-asset basket uses a 3x3 matrix instead of the full NxN global matrix.
        /// </summary>
        /// <exception cref="MissingMarketDataException">Thrown if a requested ticker is missing from the snapshots.</exception>
        public MarketEnvironment Extract(IEnumerable<string> tickers)
        {
            var tickerList = tickers.Distinct().ToList();

            // 1. Valider la présence de tous les tickers
            var missing = tickerList.Where(t => !Snapshots.ContainsKey(t)).ToList();
            if (missing.Any())
                throw new MissingMarketDataException(
                    $"Tickers not found in MarketEnvironment: {string.Join(", ", missing)}");

            // 2. Filtrer les snapshots
            var filteredSnapshots = tickerList.ToDictionary(t => t, t => Snapshots[t]);

            // 3. Indices dans la matrice de corrélation globale
            var factorIndices = tickerList
                .Select(t => Correlation.IndexOf(t))
                .ToArray();

            // 4. Sous-matrice de corrélation (Cholesky recalculé sur le sous-ensemble)
            var subCorrelation = Correlation.Sub(factorIndices);

            // 5. Conserver uniquement les ForeignRates des devises impliquées
            var impliedCurrencies = filteredSnapshots.Values
                .Select(s => s.Asset.NaturalCurrency)
                .Where(c => c != Currency.EUR) // EUR = domestique, pas dans ForeignRates
                .Distinct()
                .ToHashSet();

            var filteredForeignRates = ForeignRates
                .Where(kvp => impliedCurrencies.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return new MarketEnvironment(
                domesticRate: DomesticRate,
                foreignRates: filteredForeignRates,
                snapshots:    filteredSnapshots,
                correlation:  subCorrelation,
                asOf:         AsOf
            );
        }
    }
}
