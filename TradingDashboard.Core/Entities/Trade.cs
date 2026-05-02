using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace TradingDashboard.Core.Entities
{
    public record Trade(Guid TradeId, Guid InstrumentId, double Notional, int Quantity, double TradePrice, DateTimeOffset TradeDate, string Desk, string Trader);
}
