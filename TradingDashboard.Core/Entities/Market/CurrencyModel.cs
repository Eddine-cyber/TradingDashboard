using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Entities.Market
{
    /// <summary>
    /// Représentation d'une devise dans le modèle multi-devises.
    /// Associe une devise (enum) à son taux sans risque domestique,
    /// utilisé pour actualiser les payoffs libellés dans cette devise.
    /// </summary>
    /// <param name="Currency">Devise identifiée par l'enum Currency.</param>
    /// <param name="RiskFreeRate">
    ///     Taux sans risque annualisé dans cette devise (ex : 0.05 = 5%).
    ///     Sous mesure risque-neutre domestique, chaque devise i a son propre ri.
    /// </param>
    /// <param name="Label">Libellé optionnel (ex : "EURIBOR 3M", "Fed Funds Rate").</param>
    public record CurrencyModel(
        Currency Currency,
        double RiskFreeRate,
        string? Label = null)
    {
        /// <summary>
        /// Facteur d'actualisation continu sur un horizon T (en années).
        /// e^{-r * T}
        /// </summary>
        public double DiscountFactor(double yearsToMaturity)
            => Math.Exp(-RiskFreeRate * yearsToMaturity);
    }
}
