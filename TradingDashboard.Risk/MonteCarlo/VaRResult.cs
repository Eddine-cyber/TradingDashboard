using System;
using System.Collections.Generic;
using System.Text;

namespace TradingDashboard.Risk.MonteCarlo
{
    internal record VaRResult(
        double Var,
        double ConfidenceLevel,
        int Horizon,
        int ScenariosUsed,
        DateTimeOffset ComputedAt
    );
}
