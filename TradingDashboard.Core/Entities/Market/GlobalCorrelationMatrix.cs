using System;
using TradingDashboard.Core.Enums;
using TradingDashboard.Core.Exceptions;

namespace TradingDashboard.Core.Entities.Market
{
    /// <summary>
    /// Global correlation matrix encompassing all risk factors.
    ///
    /// Factor Ordering Convention:
    ///   [ Domestic Assets | Foreign Assets grouped by currency | FX Rates grouped by currency ]
    ///
    /// The Cholesky decomposition L (where L * L^T = Rho) is pre-computed during construction.
    /// If the matrix is not positive definite, an InvalidCorrelationMatrixException is thrown immediately.
    /// </summary>
    public record GlobalCorrelationMatrix
    {
        /// <summary>The NxN correlation matrix.</summary>
        public double[,] Rho { get; }

        /// <summary>
        /// Ordered mapping where index 'i' corresponds to a specific Underlying risk factor.
        /// </summary>
        public Underlying[] AssetMapping { get; }

        /// <summary>The pre-computed Cholesky factor L such that L * L^T = Rho.</summary>
        public double[,] CholeskyFactor { get; }

        public int Dimension => AssetMapping.Length;

        public GlobalCorrelationMatrix(Underlying[] assetMapping, double[,] rho)
        {
            AssetMapping = assetMapping ?? throw new ArgumentNullException(nameof(assetMapping));
            Rho          = rho          ?? throw new ArgumentNullException(nameof(rho));

            if (rho.GetLength(0) != assetMapping.Length || rho.GetLength(1) != assetMapping.Length)
                throw new ArgumentException("Correlation matrix dimensions must match the number of assets.");

            Validate();

            CholeskyFactor = ComputeCholesky()
                ?? throw new InvalidCorrelationMatrixException("The correlation matrix is not positive definite.");
        }

        // -------------------------------------------------------------------------
        // Validation
        // -------------------------------------------------------------------------

        public void Validate()
        {
            int n = Dimension;
            for (int i = 0; i < n; i++)
            {
                if (Math.Abs(Rho[i, i] - 1.0) > 1e-9)
                    throw new InvalidCorrelationMatrixException($"Diagonal element at ({i},{i}) is not 1.");

                for (int j = 0; j < n; j++)
                {
                    if (Math.Abs(Rho[i, j] - Rho[j, i]) > 1e-9)
                        throw new InvalidCorrelationMatrixException($"Matrix is not symmetric at ({i},{j}).");
                }
            }
        }

        // -------------------------------------------------------------------------
        // Lookup
        // -------------------------------------------------------------------------

        /// <summary>
        /// Returns the index of the risk factor identified by its ticker.
        /// For FX assets, the ticker is the foreign currency code (e.g., "USD").
        /// </summary>
        public int IndexOf(string ticker)
        {
            for (int i = 0; i < AssetMapping.Length; i++)
            {
                if (AssetMapping[i].Name == ticker)
                    return i;
            }
            throw new ArgumentException($"Underlying with ticker '{ticker}' not found in correlation matrix.");
        }

        /// <summary>Overload to specify the AssetClass in case multiple assets share the same ticker.</summary>
        public int IndexOf(string ticker, AssetClass assetClass)
        {
            for (int i = 0; i < AssetMapping.Length; i++)
            {
                if (AssetMapping[i].Name == ticker && AssetMapping[i].AssetClass == assetClass)
                    return i;
            }
            throw new ArgumentException($"Underlying '{ticker}' with class {assetClass} not found.");
        }

        // -------------------------------------------------------------------------
        // Sous-matrice
        // -------------------------------------------------------------------------

        /// <summary>
        /// Extracts a sub-correlation matrix for a specific subset of risk factors.
        /// Used by MarketEnvironment.Extract() and PortfolioPricer for optimization.
        /// </summary>
        public GlobalCorrelationMatrix Sub(int[] indices)
        {
            int newN = indices.Length;
            var subMapping = new Underlying[newN];
            var subRho     = new double[newN, newN];

            for (int i = 0; i < newN; i++)
            {
                subMapping[i] = AssetMapping[indices[i]];
                for (int j = 0; j < newN; j++)
                    subRho[i, j] = Rho[indices[i], indices[j]];
            }

            return new GlobalCorrelationMatrix(subMapping, subRho);
        }

        // -------------------------------------------------------------------------
        // Factories
        // -------------------------------------------------------------------------

        /// <summary>Creates a 1x1 identity matrix for a single asset (used for backward compatibility).</summary>
        public static GlobalCorrelationMatrix Identity(Underlying[] assets)
        {
            int n = assets.Length;
            double[,] m = new double[n, n];
            for (int i = 0; i < n; i++)
                m[i, i] = 1.0;
            return new GlobalCorrelationMatrix(assets, m);
        }

        // -------------------------------------------------------------------------
        // Cholesky privé
        // -------------------------------------------------------------------------

        private double[,]? ComputeCholesky()
        {
            int n = Dimension;
            double[,] L = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    double sum = Rho[i, j];
                    for (int k = 0; k < j; k++)
                        sum -= L[i, k] * L[j, k];

                    if (i == j)
                    {
                        if (sum <= 1e-12) return null;
                        L[i, j] = Math.Sqrt(sum);
                    }
                    else
                    {
                        L[i, j] = sum / L[j, j];
                    }
                }
            }
            return L;
        }
    }
}
