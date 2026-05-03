using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Entities;

namespace TradingDashboard.Risk.PnL
{
    internal record PortfolioPnL(
        double TotalDailyPnl,
        double TotalMTD,
        double TotalYTD,
        Greeks AggregatedGreeks,
        IReadOnlyList<PnLSummary> Positions,
        DateTimeOffset ComputedAt
        );
}
