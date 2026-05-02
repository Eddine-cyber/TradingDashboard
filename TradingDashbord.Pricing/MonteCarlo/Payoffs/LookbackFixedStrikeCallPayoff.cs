using System;
using System.Collections.Generic;
using System.Text;

namespace TradingDashbord.Pricing.MonteCarlo.Payoffs
{
    public class LookbackFixedStrikeCallPayoff : IPayoff
    {
        public double Strike { get; init; }
        public string Name { get; init; }

        public LookbackFixedStrikeCallPayoff(double strike)
        {
            Strike = strike;
            Name = "Lookback Fixed Strike Call";
        }

        public double Compute(double[] path)
        {
            if (path.Length == 0)
                throw new ArgumentException("Path should not be empty");

            double max = path.Max();
            return max > Strike ? max - Strike : 0.0;
        }
    }
}
