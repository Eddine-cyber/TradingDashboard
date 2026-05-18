using TradingDashboard.Core.Entities;

namespace TradingDashboard.Core.Entities.Market
{
    /// <summary>
    /// Instantaneous market data for a specific risk factor (equity or FX).
    /// Identification key: Asset.Name (unique ticker).
    ///
    /// FX Assets:
    ///   Asset = Underlying("USD", Currency.EUR, AssetClass.FX)
    ///   SpotPrice = 0.92 (price of 1 USD expressed in EUR)
    ///   ImpliedVolatility = annualized FX exchange rate volatility
    ///
    /// Equity Assets:
    ///   Asset = Underlying("AAPL", Currency.USD, AssetClass.Equity)
    ///   SpotPrice = 185.50
    ///   ImpliedVolatility = ATM implied volatility (sufficient for BS-GBM models)
    ///
    /// Future Extensions (Heston/Local Volatility):
    ///   Calibrated model parameters will be stored in the database and injected directly 
    ///   into the PathGenerator, keeping this snapshot lightweight and focused on real-time data.
    /// </summary>
    public record struct MarketSnapshot(
        Underlying Asset,
        double SpotPrice,
        double ImpliedVolatility,
        DateTimeOffset Timestamp,
        string Source)
    {
        /// <summary>Shortcut to retrieve the asset's ticker, serving as the dictionary key.</summary>
        public string Ticker => Asset.Name;
    }
}
