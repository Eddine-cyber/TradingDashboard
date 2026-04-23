using System;
using System.Collections.Generic;
using System.Text;

namespace TradingDashboard.Core.Enums
{
    public enum AlertLevels
    {
        Info = 0,
        Warning = 1,
        Critical = 2,
        Fatal = 3
    }

    public static class AlertLevelsExtensions
    {
        public static bool RequiresImmediateAction(this AlertLevels alert)
        {
            return (int)alert > 1;
        }
    }
}
