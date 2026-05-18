using System;

namespace TradingDashboard.Core.Exceptions
{
    public class MissingMarketDataException : Exception
    {
        public MissingMarketDataException()
            : base("Required market data is missing.")
        {
        }

        public MissingMarketDataException(string message)
            : base(message)
        {
        }

        public MissingMarketDataException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
