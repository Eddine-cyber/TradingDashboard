using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Reflection;

namespace TradingDashboard.Core.Enums
{
    public enum ProductType
    {
        [Description("European Call Option")]
        Call,
        [Description("European Put Option")]
        Put,
        [Description("futures contract")]
        Future,
        [Description("forward contract")]
        Forward
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
            return t == ProductType.Call || t == ProductType.Put;
        }
    }
    

}
