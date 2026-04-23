using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Enums;

namespace TradingDashboard.Core.Entities
{
    public class Alert
    {
        public Guid AlertId { get; init; }
        public AlertLevel Level { get; set; }
        public string Message { get; set; }
        public Guid? PositionId { get; init; }
        public DateTimeOffset TriggeredAt { get; init; }
        public bool IsAcknowledged { get; set; } = false;
        public DateTimeOffset? AcknowledgedAt { get; set; }

        public void Acknowledge()
        {
            IsAcknowledged = true;
            AcknowledgedAt = DateTimeOffset.Now;
        }

        //Factory Methode
        public static Alert Create(AlertLevel level, string message , Guid positionId) => new Alert{Level = level, Message = message, PositionId = positionId};

    }
}
