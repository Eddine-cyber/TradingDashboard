using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Entities.Market
{
    /// <summary>
    /// Taux de change spot entre deux devises, avec direction explicite.
    /// Convention : X_{i} = prix en devise domestique d'une unité de devise étrangère i.
    ///
    /// IMPORTANT FINANCIER :
    /// FX n'est pas un simple multiplicateur final.
    /// Il doit être simulé conjointement avec les sous-jacents equity
    /// pour capturer la corrélation FX/equity et permettre les quanto adjustments.
    ///
    /// Dynamique GBM historique :
    ///   dX_t = X_t (µ_X dt + σ_X dW_X_t)
    /// Sous mesure risque-neutre domestique :
    ///   dX_t = X_t ((r_d - r_f) dt + σ_X dW_X_t)
    /// </summary>
    /// <param name="DomesticCurrency">Devise de cotation (numéraire).</param>
    /// <param name="ForeignCurrency">Devise de base.</param>
    /// <param name="SpotRate">
    ///     Taux spot : nombre d'unités de devise domestique pour 1 unité étrangère.
    ///     Ex : si DomesticCurrency=EUR, ForeignCurrency=USD, SpotRate=0.92 (1 USD = 0.92 EUR).
    /// </param>
    /// <param name="Volatility">Volatilité annualisée du taux de change (σ_X).</param>
    /// <param name="Timestamp">Horodatage du snapshot FX.</param>
    public record FxRate(
        Currency DomesticCurrency,
        Currency ForeignCurrency,
        double SpotRate,
        double Volatility,
        DateTimeOffset Timestamp)
    {
        /// <summary>
        /// Identifiant canonique de la paire (ex : "EUR/USD").
        /// </summary>
        public string PairCode => $"{DomesticCurrency}/{ForeignCurrency}";

        /// <summary>
        /// Taux inverse : prix en devise étrangère d'une unité de devise domestique.
        /// </summary>
        public double InverseRate => SpotRate == 0 ? double.NaN : 1.0 / SpotRate;

        /// <summary>
        /// Validation minimale : le spot doit être strictement positif.
        /// </summary>
        public void Validate()
        {
            if (SpotRate <= 0)
                throw new ArgumentException($"FxRate SpotRate must be strictly positive. Got {SpotRate} for {PairCode}.");
            if (Volatility < 0)
                throw new ArgumentException($"FxRate Volatility cannot be negative. Got {Volatility} for {PairCode}.");
            if (DomesticCurrency == ForeignCurrency)
                throw new ArgumentException($"DomesticCurrency and ForeignCurrency must be different. Got {DomesticCurrency}.");
        }
    }
}
