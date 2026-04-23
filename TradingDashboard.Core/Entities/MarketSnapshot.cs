using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace TradingDashboard.Core.Entities
{
    public record MarketSnapshot(string Ticker, decimal SpotPrice, double ImpliedVolatility, double RiskFreeRate, DateTimeOffset Timestamp, string Source);
}
