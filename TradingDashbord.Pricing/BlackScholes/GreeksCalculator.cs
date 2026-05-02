using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Enums;
using TradingDashboard.Core.Entities;

namespace TradingDashbord.Pricing.BlackScholes
{
    internal static class GreeksCalculator
    {
        internal static Greeks Compute(ProductType type, double spot, double strike, double rate, double vol, double maturityInYears)
        {
            Greeks greek = new();
            double D1 = MathExtensions.d1(spot, strike, rate, vol, maturityInYears);

            double DeltaCall = D1.NormalCDF();
            greek.Delta = type == ProductType.Call ? DeltaCall : DeltaCall - 1;

            double nd1 = D1.NormalPDF();
            double sqrtMaturityInYears = Math.Sqrt(maturityInYears);
            greek.Gamma = nd1 / (spot*vol* sqrtMaturityInYears);
            greek.Vega = spot * nd1 * sqrtMaturityInYears;
            double D2 = MathExtensions.d2(spot, strike, rate, vol, maturityInYears);
            double Nd2 = D2.NormalCDF();
            greek.Theta = type == ProductType.Call
                ? (-spot * nd1 * vol / (2 * sqrtMaturityInYears) - rate * strike * Math.Exp(-rate * maturityInYears) * Nd2) / 365.0
                : (-spot * nd1 * vol / (2 * sqrtMaturityInYears) + rate * strike * Math.Exp(-rate * maturityInYears) * (1 - Nd2)) / 365.0;

            greek.Rho = type == ProductType.Call
                ? strike * maturityInYears * Math.Exp(-rate * maturityInYears) * Nd2 / 10000.0
                : -strike * maturityInYears * Math.Exp(-rate * maturityInYears) * (1 - Nd2) / 10000.0;

            greek.CalculatedAt = DateTimeOffset.UtcNow;

            return greek;
        }

    }
}
