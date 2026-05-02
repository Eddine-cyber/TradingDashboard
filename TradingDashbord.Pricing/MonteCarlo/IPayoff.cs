using System;
using System.Collections.Generic;
using System.Text;

namespace TradingDashbord.Pricing.MonteCarlo
{
    public interface IPayoff
    {
        public double Compute(double[] path);
        public string Name{ get; init; }
    }
}
