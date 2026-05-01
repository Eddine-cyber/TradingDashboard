using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Enums;
using TradingDashboard.Core.Interfaces;
using TradingDashbord.Pricing.BlackScholes;

namespace TradingDashbord.Pricing
{
    internal class PricerFactory
    {
        internal IPricer<decimal> Create(ProductType typeProduct)
        {
            if (typeProduct.IsOption()) return new VanillaOptionPricer(typeProduct);
            else
            {
                throw new Exception("UnsupportedProductTypeException");
            }
        }
    }
}
