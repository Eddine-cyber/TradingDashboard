using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TradingDashboard.Core.Entities
{
    public class Position
    {
        public Guid PositionId { get; init; }
        public Instrument Instrument { get; set; }
        public int NetQuantity { get; set; }
        public double AverageEntryPrice { get; set; }
        public double CurrentMarketValue { get; set; }
        public double UnrealizedPnL { get; set; }
        public double DailyPnL { get; set; }
        public Greeks LastGreeks { get; set; }
        public DateTimeOffset LastUpdatedAt { get; set; }


        public bool IsLong => this.NetQuantity > 0;
        public bool IsFlat => this.NetQuantity == 0;

        public void UpdateMarketValue(double newSpot)
        {
            CurrentMarketValue = NetQuantity * newSpot;
            UnrealizedPnL = CurrentMarketValue - NetQuantity*AverageEntryPrice;
        }
    }

}
