using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Entities;

namespace TradingDashboard.Risk.PnL
{
    internal record PnLSummary(
        Guid PositionId,
        string Ticker,
        double DailyPnl,
        double MTD,
        double YTD,
        Greeks CurrentGreeks,
        DateTimeOffset ComputedAt
    );
}
