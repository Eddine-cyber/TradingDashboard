using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TradingDashboard.Risk.PnL
{
    internal class PnLEngine
    {
        IDisposable Subscription { get; init; }

        void OnNext(MarketSnapshot snapshot)
        {

        }
        void OnError(Exception error)
        {

        }
        void OnCompleted()
        {

        }
        Task<PnLSummary> GetCurrentPnLAsync(Guid positionId, CancellationToken ct)
        {

        }
        Task<PortfolioPnL> GetPortfolioPnLAsync(CancellationToken ct)
        {

        }

    }
}
