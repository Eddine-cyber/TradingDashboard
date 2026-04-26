using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Entities;
using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Interfaces
{
    interface IPricer<TResult>
    {
        Task<TResult> CalculatePrice(MarketSnapshot SnapShot);
        Task<Greeks> CalculateGreeks(MarketSnapshot SnapShot);
        ProductType SupportedProduct { get; }

    }
}
