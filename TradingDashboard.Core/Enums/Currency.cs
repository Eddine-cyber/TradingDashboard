using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace TradingDashboard.Core.Enums
{
    public enum Currency
    {
        [Description("Euro")]
        EUR,

        [Description("US Dollar")]
        USD,

        [Description("British Pound")]
        GBP,

        [Description("Japanese Yen")]
        JPY,

        [Description("Swiss Franc")]
        CHF
    }

}
