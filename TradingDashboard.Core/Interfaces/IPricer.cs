using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Entities;
using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Interfaces
{
    public interface IPricer<TResult>
    {
        Task<TResult> CalculatePrice(Instrument instrument, MarketSnapshot SnapShot);
        Task<Greeks> CalculateGreeks(Instrument instrument, MarketSnapshot SnapShot);
        ProductType SupportedProduct { get; }

    }
}
