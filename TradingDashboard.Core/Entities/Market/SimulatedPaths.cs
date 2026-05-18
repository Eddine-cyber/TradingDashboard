using System;
using System.Linq;
using TradingDashboard.Core.Entities;

namespace TradingDashboard.Core.Entities.Market
{
    /// <summary>
    /// Tensor of simulated paths with dimensions: [nFactors, nPaths, nSteps+1].
    /// Serves as the standard output for all path generators and standard input for all payoffs.
    ///
    /// Dimension 0: Risk factors (e.g., equities, FX rates for multi-currency).
    /// Dimension 1: Monte Carlo scenarios (paths).
    /// Dimension 2: Discrete time steps (index 0 corresponds to the initial spot S_0).
    /// </summary>
    public class SimulatedPaths
    {
        public double[,,] Paths      { get; }
        public Underlying[] AssetMapping { get; }
        public int NFactors { get; }
        public int NPaths   { get; }
        public int NSteps   { get; }

        public SimulatedPaths(double[,,] paths, Underlying[] assetMapping)
        {
            Paths        = paths        ?? throw new ArgumentNullException(nameof(paths));
            AssetMapping = assetMapping ?? throw new ArgumentNullException(nameof(assetMapping));

            NFactors = paths.GetLength(0);
            NPaths   = paths.GetLength(1);
            NSteps   = paths.GetLength(2) - 1; // index 0 = S_0

            if (NFactors != assetMapping.Length)
                throw new ArgumentException("Number of factors in Paths must match AssetMapping length.");
        }

        // -------------------------------------------------------------------------
        // Extract — used by IPayoff to isolate its specific slice of the joint tensor
        // -------------------------------------------------------------------------

        /// <summary>Extracts a sub-tensor based on a set of factor indices.</summary>
        public SimulatedPaths Extract(int[] factorIndices)
        {
            int newN = factorIndices.Length;
            var newMapping = new Underlying[newN];
            var newPaths   = new double[newN, NPaths, NSteps + 1];

            for (int i = 0; i < newN; i++)
            {
                int fi = factorIndices[i];
                newMapping[i] = AssetMapping[fi];
                for (int j = 0; j < NPaths; j++)
                    for (int k = 0; k <= NSteps; k++)
                        newPaths[i, j, k] = Paths[fi, j, k];
            }

            return new SimulatedPaths(newPaths, newMapping);
        }

        /// <summary>Extracts a sub-tensor based on ticker names.</summary>
        public SimulatedPaths Extract(string[] tickers)
        {
            var indices = tickers.Select(ticker =>
            {
                int idx = Array.FindIndex(AssetMapping, a => a.Name == ticker);
                if (idx < 0) throw new ArgumentException($"Ticker '{ticker}' not found in SimulatedPaths.");
                return idx;
            }).ToArray();

            return Extract(indices);
        }

        // -------------------------------------------------------------------------
        // Individual trajectory access
        // -------------------------------------------------------------------------

        /// <summary>Returns the complete trajectory [S_0, S_1, ..., S_T] for a specific factor and scenario.</summary>
        public double[] GetPathForFactor(int factorIndex, int pathIndex)
        {
            var path = new double[NSteps + 1];
            for (int k = 0; k <= NSteps; k++)
                path[k] = Paths[factorIndex, pathIndex, k];
            return path;
        }

        /// <summary>Returns the array of terminal values (S_T) for a specific factor across all scenarios.</summary>
        public double[] GetTerminalValues(int factorIndex)
        {
            var terminal = new double[NPaths];
            for (int j = 0; j < NPaths; j++)
                terminal[j] = Paths[factorIndex, j, NSteps];
            return terminal;
        }
    }
}
