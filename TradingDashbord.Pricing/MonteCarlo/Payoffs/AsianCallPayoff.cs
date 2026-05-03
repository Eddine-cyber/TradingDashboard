using System;
using System.Collections.Generic;
using System.Text;

namespace TradingDashbord.Pricing.MonteCarlo.Payoffs
{
    public class ArithmeticAsianCallPayoff : IPayoff
    {
        public int ObservationCount { get; init; }
        public double Strike { get; init; }
        public string Name { get; init; }
        public ArithmeticAsianCallPayoff(double strike, int observationCount)
        {
            if (observationCount == 0)
                throw new ArgumentException("ObservationCount shoud not be zero");

            ObservationCount = observationCount;
            Strike = strike;
            Name = "Arithmetic Asian Call Payoff";
        }
        public double Compute(double[] path)
        {
            double[] indices = GetObservationIndices(path.Length, ObservationCount);
            double mean = 0;
            foreach(int indice in indices)
            {
                mean += path[indice];
            }
            mean /= indices.Length;
            return mean > Strike ? mean - Strike : 0.0;
        }
        private double[] GetObservationIndices(int pathLength, int observationCount)
        {
            double[] indices = new double[observationCount];
            if (observationCount >= pathLength)
                throw new Exception("ObservationCount cannot be more ot equale to pathLength");
            int n = (pathLength - 1) / ObservationCount;
            for(int i = 1; i <= observationCount; i++)
    {
                indices[i-1] = n * i;
            }
            return indices;
        }
    }
}
