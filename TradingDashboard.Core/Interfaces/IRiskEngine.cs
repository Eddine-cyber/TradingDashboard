using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Entities;

namespace TradingDashboard.Core.Interfaces
{
    interface IRiskEngine
    {
        Task<double> CalculatePnl(MarketSnapshot SnapShot);
        Task<Greeks> CalculateGreeks();
        Task<IEnumerable<Alert>> VerifyRiskLimits();
    }
}
