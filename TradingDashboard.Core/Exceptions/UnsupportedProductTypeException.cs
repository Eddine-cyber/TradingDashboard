using System;
using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Exceptions
{
    public class UnsupportedProductTypeException : Exception
    {
        public ProductType ProductType { get; }

        public UnsupportedProductTypeException(ProductType type)
            : base($"The product type '{type}' is not supported by the current factory or engine.")
        {
            ProductType = type;
        }

        public UnsupportedProductTypeException(ProductType type, string message)
            : base(message)
        {
            ProductType = type;
        }

        public UnsupportedProductTypeException(ProductType type, string message, Exception innerException)
            : base(message, innerException)
        {
            ProductType = type;
        }
    }
}
