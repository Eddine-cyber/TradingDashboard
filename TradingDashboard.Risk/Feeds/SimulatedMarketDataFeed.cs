using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Entities;

namespace TradingDashboard.Risk.Feeds
{
    internal class SimulatedMarketDataFeed
    {
        List<IObserver<MarketSnapshot>> Observer { get; init; }
        IDisposable Subscribe(List<IObserver<MarketSnapshot>> observer)
        {
            throw new NotImplementedException();
        }
        Task StartAsync(CancellationToken c)
        {
            throw new NotImplementedException();
        }

        Task<MarketSnapshot> GetSnapshotAsync(string Tiker)
        {
            throw new NotImplementedException();
        }
    }
}
