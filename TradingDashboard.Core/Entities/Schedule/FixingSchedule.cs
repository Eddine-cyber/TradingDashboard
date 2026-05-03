using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Entities.Schedule
{
    /// <summary>
    /// Calendrier de fixings pour les produits multi-flux (asian, cliquets, accumulateurs…).
    /// Regroupe les dates d'observation du sous-jacent (fixings) et les dates de paiement.
    ///
    /// Convention :
    ///   - Les FixingDates sont les dates où le spot est observé / fixé.
    ///   - Les PaymentDates sont les dates où le flux de paiement est versé.
    ///   - Pour un produit simple (un seul fixing = une seule maturité),
    ///     FixingDates = { Maturity } et PaymentDates = { Maturity }.
    /// </summary>
    public record FixingSchedule
    {
        /// <summary>Dates d'observation du sous-jacent, triées chronologiquement.</summary>
        public IReadOnlyList<DateOnly> FixingDates { get; init; }

        /// <summary>
        /// Dates de paiement associées. Doit avoir le même nombre d'éléments que FixingDates
        /// (relation 1-1 fixing → paiement) ou être vide si les paiements sont à maturité.
        /// </summary>
        public IReadOnlyList<DateOnly> PaymentDates { get; init; }

        /// <summary>Convention de comptage de jours pour le calcul des fractions d'année.</summary>
        public DayCountConvention DayCount { get; init; }

        /// <summary>Nombre de fixings.</summary>
        public int Count => FixingDates.Count;

        public FixingSchedule(
            IReadOnlyList<DateOnly> fixingDates,
            IReadOnlyList<DateOnly>? paymentDates = null,
            DayCountConvention dayCount = DayCountConvention.Act252)
        {
            if (fixingDates.Count == 0)
                throw new ArgumentException("FixingSchedule must have at least one fixing date.");

            // Vérification ordre chronologique
            for (int i = 1; i < fixingDates.Count; i++)
                if (fixingDates[i] <= fixingDates[i - 1])
                    throw new ArgumentException(
                        $"FixingDates must be strictly increasing. Violation at index {i}.");

            FixingDates = fixingDates;
            PaymentDates = paymentDates ?? fixingDates; // même date si non précisé
            DayCount = dayCount;
        }

        /// <summary>
        /// Fractions d'année (yearfracs) depuis une date de référence vers chaque fixing.
        /// Utile pour le simulateur MC pour déterminer les indices de path.
        /// </summary>
        public IReadOnlyList<double> GetYearFractions(DateOnly referenceDate)
        {
            double divisor = DayCount switch
            {
                DayCountConvention.Act365 => 365.0,
                DayCountConvention.Act252 => 252.0,
                DayCountConvention.Act360 => 360.0,
                _ => 252.0
            };
            return FixingDates
                .Select(d => (d.DayNumber - referenceDate.DayNumber) / divisor)
                .ToList();
        }

        /// <summary>
        /// Construit un schedule simple (un seul fixing à maturité).
        /// Compatible avec les instruments mono-flux existants.
        /// </summary>
        public static FixingSchedule Single(DateOnly maturity)
            => new FixingSchedule(new[] { maturity });

        /// <summary>
        /// Construit un schedule d'observations régulières entre start et end.
        /// </summary>
        /// <param name="start">Date de début (exclue).</param>
        /// <param name="end">Date de fin / maturité (incluse).</param>
        /// <param name="count">Nombre d'observations.</param>
        public static FixingSchedule Uniform(DateOnly start, DateOnly end, int count)
        {
            if (count <= 0) throw new ArgumentException("count must be positive.");
            int totalDays = end.DayNumber - start.DayNumber;
            var dates = Enumerable.Range(1, count)
                .Select(i => DateOnly.FromDayNumber(
                    start.DayNumber + (int)Math.Round((double)i * totalDays / count)))
                .ToList();
            return new FixingSchedule(dates);
        }
    }

    /// <summary>Convention de comptage de jours pour les fractions d'année.</summary>
    public enum DayCountConvention
    {
        Act252,   // jours de trading — convention utilisée dans le système existant
        Act365,
        Act360
    }
}
