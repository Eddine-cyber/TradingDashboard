using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Enums;
using TradingDashboard.Core.Entities;
using TradingDashbord.Pricing.MonteCarlo.Payoffs;

namespace TradingDashbord.Pricing.MonteCarlo
{
    public static class PayoffFactory
    {
        public static IPayoff Creat(Instrument instrument)
        {
            return instrument.ProductType switch
            {
                ProductType.AsianCall    => new ArithmeticAsianCallPayoff(instrument.StrikeOrNull!.Value, instrument.ObservationCountOrNull!.Value),
                ProductType.AsianPut     => new ArithmeticAsianPutPayoff(instrument.StrikeOrNull!.Value, instrument.ObservationCountOrNull!.Value),
                ProductType.LookbackCall => new LookbackFixedStrikeCallPayoff(instrument.StrikeOrNull!.Value),
                ProductType.LookbackPut  => new LookbackFixedStrikePutPayoff(instrument.StrikeOrNull!.Value),
                _ => throw new ArgumentException($"PayoffFactory cannot be instancieted by {instrument.ProductType}")
            };
        }
    }
}
