using System.ComponentModel;

namespace TradingDashboard.Core.Enums
{
    public enum AssetClass
    {
        [Description("Equity / Action")]
        Equity,

        [Description("Foreign Exchange")]
        FX,

        [Description("Interest Rate")]
        Rate
    }
}
