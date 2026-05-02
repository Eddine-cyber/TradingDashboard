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

        public Greeks(double delta, double gamma, double vega, double rho, double theta, DateTimeOffset calculatedAt)
        {
            Delta = delta;
            Gamma = gamma;
            Vega = vega;
            Theta = theta;
            Rho = rho;
            Theta = theta;
            CalculatedAt = calculatedAt;
        }
        public double Delta { get; set; }
        public double Gamma { get; set; }
        public double Vega { get; set; }
        public double Theta { get; set; }
        public double Rho { get; set; }
        public DateTimeOffset CalculatedAt { get; set; }
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
