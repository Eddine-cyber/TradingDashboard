using System;

namespace TradingDashboard.Core.Exceptions
{
    public class InvalidInstrumentException : Exception
    {
        public InvalidInstrumentException()
            : base("The instrument is invalid or incomplete.")
        {
        }

        public InvalidInstrumentException(string message)
            : base(message)
        {
        }

        public InvalidInstrumentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
