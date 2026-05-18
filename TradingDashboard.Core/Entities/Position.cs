using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TradingDashboard.Core.Entities
{
    /// <summary>
    /// Represents a trading position for a specific financial instrument.
    /// Tracks the net quantity, market value, unrealized PnL, and aggregated sensitivities.
    /// </summary>
    public class Position
    {
        public Guid PositionId { get; init; }
        
        public Instrument Instrument { get; init; }
        
        public int NetQuantity { get; init; }
        
        public double AverageEntryPrice { get; init; }
        
        public double CurrentMarketValue { get; set; }
        
        public double UnrealizedPnL { get; set; }
        
        public double DailyPnL { get; set; }
        
        public Greeks LastGreeks { get; set; }
        
        public DateTimeOffset LastUpdatedAt { get; set; }

        public bool IsLong => this.NetQuantity > 0;
        
        public bool IsFlat => this.NetQuantity == 0;

        /// <summary>
        /// Updates the current market value and unrealized PnL based on the latest instrument price.
        /// </summary>
        /// <param name="newSpot">The current market price of the instrument.</param>
        public void UpdateMarketValue(double newSpot)
        {
            CurrentMarketValue = NetQuantity * newSpot;
            UnrealizedPnL = CurrentMarketValue - NetQuantity * AverageEntryPrice;
            LastUpdatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Updates the aggregated Greeks for the position based on the per-unit Greeks.
        /// </summary>
        /// <param name="newGreeks">The newly calculated per-unit Greeks.</param>
        public void UpdateGreeks(Greeks newGreeks)
        {
            LastGreeks = NetQuantity * newGreeks;
        }
    }
}
