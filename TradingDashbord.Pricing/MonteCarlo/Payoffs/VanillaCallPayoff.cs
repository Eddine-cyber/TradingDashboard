using System;
using System.Collections.Generic;
using System.Text;

namespace TradingDashbord.Pricing.MonteCarlo.Payoffs
{
    public class VanillaCallPayoff : IPayoff
    {
        public double Strike { get; init; }
        public string Name { get; init; }

        public VanillaCallPayoff(double strike)
        {
            Strike = strike;
            Name = "Vanilla Call Option Payoff";
        }

        public double Compute(double[] path)
        {
            if (path.Length == 0)
                throw new ArgumentException("Path should not be empty");

            return path[path.Length-1] > Strike ? path[path.Length - 1] - Strike : 0.0;
        }
    }
}

