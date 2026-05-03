using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Entities
{
    public record Underlying(
        string Name,
        Currency NaturalCurrency);

    // Pour basket : IReadOnlyList<Underlying> dans BasketInstrument
    // Pour single : un seul Underlying

    // FxPair.cs — juste la donnée statique, pas la dynamique stochastique
    public record FxPair(
        Currency Base,      // USD
        Currency Quote,     // EUR
        double SpotRate);   // taux au moment du snapshot
                            // La dynamique GBM du FX vivra dans Pricing quand tu l'implémentes

    /// <summary>
    /// Wrapper enrichi autour de Underlying : porte en plus la volatilité et la classe d'actif.
    /// Utilisé par la simulation multi-assets pour construire le vecteur de facteurs de risque.
    ///
    /// Non-régression :
    ///   Le record Underlying existant reste inchangé.
    ///   UnderlyingWithCurrency est un NOUVEAU type qui ne remplace pas l'existant.
    ///   Le pricer mono-asset existant continue d'utiliser Underlying + MarketSnapshot.
    /// </summary>
    /// <param name="Underlying">Référence au Underlying de base (compatibilité totale).</param>
    /// <param name="Volatility">Volatilité annualisée de l'actif (σ).</param>
    /// <param name="AssetClass">Type de facteur dans le vecteur global (Equity, FX, Rate).</param>
    /// <param name="Spot">Prix spot actuel de l'actif.</param>
    public record UnderlyingWithCurrency(
        Underlying Underlying,
        double Spot,
        double Volatility,
        Enums.AssetClass AssetClass)
    {
        /// <summary>Nom de l'actif (délégation vers Underlying.Name).</summary>
        public string Name => Underlying.Name;

        /// <summary>Devise naturelle de l'actif (délégation vers Underlying.NaturalCurrency).</summary>
        public Currency NaturalCurrency => Underlying.NaturalCurrency;
    }
}
