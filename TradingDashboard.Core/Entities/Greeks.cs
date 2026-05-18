using System;

namespace TradingDashboard.Core.Entities
{
    /// <summary>
    /// Greeks of a financial instrument.
    /// Support addition and multiplication by a scalar.
    /// </summary>
    public struct Greeks
    {
        public double Delta       { get; set; }
        public double Gamma       { get; set; }
        public double Vega        { get; set; }
        public double Theta       { get; set; }
        public double Rho         { get; set; }
        public DateTimeOffset CalculatedAt { get; set; }

        public Greeks(double delta, double gamma, double vega, double theta, double rho, DateTimeOffset calculatedAt)
        {
            Delta       = delta;
            Gamma       = gamma;
            Vega        = vega;
            Theta       = theta;
            Rho         = rho;
            CalculatedAt = calculatedAt;
        }

        public static Greeks Zero() => new Greeks(0, 0, 0, 0, 0, DateTimeOffset.UtcNow);

        public static Greeks operator +(Greeks a, Greeks b) => new Greeks(
            a.Delta + b.Delta,
            a.Gamma + b.Gamma,
            a.Vega  + b.Vega,
            a.Theta + b.Theta,
            a.Rho   + b.Rho,
            DateTimeOffset.UtcNow);

        public static Greeks operator *(int quantity, Greeks g) => new Greeks(
            quantity * g.Delta,
            quantity * g.Gamma,
            quantity * g.Vega,
            quantity * g.Theta,
            quantity * g.Rho,
            DateTimeOffset.UtcNow);

        public override string ToString()
            => $"Δ={Delta:F4} Γ={Gamma:F4} ν={Vega:F4} Θ={Theta:F4} ρ={Rho:F4} @ {CalculatedAt:HH:mm:ss}";
    }
}
