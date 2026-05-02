using System;
using System.Collections.Generic;
using System.Text;

namespace TradingDashbord.Pricing.MonteCarlo
{
    public class GBMPathSimulator
    {
        double Spot { get; init; }
        double Rate { get; init; }
        double Vol { get; init; }
        double MaturityInYears { get; init; }
        int Steps { get; init; }
        public GBMPathSimulator(double spot, double rate, double vol, double maturityInYears, int steps)
        {
            if (spot <= 0)
                throw new Exception("Spot must be strictly positive");

            if (vol < 0)
                throw new Exception("Volatility cannot be negative");

            if (maturityInYears <= 0)
                throw new Exception("Maturity must be strictly positive");

            if (steps <= 0)
                throw new Exception("Steps must be strictly positive");

            Spot = spot;
            Rate = rate;
            Vol = vol;
            MaturityInYears = maturityInYears;
            Steps = steps;
        }
        public double[] SimulatePath(Random rng)
        {
            double[] result = new double[Steps];
            double dt = MaturityInYears / Steps;
            double DriftPart = (Rate - Vol * Vol / 2) * dt;
            result[0] = Spot;
            for(int i = 1; i< Steps; i++)
            {
                double U1 = 1.0 - rng.NextDouble(), U2 = rng.NextDouble(); // because rng.NextDouble() ∈ [0.0, 1.0) so if 0.0 we xill have a probleme with the log
                double z = Math.Sqrt(-2 * Math.Log(U1)) * Math.Cos(2 * Math.PI * U2);
                result[i] = result[i - 1] * Math.Exp(DriftPart + Vol * Math.Sqrt(dt) * z);
            }
            return result;
        }
        public double[][] SimulatePaths(int numberOfPaths, Random rng)
        {
            double[][] results = new double[Steps][];
            for (int i = 0; i < numberOfPaths; i++)
            {
                results[i] = SimulatePath(rng);
            }
            return results;
        }
        public double[][] SimulatePathsParallel(int numberOfPaths, int? degreeOfParallelism = null)
        {
            double[][] results = new double[numberOfPaths][];
            ThreadLocal<Random> rng = new(() => new Random(Random.Shared.Next()));
            Parallel.For(0, numberOfPaths, i =>
            {
                results[i] = SimulatePath(rng.Value);
            });
            return results;
        }
    }
}
