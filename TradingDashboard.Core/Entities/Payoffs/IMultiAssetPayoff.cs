using TradingDashboard.Core.Entities.Market;
using TradingDashboard.Core.Entities.Schedule;

namespace TradingDashboard.Core.Entities.Payoffs
{
    /// <summary>
    /// Abstraction de payoff pour les produits multi-assets.
    /// Reçoit les trajectoires de prix de TOUS les facteurs de risque
    /// simulés conjointement (equity + FX) et calcule le payoff non actualisé.
    ///
    /// Convention des paths :
    ///   paths[i][t] = prix du facteur i au pas t.
    ///   L'ordre des facteurs est celui de GlobalCorrelationMatrix.FactorIndex.
    ///   paths[i][0] = S0 du facteur i.
    ///
    /// IMPORTANT :
    ///   L'actualisation est appliquée par le pricer, pas par ce payoff.
    ///   Ce contrat ne connaît pas la mesure de probabilité utilisée.
    /// </summary>
    public interface IMultiAssetPayoff
    {
        /// <summary>Libellé du produit, utilisé dans les logs et rapports.</summary>
        string Name { get; }

        /// <summary>
        /// Calcule le payoff brut (non actualisé).
        /// </summary>
        /// <param name="paths">
        ///     Tableau [nbFacteurs][nbSteps+1] des trajectoires simulées conjointement.
        ///     L'ordre des facteurs est défini par GlobalCorrelationMatrix.FactorIndex.
        /// </param>
        /// <returns>Payoff non actualisé exprimé en devise domestique.</returns>
        double Compute(double[][] paths);

        /// <summary>
        /// Indices des facteurs utilisés par ce payoff dans la matrice globale.
        /// Permet au pricer de ne simuler que les facteurs nécessaires.
        /// </summary>
        IReadOnlyList<int> RequiredFactorIndices { get; }
    }
}
