using System;
using System.Collections.Generic;
using System.Text;

namespace TradingDashbord.Pricing.MonteCarlo
{
    public class MonteCarloConfig
    {
        public int Steps { get; set; } = 252;
        public int NumberOfPaths { get; set; } = 100000;
        public double[] Perturbations { get; set; } = [0.01, 0.01, 0.01, 0.01, 1.0]; // Delta, Gamma, Vega, Rho, Theta 
    }
}
