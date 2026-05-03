using TradingDashboard.Core.Entities;
using TradingDashboard.Core.Entities.Market;
using TradingDashboard.Core.Entities.Payoffs;
using TradingDashboard.Core.Entities.Schedule;
using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Interfaces
{
    /// <summary>
    /// Contrat de pricing pour les produits multi-assets.
    /// Extension de IPricer existant, compatible avec le MarketEnvironment global.
    ///
    /// Non-régression :
    ///   IPricer&lt;TResult&gt; existant (mono-asset / MarketSnapshot) reste inchangé.
    ///   IMultiAssetPricer est une nouvelle interface parallèle, sans remplacement.
    ///
    /// Implémentations futures attendues :
    ///   - MonteCarloMultiAssetPricer (Pricing module)
    ///   - FxOptionPricer (Garman-Kohlhagen)
    ///   - BasketOptionPricer
    /// </summary>
    /// <typeparam name="TResult">Type de résultat retourné (ex: PricingResult).</typeparam>
    public interface IMultiAssetPricer<TResult>
    {
        /// <summary>
        /// Calcule le prix d'un instrument multi-assets dans un environnement de marché global.
        /// </summary>
        /// <param name="instrument">Instrument à pricer (peut être BasketInstrument, FxOptionInstrument, etc.).</param>
        /// <param name="environment">Environnement de marché global multi-devises.</param>
        /// <param name="ct">Token d'annulation.</param>
        Task<TResult> CalculatePrice(
            Instrument instrument,
            MarketEnvironment environment,
            CancellationToken ct = default);

        /// <summary>
        /// Calcule uniquement le prix théorique (sans Greeks) pour usage rapide.
        /// </summary>
        Task<double> CalculatePriceOnly(
            Instrument instrument,
            MarketEnvironment environment,
            CancellationToken ct = default);

        /// <summary>
        /// Calcule les Greeks dans le contexte multi-devises.
        /// Delta est un vecteur (un par facteur de risque), Vega idem.
        /// Retourne les Greeks agrégés (delta net, gamma net, vega net) compatibles
        /// avec la structure Greeks existante, pour non-régression avec Position et PnL.
        /// </summary>
        Task<Greeks> CalculateGreeks(
            Instrument instrument,
            MarketEnvironment environment,
            CancellationToken ct = default);

        /// <summary>
        /// Types de produits supportés par cette implémentation.
        /// </summary>
        IReadOnlyList<ProductType> SupportedProducts { get; }
    }
}
