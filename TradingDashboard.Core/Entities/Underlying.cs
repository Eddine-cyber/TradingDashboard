using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Entities
{
    /// <summary>
    /// Risk factor identity, representing an Equity, FX rate, or Interest rate.
    ///
    /// FX Convention:
    ///   Underlying("USD", Currency.EUR, AssetClass.FX) represents "the price of 1 USD expressed in EUR".
    ///   The name (ticker) is the foreign currency, while the natural currency is the domestic numeraire (EUR).
    ///
    /// Equity Convention:
    ///   Underlying("AAPL", Currency.USD, AssetClass.Equity) represents "AAPL stock quoted in USD".
    ///
    /// Uniqueness: The Name property serves as the unique identifier across all dictionaries.
    /// </summary>
    public record Underlying(
        string Name, // This name is unique and used as a key in dictionaries
        Currency NaturalCurrency,
        AssetClass AssetClass = AssetClass.Equity);
}
