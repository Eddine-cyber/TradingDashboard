using System;
using System.Collections.Generic;
using System.Text;

namespace TradingDashbord.Pricing.MonteCarlo.Payoffs
{
    public class LookbackFixedStrikePutPayoff : IPayoff
    {
        public double Strike { get; init; }
        public string Name { get; init; }
        public LookbackFixedStrikePutPayoff(double strike)
        {
            Strike = strike;
            Name = "Lookback Fixed Strike Put";
        }

        public double Compute(double[] path)
    {
            if (path.Length == 0)
                throw new ArgumentException("Path should not be empty");

            double max = path.Max();
            return max < Strike ? Strike - max : 0.0;
        }
    }
}
