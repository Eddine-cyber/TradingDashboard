using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Enums;
using TradingDashboard.Core.Interfaces;

namespace TradingDashbord.Pricing.BlackScholes
{
    internal class PricerFactory
    {
        IPricer<decimal> Create(ProductType typeProduct)
        {
            if (typeProduct.IsOption()) return new VanillaOptionPricer();
            else 

        }
    }
}
