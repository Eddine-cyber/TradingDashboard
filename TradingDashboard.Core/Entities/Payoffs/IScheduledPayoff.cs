using TradingDashboard.Core.Entities.Schedule;

namespace TradingDashboard.Core.Entities.Payoffs
{
    /// <summary>
    /// Abstraction de payoff pour les produits multi-fixings (multi-flux).
    /// Reçoit les trajectoires ET le calendrier de fixings ;
    /// le payoff est calculé sur les dates d'observation définies dans le schedule.
    ///
    /// Cas d'usage :
    ///   - Options asiatiques avec dates d'observation précises
    ///   - Accumulateurs
    ///   - Cliquets
    ///   - TARFs (Target Accrual Range Forwards)
    ///
    /// Relation avec IMultiAssetPayoff :
    ///   Un produit peut implémenter les deux interfaces si besoin.
    ///   IScheduledPayoff est préféré quand le calendrier est central au produit.
    /// </summary>
    public interface IScheduledPayoff
    {
        /// <summary>Libellé du produit.</summary>
        string Name { get; }

        /// <summary>Calendrier de fixings associé au produit.</summary>
        FixingSchedule Schedule { get; }

        /// <summary>
        /// Calcule le payoff sur le calendrier de fixings.
        /// </summary>
        /// <param name="paths">
        ///     Trajectoires simulées [nbFacteurs][nbSteps+1].
        /// </param>
        /// <param name="schedule">
        ///     Calendrier permettant d'identifier les indices de steps
        ///     correspondant aux dates de fixing.
        /// </param>
        /// <returns>Payoff non actualisé en devise domestique.</returns>
        double Compute(double[][] paths, FixingSchedule schedule);
    }
}
