using TradingDashboard.Core.Entities;

namespace TradingDashboard.Core.Entities
{
    /// <summary>
    /// Result of a pricing calculation (analytic or Monte Carlo).
    /// </summary>
    public record PricingResult
    {
        /// <summary>Theoretical discounted price in domestic currency.</summary>
        public double TheoreticalPrice { get; init; }

        /// <summary>Greeks computed alongside the price.</summary>
        public Greeks Greeks { get; init; }

        /// <summary>Calculation timestamp.</summary>
        public DateTimeOffset ComputedAt { get; init; }

        /// <summary>Pricing model used (e.g. Black-Scholes, Monte Carlo).</summary>
        public string PricingMethod { get; init; }

        /// <summary>Monte Carlo standard error. Null for analytic models.</summary>
        public double? StandardError { get; init; }

        /// <summary>95% confidence interval half-width. Null for analytic models.</summary>
        public double? ConfidenceInterval95 { get; init; }

        public PricingResult(
            double theoreticalPrice,
            Greeks greeks,
            string pricingMethod,
            double? standardError = null,
            double? confidenceInterval95 = null)
        {
            if (string.IsNullOrWhiteSpace(pricingMethod))
                throw new ArgumentException("PricingMethod cannot be null or empty.", nameof(pricingMethod));

            TheoreticalPrice = theoreticalPrice;
            Greeks = greeks;
            PricingMethod = pricingMethod;
            StandardError = standardError;
            ConfidenceInterval95 = confidenceInterval95;
            ComputedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>True if the result comes from a Monte Carlo run.</summary>
        public bool IsMonteCarlo => StandardError.HasValue;
    }
}