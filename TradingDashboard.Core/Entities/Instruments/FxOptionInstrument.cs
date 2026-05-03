using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Entities.Instruments
{
    /// <summary>
    /// Option sur taux de change (FX option).
    /// Modèle de Garman-Kohlhagen — équivalent Black-Scholes avec deux taux sans risque.
    ///
    /// Exemples :
    ///   EUR call / USD put → droit d'acheter 1 EUR contre K USD à maturité.
    ///   Underlying = paire FX (ex : EURUSD).
    ///
    /// Dynamique sous mesure risque-neutre domestique (currency d) :
    ///   dX_t = X_t * (r_d - r_f) dt + σ_X dW_t
    ///
    /// Non-régression :
    ///   Hérite de Instrument. NaturalCurrency de Underlying = devise étrangère (base).
    ///   DomesticCurrency = devise de règlement (quote).
    /// </summary>
    public record FxOptionInstrument(
        Guid Id,
        string Ticker,
        Underlying Underlying,           // FX pair — NaturalCurrency = devise étrangère (base)
        DateOnly Maturity,
        Currency DomesticCurrency,       // devise de règlement (quote currency)
        double Strike,                   // K : nombre d'unités de devise domestique par unité étrangère
        ProductType ProductType,
        double ForeignRiskFreeRate,      // r_f : taux sans risque de la devise étrangère
        double FxVolatility)             // σ_X : volatilité du taux de change
        : Instrument(Id, Ticker, Underlying, Maturity, DomesticCurrency, ProductType)
    {
        /// <summary>Devise étrangère (base) = NaturalCurrency du Underlying FX.</summary>
        public Currency ForeignCurrency => Underlying.NaturalCurrency;

        public static FxOptionInstrument Create(
            string ticker,
            string fxPairName,
            Currency foreignCurrency,
            DateOnly maturity,
            Currency domesticCurrency,
            double strike,
            ProductType productType,
            double foreignRiskFreeRate,
            double fxVolatility,
            Guid id = default)
        {
            var underlying = new Underlying(fxPairName, foreignCurrency);
            var inst = new FxOptionInstrument(
                id == Guid.Empty ? Guid.NewGuid() : id,
                ticker, underlying, maturity, domesticCurrency,
                strike, productType, foreignRiskFreeRate, fxVolatility);
            inst.Validate();
            return inst;
        }

        public override void Validate()
        {
            if (ProductType != ProductType.FxCall && ProductType != ProductType.FxPut)
                throw new ArgumentException("ProductType must be FxCall or FxPut.");
            if (Strike <= 0)
                throw new ArgumentException("FX option Strike must be strictly positive.");
            if (FxVolatility < 0)
                throw new ArgumentException("FxVolatility cannot be negative.");
            if (ForeignCurrency == DomesticCurrency)
                throw new ArgumentException(
                    $"ForeignCurrency ({ForeignCurrency}) and DomesticCurrency ({DomesticCurrency}) must differ.");
        }

        public override string ToString()
            => $"{Ticker} | {ProductType} | Strike: {Strike} | " +
               $"FX: {ForeignCurrency}/{DomesticCurrency} | σ_X: {FxVolatility:P2} | " +
               $"r_f: {ForeignRiskFreeRate:P2} | Maturity: {Maturity:yyyy-MM-dd} | Id: {Id}";
    }
}
