using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Entities.Market
{
    /// <summary>
    /// Environnement de marché global multi-devises.
    /// Point d'entrée unique pour toutes les données de marché nécessaires
    /// au pricing d'instruments multi-assets, FX, quanto et cross-currency.
    ///
    /// Structure :
    ///   - 1 devise domestique avec son taux sans risque
    ///   - N devises étrangères, chacune avec son taux sans risque
    ///   - Actifs equity par devise (domestique + étrangers)
    ///   - Taux FX (domestic → foreign i) pour chaque devise étrangère
    ///   - Matrice de corrélation globale sur (S_equity, X_fx)
    ///
    /// Cohérence mesure risque-neutre :
    ///   Sous la mesure Q^d (domestique), la dynamique de chaque actif doit
    ///   être ajustée. Cet objet porte les données brutes ; le simulateur
    ///   appliquera les ajustements (Girsanov, quanto drift correction).
    /// </summary>
    public record MarketEnvironment
    {
        /// <summary>Devise de référence du desk / portefeuille.</summary>
        public CurrencyModel DomesticCurrency { get; init; }

        /// <summary>
        /// Devises étrangères supportées.
        /// Clé = enum Currency, valeur = modèle avec taux sans risque associé.
        /// </summary>
        public IReadOnlyDictionary<Currency, CurrencyModel> ForeignCurrencies { get; init; }

        /// <summary>
        /// Snapshots d'actifs equity regroupés par devise.
        /// Clé = Currency (devise de cotation naturelle de l'actif),
        /// valeur = liste de snapshots pour les actifs dans cette devise.
        ///
        /// Compatibilité : les MarketSnapshot existants sont réutilisés sans modification.
        /// La clé Currency est la NaturalCurrency de l'Underlying sous-jacent.
        /// </summary>
        public IReadOnlyDictionary<Currency, IReadOnlyList<MarketSnapshot>> EquitySnapshots { get; init; }

        /// <summary>
        /// Taux de change FX (domestic → foreign i).
        /// Clé = devise étrangère i, valeur = FxRate avec spot et volatilité.
        /// Doit contenir une entrée pour chaque devise dans ForeignCurrencies.
        /// </summary>
        public IReadOnlyDictionary<Currency, FxRate> FxRates { get; init; }

        /// <summary>
        /// Matrice de corrélation globale couvrant tous les facteurs :
        /// equity domestiques, equity étrangers (par devise), FX par devise.
        ///
        /// Ordre strict des facteurs (voir RiskFactorKey) :
        ///   1. Equity domestiques
        ///   2. Equity étrangers (par devise i puis par actif ℓ)
        ///   3. FX pour chaque devise étrangère i
        /// </summary>
        public GlobalCorrelationMatrix CorrelationMatrix { get; init; }

        /// <summary>Horodatage de référence de l'environnement de marché.</summary>
        public DateTimeOffset AsOf { get; init; }

        public MarketEnvironment(
            CurrencyModel domesticCurrency,
            IReadOnlyDictionary<Currency, CurrencyModel> foreignCurrencies,
            IReadOnlyDictionary<Currency, IReadOnlyList<MarketSnapshot>> equitySnapshots,
            IReadOnlyDictionary<Currency, FxRate> fxRates,
            GlobalCorrelationMatrix correlationMatrix,
            DateTimeOffset asOf)
        {
            DomesticCurrency = domesticCurrency;
            ForeignCurrencies = foreignCurrencies;
            EquitySnapshots = equitySnapshots;
            FxRates = fxRates;
            CorrelationMatrix = correlationMatrix;
            AsOf = asOf;
        }

        /// <summary>
        /// Récupère le snapshot d'un actif par son ticker dans toutes les devises.
        /// Retourne null si non trouvé.
        /// </summary>
        public MarketSnapshot? GetEquitySnapshot(string ticker)
        {
            foreach (var (_, snapshots) in EquitySnapshots)
            {
                var found = snapshots.FirstOrDefault(s => s.Ticker == ticker);
                if (found.Ticker is not null) return found;
            }
            return null;
        }

        /// <summary>
        /// Retourne le taux FX (domestic/foreign) pour une devise étrangère donnée.
        /// Lève ArgumentException si la devise étrangère n'est pas dans l'environnement.
        /// </summary>
        public FxRate GetFxRate(Currency foreignCurrency)
        {
            if (!FxRates.TryGetValue(foreignCurrency, out var rate))
                throw new ArgumentException(
                    $"FX rate for {foreignCurrency} not found in MarketEnvironment.");
            return rate;
        }

        /// <summary>
        /// Retourne le taux sans risque de la devise étrangère i.
        /// Utilisé pour les ajustements de dérive sous mesure risque-neutre.
        /// </summary>
        public double GetForeignRiskFreeRate(Currency foreignCurrency)
        {
            if (!ForeignCurrencies.TryGetValue(foreignCurrency, out var model))
                throw new ArgumentException(
                    $"Foreign currency {foreignCurrency} not found in MarketEnvironment.");
            return model.RiskFreeRate;
        }

        /// <summary>
        /// Construit un MarketEnvironment minimal pour le pricing mono-asset / mono-flux existant.
        /// Permet à Pricing et Risk de continuer à fonctionner sans modification
        /// en wrappant un simple MarketSnapshot dans l'environnement global.
        /// </summary>
        public static MarketEnvironment FromSingleAsset(
            MarketSnapshot snapshot,
            Currency domesticCurrency,
            double domesticRiskFreeRate)
        {
            var domestic = new CurrencyModel(domesticCurrency, domesticRiskFreeRate);
            var snapshotsByDomestic = new Dictionary<Currency, IReadOnlyList<MarketSnapshot>>
            {
                [domesticCurrency] = new List<MarketSnapshot> { snapshot }
            };
            var factorKey = new RiskFactorKey(
                Enums.AssetClass.Equity, domesticCurrency, snapshot.Ticker);
            var corrMatrix = GlobalCorrelationMatrix.Identity(
                new List<RiskFactorKey> { factorKey });

            return new MarketEnvironment(
                domestic,
                foreignCurrencies: new Dictionary<Currency, CurrencyModel>(),
                equitySnapshots: snapshotsByDomestic,
                fxRates: new Dictionary<Currency, FxRate>(),
                correlationMatrix: corrMatrix,
                asOf: snapshot.Timestamp);
        }
    }
}
