using TradingDashboard.Core.Entities.Market;

namespace TradingDashboard.Core.Interfaces
{
    /// <summary>
    /// Contrat extensible pour les simulateurs de trajectoires.
    /// Abstraction au-dessus de GBMPathSimulator existant (qui reste inchangé dans Pricing).
    ///
    /// Non-régression :
    ///   GBMPathSimulator dans Pricing.MonteCarlo continue à exister et fonctionner.
    ///   IPathSimulator est un contrat Core qui permet au module Pricing d'exposer
    ///   des simulateurs plus riches (multi-actifs, corrélés, Heston, etc.)
    ///   sans casser les usages existants.
    ///
    /// Implémentations futures attendues dans Pricing :
    ///   - GBMPathSimulatorAdapter : wrapper de GBMPathSimulator existant
    ///   - CorrelatedGBMSimulator : simulation multi-actifs avec Cholesky
    ///   - HestonSimulator : volatilité stochastique
    /// </summary>
    public interface IPathSimulator
    {
        /// <summary>
        /// Simule une trajectoire pour UN seul facteur de risque.
        /// Signature identique à GBMPathSimulator.SimulatePath pour la compatibilité.
        /// </summary>
        /// <param name="rng">Générateur aléatoire fourni par l'appelant (thread-safety).</param>
        /// <returns>Tableau [steps+1] incluant S0 en index 0.</returns>
        double[] SimulatePath(Random rng);

        /// <summary>
        /// Simule N trajectoires indépendantes pour un seul facteur.
        /// Compatibilité directe avec GBMPathSimulator.SimulatePaths.
        /// </summary>
        double[][] SimulatePaths(int numberOfPaths, Random rng);

        /// <summary>
        /// Version parallélisée (Parallel.For + ThreadLocal&lt;Random&gt;).
        /// Compatibilité directe avec GBMPathSimulator.SimulatePathsParallel.
        /// </summary>
        double[][] SimulatePathsParallel(int numberOfPaths, int? degreeOfParallelism = null);

        /// <summary>Nombre de pas de discrétisation.</summary>
        int Steps { get; }

        /// <summary>Horizon de simulation en années.</summary>
        double MaturityInYears { get; }
    }

    /// <summary>
    /// Extension de IPathSimulator pour la simulation multi-facteurs corrélée.
    /// Utilisé pour FX + equity jointly, basket, hybrid, quanto.
    ///
    /// Retourne paths[facteurIndex][stepIndex], où l'ordre des facteurs
    /// correspond à GlobalCorrelationMatrix.FactorIndex.
    /// </summary>
    public interface IMultiFactorPathSimulator : IPathSimulator
    {
        /// <summary>Nombre de facteurs de risque simulés conjointement.</summary>
        int NumberOfFactors { get; }

        /// <summary>
        /// Simule une trajectoire multi-facteurs corrélée.
        /// </summary>
        /// <param name="rng">Générateur aléatoire (thread-local recommandé).</param>
        /// <returns>
        ///     Tableau [nbFacteurs][steps+1].
        ///     result[i] = trajectoire du facteur i (équité ou FX).
        /// </returns>
        double[][] SimulateMultiFactorPath(Random rng);

        /// <summary>
        /// Simule N scénarios multi-facteurs en parallèle.
        /// </summary>
        /// <returns>
        ///     Tableau [nbScénarios][nbFacteurs][steps+1].
        /// </returns>
        double[][][] SimulateMultiFactorPathsParallel(
            int numberOfPaths,
            int? degreeOfParallelism = null);

        /// <summary>
        /// Matrice de corrélation utilisée par ce simulateur.
        /// Permet au pricer de vérifier la cohérence avec l'instrument.
        /// </summary>
        GlobalCorrelationMatrix CorrelationMatrix { get; }
    }
}
