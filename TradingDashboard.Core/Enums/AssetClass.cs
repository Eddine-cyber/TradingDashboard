using System.ComponentModel;

namespace TradingDashboard.Core.Enums
{
    /// <summary>
    /// Classe d'actif dans le modèle multi-devises.
    /// Utilisé pour typer chaque facteur de risque dans GlobalCorrelationMatrix.
    /// </summary>
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
