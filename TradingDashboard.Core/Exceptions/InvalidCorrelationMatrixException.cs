using System;

namespace TradingDashboard.Core.Exceptions
{
    public class InvalidCorrelationMatrixException : Exception
    {
        public InvalidCorrelationMatrixException()
            : base("The correlation matrix is invalid. It must be symmetric, have 1s on the diagonal, and be positive definite.")
        {
        }

        public InvalidCorrelationMatrixException(string message)
            : base(message)
        {
        }

        public InvalidCorrelationMatrixException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
