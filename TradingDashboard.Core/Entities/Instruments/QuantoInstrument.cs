using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Entities.Instruments
{
    /// <summary>
    /// Quanto instrument : option sur actif étranger, payée en devise domestique
    /// à un taux FX fixé (généralement 1.0 — le "quanto" élimine le risque FX
    /// mais conserve l'exposition à l'actif étranger).
    ///
    /// Exemples typiques :
    ///   - Call sur Nikkei 225, payé en EUR (pas en JPY).
    ///   - Option sur S&amp;P 500 payée en EUR.
    ///
    /// Mathématique sous mesure risque-neutre domestique (d) :
    ///   La dynamique de l'actif étranger S_f est ajustée par un quanto drift :
    ///   dS_f = S_f * (r_d - ρ_{S,X} * σ_S * σ_X) dt + σ_S * dW_S
    ///
    ///   Le terme (-ρ_{S,X} * σ_S * σ_X) est le "quanto adjustment".
    ///   Il dépend de la corrélation equity/FX et des deux volatilités.
    ///
    /// Design :
    ///   QuantoInstrument est un wrapper autour d'un Instrument de base (l'actif étranger).
    ///   Le FxQuantoRate est le taux auquel le payoff est converti (fixé à la trade date,
    ///   typiquement 1.0 ou le spot FX initial).
    /// </summary>
    public record QuantoInstrument(
        Guid Id,
        string Ticker,
        Underlying Underlying,          // actif étranger sous-jacent
        DateOnly Maturity,
        Currency DomesticCurrency,      // devise de paiement (domestic)
        double Strike,
        ProductType ProductType,
        double FxQuantoRate,            // taux FX fixé pour la conversion (typiquement spot initial ou 1.0)
        double EquityFxCorrelation,     // ρ_{S,X} : corrélation equity/FX pour le quanto adjustment
        double ForeignEquityVolatility, // σ_S : vol de l'actif étranger
        double FxVolatility)            // σ_X : vol du taux de change (nécessaire pour le drift)
        : Instrument(Id, Ticker, Underlying, Maturity, DomesticCurrency, ProductType)
    {
        /// <summary>
        /// Quanto drift adjustment : -ρ_{S,X} * σ_S * σ_X.
        /// À ajouter au drift de l'actif étranger sous la mesure domestique Q^d.
        /// </summary>
        public double QuantoDriftAdjustment
            => -EquityFxCorrelation * ForeignEquityVolatility * FxVolatility;

        public static QuantoInstrument Create(
            string ticker,
            Underlying foreignUnderlying,
            DateOnly maturity,
            Currency domesticCurrency,
            double strike,
            ProductType productType,
            double fxQuantoRate,
            double equityFxCorrelation,
            double foreignEquityVolatility,
            double fxVolatility,
            Guid id = default)
        {
            var inst = new QuantoInstrument(
                id == Guid.Empty ? Guid.NewGuid() : id,
                ticker, foreignUnderlying, maturity, domesticCurrency,
                strike, productType, fxQuantoRate, equityFxCorrelation,
                foreignEquityVolatility, fxVolatility);
            inst.Validate();
            return inst;
        }

        public override void Validate()
        {
            if (!ProductType.IsOption())
                throw new ArgumentException("QuantoInstrument ProductType must be an option.");
            if (Strike < 0)
                throw new ArgumentException("Strike must be non-negative.");
            if (FxQuantoRate <= 0)
                throw new ArgumentException("FxQuantoRate must be strictly positive.");
            if (EquityFxCorrelation < -1.0 || EquityFxCorrelation > 1.0)
                throw new ArgumentException(
                    $"EquityFxCorrelation must be in [-1, 1]. Got {EquityFxCorrelation}.");
            if (ForeignEquityVolatility < 0)
                throw new ArgumentException("ForeignEquityVolatility cannot be negative.");
            if (FxVolatility < 0)
                throw new ArgumentException("FxVolatility cannot be negative.");
        }

        public override string ToString()
            => $"{Ticker} | QUANTO | {ProductType} | Strike: {Strike} | " +
               $"Underlying: {Underlying.Name} ({Underlying.NaturalCurrency}) | " +
               $"Paid in: {DomesticCurrency} | ρ(S,X): {EquityFxCorrelation:F2} | " +
               $"QuantoDrift: {QuantoDriftAdjustment:F4} | Maturity: {Maturity:yyyy-MM-dd} | Id: {Id}";
    }
}
