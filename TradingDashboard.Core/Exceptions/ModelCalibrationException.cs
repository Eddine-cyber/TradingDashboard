using System;

namespace TradingDashboard.Core.Exceptions
{
    public class ModelCalibrationException : Exception
    {
        public ModelCalibrationException()
            : base("The model calibration failed to converge or produced invalid parameters.")
        {
        }

        public ModelCalibrationException(string message)
            : base(message)
        {
        }

        public ModelCalibrationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
