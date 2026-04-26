using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Entities;

namespace TradingDashboard.Core.Interfaces
{
    interface IMarketDataFeed
    {
        Task<MarketSnapshot> GetSnapShot(string ticker);
        Task<IEnumerable<MarketSnapshot>> GetSnapshotsList(List<string> tickers);
        void StartRealTimeFlow();
        void CancelRealTimeFlow(CancellationToken token);
        event EventHandler<MarketSnapshot> OnPriceUpdated;
    }
}
