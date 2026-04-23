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
        public decimal AverageEntryPrice { get; set; }
        public decimal CurrentMarketValue { get; set; }
        public decimal UnrealizedPnL { get; set; }
        public decimal DailyPnL { get; set; }
        public Greeks LastGreeks { get; set; }
        public DateTimeOffset LastUpdatedAt { get; set; }


        public bool IsLong() => this.NetQuantity > 0;
        public bool IsFlat() => this.NetQuantity == 0;

        public void UpdateMarketValue(decimal newSpot)
        {
            CurrentMarketValue = NetQuantity * newSpot;
            UnrealizedPnL = CurrentMarketValue - NetQuantity*AverageEntryPrice;
        }
    }

}
