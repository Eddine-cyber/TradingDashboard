using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Entities.Schedule
{
    /// <summary>
    /// Fixing schedule for multi-flow products (e.g., Asian options, cliquets).
    /// Aggregates the underlying asset observation dates (fixings) and the corresponding payment dates.
    ///
    /// Convention:
    ///   - FixingDates: Dates on which the underlying spot is observed or fixed.
    ///   - PaymentDates: Dates on which the cash flow is settled.
    ///   - For single-flow products (one fixing, one payment at maturity):
    ///     FixingDates = { Maturity } and PaymentDates = { Maturity }.
    /// </summary>
    public record FixingSchedule
    {
        /// <summary>Chronologically ordered dates for underlying asset observation.</summary>
        public IReadOnlyList<DateOnly> FixingDates { get; init; }

        /// <summary>
        /// Associated payment dates. Must match the length of FixingDates (1-to-1 relationship),
        /// or can be empty if all payments are deferred to maturity.
        /// </summary>
        public IReadOnlyList<DateOnly> PaymentDates { get; init; }

        /// <summary>Day count convention used for calculating year fractions.</summary>
        public DayCountConvention DayCount { get; init; }

        /// <summary>Total number of scheduled fixings.</summary>
        public int Count => FixingDates.Count;

        public FixingSchedule(
            IReadOnlyList<DateOnly> fixingDates,
            IReadOnlyList<DateOnly>? paymentDates = null,
            DayCountConvention dayCount = DayCountConvention.Act252)
        {
            if (fixingDates.Count == 0)
                throw new ArgumentException("FixingSchedule must have at least one fixing date.");

            // Enforce chronological order
            for (int i = 1; i < fixingDates.Count; i++)
                if (fixingDates[i] <= fixingDates[i - 1])
                    throw new ArgumentException(
                        $"FixingDates must be strictly increasing. Violation at index {i}.");

            FixingDates = fixingDates;
            PaymentDates = paymentDates ?? fixingDates; // default to fixing dates if omitted
            DayCount = dayCount;
        }

        /// <summary>
        /// Computes the year fractions from a given reference date to each fixing date.
        /// Used by the Monte Carlo simulator to determine path indices.
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
        /// Constructs a simple schedule consisting of a single fixing at maturity.
        /// Designed for compatibility with existing single-flow instruments.
        /// </summary>
        public static FixingSchedule Single(DateOnly maturity)
            => new FixingSchedule(new[] { maturity });

        /// <summary>
        /// Constructs a schedule with evenly distributed observation dates between the start and end dates.
        /// </summary>
        /// <param name="start">The start date (exclusive).</param>
        /// <param name="end">The end date/maturity (inclusive).</param>
        /// <param name="count">The total number of observations.</param>
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

    /// <summary>Day count convention for year fraction calculation.</summary>
    public enum DayCountConvention
    {
        Act252,   // trading days - convention used in the existing system
        Act365,
        Act360
    }
}
