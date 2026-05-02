using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Enums;
using TradingDashboard.Core.Interfaces;
using TradingDashbord.Pricing.BlackScholes;
using TradingDashbord.Pricing.MonteCarlo;

namespace TradingDashbord.Pricing.Strategy
{
    internal class PricerFactory
    {
        internal IPricingStrategy Create(ProductType typeProduct, MonteCarloConfig config = null)
        {
            if (typeProduct.IsVanilla()) return new BlackScholesPricingStrategy();
            else if (typeProduct.IsExotic()) return new MonteCarloPricingStrategy(config);
            else throw new NotImplementedException("PricerFactory in not implemented yet for forward and future");
        }
    }
}
