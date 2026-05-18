using System;

namespace TradingDashboard.Core.Exceptions
{
    public class RiskLimitExceededException : Exception
    {
        public RiskLimitExceededException()
            : base("A defined risk limit was exceeded.")
        {
        }

        public RiskLimitExceededException(string message)
            : base(message)
        {
        }

        public RiskLimitExceededException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
