using System;

namespace TradingDashboard.Core.Exceptions
{
    public class PricingException : Exception
    {
        public PricingException()
            : base("An error occurred during the pricing of the instrument.")
        {
        }

        public PricingException(string message)
            : base(message)
        {
        }

        public PricingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
