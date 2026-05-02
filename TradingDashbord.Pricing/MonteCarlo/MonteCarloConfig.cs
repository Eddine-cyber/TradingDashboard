using System;
using System.Collections.Generic;
using System.Text;

namespace TradingDashbord.Pricing.MonteCarlo
{
    public class MonteCarloConfig
    {
        public int Steps { get; init; } = 252;
        public int NumberOfPaths { get; init; } = 100000;
        public double[] Perturbations { get; init; } = [0.01, 0.01, 0.01, 0.01, 1.0];
    }
}
