using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Entities;
using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Interfaces
{
    public interface IPricer<TResult>
    {
        public Task<TResult> CalculatePrice(Instrument instrument, MarketSnapshot SnapShot);
        public Task<Greeks> CalculateGreeks(Instrument instrument, MarketSnapshot SnapShot);
        public ProductType SupportedProduct { get; }

    }
}
