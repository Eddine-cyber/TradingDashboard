using System;
using System.ComponentModel;
using System.Reflection;

namespace TradingDashboard.Core.Enums
{
    public enum ProductType
    {
        // ── Single-Asset Vanilla Options ───────────────────────────────────
        [Description("European Call Option")]
        Call,
        [Description("European Put Option")]
        Put,
        [Description("Futures Contract")]
        Future,
        [Description("Forward Contract")]
        Forward,

        // ── Single-Asset Exotic Options ────────────────────────────────────
        [Description("Asian Arithmetic Average Call")]
        AsianCall,
        [Description("Asian Arithmetic Average Put")]
        AsianPut,
        [Description("Lookback Fixed Strike Call")]
        LookbackCall,
        [Description("Lookback Fixed Strike Put")]
        LookbackPut,
        [Description("Lookback Floating Strike Call")]
        FloatingLookbackCall,
        [Description("Lookback Floating Strike Put")]
        FloatingLookbackPut,

        // ── Barrier Options (Knock-Out / Knock-In) ─────────────────────────
        [Description("Up-and-Out Call")]
        UpAndOutCall,
        [Description("Up-and-Out Put")]
        UpAndOutPut,
        [Description("Down-and-Out Call")]
        DownAndOutCall,
        [Description("Down-and-Out Put")]
        DownAndOutPut,
        [Description("Up-and-In Call")]
        UpAndInCall,
        [Description("Up-and-In Put")]
        UpAndInPut,
        [Description("Down-and-In Call")]
        DownAndInCall,
        [Description("Down-and-In Put")]
        DownAndInPut,
        [Description("Double Barrier Call")]
        DoubleBarrierCall,
        [Description("Double Barrier Put")]
        DoubleBarrierPut,

        // ── Digital Options ────────────────────────────────────────────────
        [Description("Cash-or-Nothing Call")]
        CashOrNothingCall,
        [Description("Cash-or-Nothing Put")]
        CashOrNothingPut,
        [Description("Asset-or-Nothing Call")]
        AssetOrNothingCall,
        [Description("Asset-or-Nothing Put")]
        AssetOrNothingPut,

        // ── FX Options ─────────────────────────────────────────────────────
        [Description("FX European Call (Garman-Kohlhagen)")]
        FxCall,
        [Description("FX European Put (Garman-Kohlhagen)")]
        FxPut,
        [Description("FX Asian Arithmetic Average Call")]
        FxAsianCall,
        [Description("FX Asian Arithmetic Average Put")]
        FxAsianPut,
        [Description("FX Knock-Out Call")]
        FxBarrierCall,
        [Description("FX Knock-Out Put")]
        FxBarrierPut,

        // ── Multi-Asset Options ────────────────────────────────────────────
        [Description("Basket Call (Weighted Average)")]
        BasketCall,
        [Description("Basket Put (Weighted Average)")]
        BasketPut,
        [Description("Worst-of Call (Minimum Return)")]
        WorstOfCall,
        [Description("Worst-of Put (Minimum Return)")]
        WorstOfPut,
        [Description("Best-of Call (Maximum Return)")]
        BestOfCall,
        [Description("Best-of Put (Maximum Return)")]
        BestOfPut,
        [Description("Spread Option (S1 - S2 - K)")]
        SpreadOption,
        [Description("Outperformance (S1 - S2, K=0)")]
        Outperformance,

        // ── Multi-Currency Options ─────────────────────────────────────────
        [Description("Quanto (Foreign Asset, Domestic Payment)")]
        Quanto,

        // ── Variance / Volatility ──────────────────────────────────────────
        [Description("Variance Swap")]
        VarianceSwap,
        [Description("Volatility Swap")]
        VolatilitySwap,

        // ── Cliquets ───────────────────────────────────────────────
        [Description("Cliquet (resetted-strike sum of returns)")]
        Cliquet,
        [Description("Ratchet (accumulated capped returns)")]
        Ratchet,

        // ── Structured Products ────────────────────────────────────────────
        [Description("Autocall Athena")]
        AutocallAthena,
        [Description("Autocall Phoenix")]
        AutocallPhoenix,
        [Description("Reverse Convertible")]
        ReverseConvertible
    }

    public static class ProductTypeExtensions
    {
        public static string ProductTypeDescription(this ProductType t)
        {
            var field = t.GetType().GetField(t.ToString());
            var attr = field?.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? t.ToString();
        }

        public static bool IsOption(this ProductType t)
        {
            return t != ProductType.Forward && t != ProductType.Future;
        }

        public static bool IsVanilla(this ProductType t)
        {
            return t == ProductType.Call || t == ProductType.Put;
        }

        public static bool IsExotic(this ProductType t)
        {
            return t == ProductType.AsianCall || t == ProductType.AsianPut
                || t == ProductType.LookbackCall || t == ProductType.LookbackPut
                || t == ProductType.FloatingLookbackCall || t == ProductType.FloatingLookbackPut
                || t == ProductType.UpAndOutCall || t == ProductType.UpAndOutPut
                || t == ProductType.DownAndOutCall || t == ProductType.DownAndOutPut
                || t == ProductType.UpAndInCall || t == ProductType.UpAndInPut
                || t == ProductType.DownAndInCall || t == ProductType.DownAndInPut
                || t == ProductType.DoubleBarrierCall || t == ProductType.DoubleBarrierPut
                || t == ProductType.CashOrNothingCall || t == ProductType.CashOrNothingPut
                || t == ProductType.AssetOrNothingCall || t == ProductType.AssetOrNothingPut
                || t == ProductType.VarianceSwap || t == ProductType.VolatilitySwap
                || t == ProductType.Cliquet || t == ProductType.Ratchet;
        }

        public static bool IsFx(this ProductType t)
        {
            return t == ProductType.FxCall || t == ProductType.FxPut
                || t == ProductType.FxAsianCall || t == ProductType.FxAsianPut
                || t == ProductType.FxBarrierCall || t == ProductType.FxBarrierPut
                || t == ProductType.Quanto;
        }

        public static bool IsMultiAsset(this ProductType t)
        {
            return t == ProductType.BasketCall || t == ProductType.BasketPut
                || t == ProductType.WorstOfCall || t == ProductType.WorstOfPut
                || t == ProductType.BestOfCall || t == ProductType.BestOfPut
                || t == ProductType.SpreadOption || t == ProductType.Outperformance;
        }

        public static bool IsBarrier(this ProductType t)
        {
            return t == ProductType.UpAndOutCall || t == ProductType.UpAndOutPut
                || t == ProductType.DownAndOutCall || t == ProductType.DownAndOutPut
                || t == ProductType.UpAndInCall || t == ProductType.UpAndInPut
                || t == ProductType.DownAndInCall || t == ProductType.DownAndInPut
                || t == ProductType.DoubleBarrierCall || t == ProductType.DoubleBarrierPut;
        }

        public static bool IsDigital(this ProductType t)
        {
            return t == ProductType.CashOrNothingCall || t == ProductType.CashOrNothingPut
                || t == ProductType.AssetOrNothingCall || t == ProductType.AssetOrNothingPut;
        }

        public static bool IsStructured(this ProductType t)
        {
            return t == ProductType.AutocallAthena || t == ProductType.AutocallPhoenix
                || t == ProductType.ReverseConvertible;
        }

        public static bool RequiresMultipleUnderlyings(this ProductType t)
        {
            return t.IsMultiAsset() || t.IsStructured();
        }
    }
}
