using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Reflection;

namespace TradingDashboard.Core.Enums
{
    public enum ProductType
    {
        // ── Vanilles mono-asset -───────────────────────────────────
        [Description("European Call Option")]
        Call,
        [Description("European Put Option")]
        Put,
        [Description("futures contract")]
        Future,
        [Description("forward contract")]
        Forward,

        // ── Exotiques mono-asset ───────────────────────────────────
        [Description("Asian Arithmetic Average Call Option")]
        AsianCall,
        [Description("Asian Arithmetic Average Put Option")]
        AsianPut,
        [Description("Lookback Fixed Strike Call Option")]
        LookbackCall,
        [Description("Lookback Fixed Strike Put Option")]
        LookbackPut,

        // ── FX Options ───────────────────────────────────────
        [Description("FX European Call Option ")]
        FxCall,
        [Description("FX European Put Option ")]
        FxPut,

        // ── Basket Options (multi-assets) ──────────────────────────────────────
        [Description("Basket Call Option")]
        BasketCall,
        [Description("Basket Put Option")]
        BasketPut,

        // ── Quanto (foreign asset payed in domestic currency) ──────────────────
        [Description("Quanto Option (foreign asset, domestic payment)")]
        Quanto
    }

    public static class ProductTypeExtensions
    {
        public static String ProductTypeDescription<T>(this T t) where T : Enum //works for All Enums -ProductType, Currency...- : generic
        {
            var field = t.GetType().GetField(t.ToString()); // GetType is executed in runtime, so it returns the real type (Currency, ProductType) not the generic type Enum
            var attr = field.GetCustomAttribute<DescriptionAttribute>();
            return attr == null ? t.ToString() : attr.Description;
        }

        public static bool IsOption(this ProductType t)
        {
            return t != ProductType.Forward && t != ProductType.Future;
        }

        /// <summary>
        /// Options exotiques mono-asset path-dependent (Asian, Lookback).
        /// Non-régression garantie : comportement identique à avant.
        /// </summary>
        public static bool IsExotic(this ProductType t)
        {
            return t == ProductType.AsianCall
                || t == ProductType.AsianPut
                || t == ProductType.LookbackCall
                || t == ProductType.LookbackPut;
        }

        /// <summary>
        /// Options vanilles européennes (Call/Put uniquement).
        /// Non-régression garantie : comportement identique à avant.
        /// </summary>
        public static bool IsVanilla(this ProductType t)
        {
            return !t.IsExotic() && t.IsOption()
                && !t.IsFx() && !t.IsBasket() && !t.IsQuanto();
        }

        /// <summary>Options sur taux de change (Garman-Kohlhagen).</summary>
        public static bool IsFx(this ProductType t)
            => t == ProductType.FxCall || t == ProductType.FxPut;

        /// <summary>Options sur panier d'actifs multi-devises.</summary>
        public static bool IsBasket(this ProductType t)
            => t == ProductType.BasketCall || t == ProductType.BasketPut;

        /// <summary>Options quanto (actif étranger payé en devise domestique).</summary>
        public static bool IsQuanto(this ProductType t)
            => t == ProductType.Quanto;

        /// <summary>
        /// Retourne true pour tout produit nécessitant une simulation multi-assets
        /// (corrélation FX/equity, Cholesky). Utilisé par les factories de Pricing.
        /// </summary>
        public static bool IsMultiAsset(this ProductType t)
            => t.IsFx() || t.IsBasket() || t.IsQuanto();
    }
    

}
