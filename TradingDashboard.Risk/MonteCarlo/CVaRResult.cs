using System;
using System.Collections.Generic;
using System.Text;

namespace TradingDashboard.Risk.MonteCarlo
{
    internal record CVaRResult(
        double Var,
        double ConfidenceLevel,
        int Horizon,
        int ScenariosUsed,
        DateTimeOffset ComputedAt,
        double CVar,
        double[] TailLosses
    );
}
