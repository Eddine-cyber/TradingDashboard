using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace TradingDashboard.Core.Entities
{
    public struct Greeks
    {
        private double _delta;
        private double _gamma;
        private double _vega;
        private double _theta;
        private double _rho;
        private DateTimeOffset _calculatedAt;

        public double Delta
        {
            get => _delta;
            set {
                if (value > 1 || value < -1)
                    throw new Exception("Delta must be between -1 and 1");
                _delta = value;
            }
        }

        public double Gamma
        {
            get => _gamma;
            set
            {
                if (value < 0)
                    throw new Exception("Gamma must be positive");
                _gamma = value;
            }
        }

        public double Vega
        {
            get => _vega;
            set
            {
                if (value < 0)
                    throw new Exception("Vega must be positive");
                _vega = value;
            }
        }
        public double Rho { 
            get => _rho;
            set { _rho = value; } 
        }

        public double Theta
        {
            get => _theta;
            set
            {
                if (value > 0)
                    throw new Exception("Theta must be negative");
                _theta = value;
            }
        }

        public DateTimeOffset CalculatedAt
        {
            get => _calculatedAt;
            set { _calculatedAt = value; }
        }

        public bool IsValide()
        {
            return true;
        }

        public static Greeks operator + (Greeks first, Greeks second)
        {

            first.Delta += second.Delta; 
            first.Gamma += second.Gamma;
            first.Vega += second.Vega;
            first.Theta += second.Theta;
            first.Rho += second.Rho;
            first.CalculatedAt = DateTime.Now;

            return first;
        }

        public static Greeks Zero()
        {
            Greeks res = new();
            res.CalculatedAt = DateTime.Now;
            return res;
        }
    }
}
