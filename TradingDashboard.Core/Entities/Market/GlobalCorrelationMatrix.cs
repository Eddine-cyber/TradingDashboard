using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Entities.Market
{
    /// <summary>
    /// Identifiant unique d'un facteur de risque dans le vecteur de simulation.
    /// Permet de construire l'ordre strict des facteurs dans la matrice de corrélation.
    ///
    /// Ordre de construction du vecteur de facteurs :
    ///   1. Actifs equity domestiques  : (AssetClass.Equity, DomesticCurrency, underlyingName)
    ///   2. Actifs equity étrangers    : (AssetClass.Equity, foreignCurrency_i, underlyingName)
    ///      par devise i puis par actif ℓ au sein de la devise
    ///   3. FX pour chaque devise i    : (AssetClass.FX, foreignCurrency_i, null)
    ///
    /// Cet ordre est celui que MonteCarloMultiAssetSimulator respectera.
    /// </summary>
    /// <param name="AssetClass">Type de facteur : Equity, FX ou Rate.</param>
    /// <param name="Currency">Devise associée au facteur.</param>
    /// <param name="UnderlyingName">
    ///     Nom de l'actif pour les equity (ex : "AAPL").
    ///     Null pour les facteurs FX (le nom de la paire est implicite).
    /// </param>
    public record RiskFactorKey(
        AssetClass AssetClass,
        Currency Currency,
        string? UnderlyingName);

    /// <summary>
    /// Matrice de corrélation globale entre tous les facteurs de risque
    /// du marché multi-devises : equity domestiques, equity étrangers, FX.
    ///
    /// Architecture mathématique :
    ///   Le vecteur de Browniens est (W_S0,1,...,W_S0,L0, W_S1,1,...,W_SN,LN, W_X1,...,W_XN).
    ///   dim = Σ L_i + N   où L_i = nb actifs dans devise i, N = nb devises étrangères.
    ///
    /// La matrice doit être définie positive (toutes valeurs propres > 0).
    /// La factorisation de Cholesky est utilisée par le simulateur MC.
    /// </summary>
    public record GlobalCorrelationMatrix
    {
        /// <summary>Mapping ordonné facteur → index colonne/ligne dans la matrice.</summary>
        public IReadOnlyList<RiskFactorKey> FactorIndex { get; init; }

        /// <summary>
        /// Matrice de corrélation N×N (N = nombre de facteurs).
        /// Stockée ligne par ligne : Matrix[i][j] = ρ(facteur i, facteur j).
        /// Doit être symétrique avec des 1 sur la diagonale.
        /// </summary>
        public double[][] Matrix { get; init; }

        /// <summary>
        /// Dimension de la matrice (nombre de facteurs de risque).
        /// </summary>
        public int Dimension => FactorIndex.Count;

        public GlobalCorrelationMatrix(IReadOnlyList<RiskFactorKey> factorIndex, double[][] matrix)
        {
            if (factorIndex.Count != matrix.Length)
                throw new ArgumentException(
                    $"FactorIndex count ({factorIndex.Count}) must match matrix dimension ({matrix.Length}).");

            for (int i = 0; i < matrix.Length; i++)
            {
                if (matrix[i].Length != matrix.Length)
                    throw new ArgumentException(
                        $"Matrix row {i} has {matrix[i].Length} elements, expected {matrix.Length}.");
            }

            FactorIndex = factorIndex;
            Matrix = matrix;
        }

        /// <summary>
        /// Retourne l'index (ligne/colonne) d'un facteur dans la matrice.
        /// Lève ArgumentException si le facteur n'est pas trouvé.
        /// </summary>
        public int IndexOf(RiskFactorKey key)
        {
            for (int i = 0; i < FactorIndex.Count; i++)
                if (FactorIndex[i] == key) return i;

            throw new ArgumentException(
                $"RiskFactor {key} not found in GlobalCorrelationMatrix.");
        }

        /// <summary>
        /// Corrélation entre deux facteurs identifiés par leurs clés.
        /// </summary>
        public double GetCorrelation(RiskFactorKey a, RiskFactorKey b)
            => Matrix[IndexOf(a)][IndexOf(b)];

        /// <summary>
        /// Construit une matrice d'identité (tous facteurs indépendants).
        /// Utile pour les tests et comme valeur par défaut.
        /// </summary>
        public static GlobalCorrelationMatrix Identity(IReadOnlyList<RiskFactorKey> factors)
        {
            int n = factors.Count;
            double[][] m = new double[n][];
            for (int i = 0; i < n; i++)
            {
                m[i] = new double[n];
                m[i][i] = 1.0;
            }
            return new GlobalCorrelationMatrix(factors, m);
        }

        /// <summary>
        /// Factorisation de Cholesky : L tel que L * L^T = Matrice.
        /// Retourne null si la matrice n'est pas définie positive.
        /// Utilisée par les simulateurs MC pour la génération de Browniens corrélés.
        /// </summary>
        public double[][]? CholeskyDecomposition()
        {
            int n = Dimension;
            double[][] L = new double[n][];
            for (int i = 0; i < n; i++) L[i] = new double[n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    double sum = Matrix[i][j];
                    for (int k = 0; k < j; k++)
                        sum -= L[i][k] * L[j][k];

                    if (i == j)
                    {
                        if (sum <= 0) return null; // matrice non définie positive
                        L[i][j] = Math.Sqrt(sum);
                    }
                    else
                    {
                        L[i][j] = sum / L[j][j];
                    }
                }
            }
            return L;
        }
    }
}
