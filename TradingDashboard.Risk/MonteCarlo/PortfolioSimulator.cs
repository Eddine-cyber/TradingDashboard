using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using TradingDashboard.Core.Entities;
using TradingDashboard.Core.Interfaces;
using TradingDashbord.Pricing;
using TradingDashbord.Pricing.MonteCarlo;
using TradingDashbord.Pricing.Strategy;

namespace TradingDashboard.Risk.MonteCarlo
{
    internal class PortfolioSimulator
    {
        IPricer<PricingResult> Pricer { get; init; }
        private MonteCarloConfig Config { get; init; }
        int NumberOfScenarios { get; init; }

        public PortfolioSimulator(IPricer<PricingResult> pricer, int numberOfScenarios)
                => (Pricer, NumberOfScenarios) = (pricer, numberOfScenarios);

        async Task<VaRResult> ComputeVarAsync(Position[] positions, IReadOnlyDictionary<string, MarketSnapshot> snapshotsByUnderlying, int horizon=1, double confidencelevel=0.99, CancellationToken ct = default)
        {
            double[] portfolioreturns = await SimulatePortfolioReturns(positions, snapshotsByUnderlying, horizon, ct);
            double Var = -portfolioreturns[(int)((1- confidencelevel) * portfolioreturns.Length)];
            return new VaRResult(Var, confidencelevel, horizon, NumberOfScenarios, DateTimeOffset.UtcNow);
        }
        async Task<CVaRResult> ComputeCVarAsync(Position[] positions, IReadOnlyDictionary<string, MarketSnapshot> snapshotsByUnderlying, int horizon = 1, double confidencelevel = 0.99, CancellationToken ct = default)
        {
            double[] portfolioreturns = await SimulatePortfolioReturns(positions, snapshotsByUnderlying, horizon, ct);
            int tailIndex = (int)((1 - confidencelevel) * portfolioreturns.Length);
            double Var = -portfolioreturns[tailIndex];
            double[] tailLosses = portfolioreturns[..tailIndex].Select(loss => -loss).ToArray() ;
            double Cvar = tailLosses.Average();
            return new CVaRResult(Var, confidencelevel, horizon, NumberOfScenarios, DateTimeOffset.UtcNow, Cvar, tailLosses);
        }
        async Task<double[]> SimulatePortfolioReturns(Position[] positions, IReadOnlyDictionary<string, MarketSnapshot> snapshotsByUnderlying, int horizon = 1, CancellationToken ct = default)
        {
            double[] PnlsPerScenario = new double[NumberOfScenarios];
            int numberOfPositions = positions.Length;

            Dictionary<string, GBMPathSimulator> SimulatorByUnderlying = new();
            foreach (string underlying in snapshotsByUnderlying.Keys)
            {
                MarketSnapshot snapshot = snapshotsByUnderlying[underlying];
                SimulatorByUnderlying[underlying] = new GBMPathSimulator(snapshot.SpotPrice, snapshot.RiskFreeRate, snapshot.ImpliedVolatility, horizon / 252, Config.Steps);
            }

            // Update position theorical prices to match the current snapshot (mark-to-model)
            PricingEngine pricingEngine = new PricingEngine(config: Config);
            for (int k = 0; k < numberOfPositions; k++)
            {
                Position position = positions[k];
                double instrumentPrice = await pricingEngine.PriceOnlyAsync(position.Instrument, snapshotsByUnderlying[position.Instrument.Underlying]);
                position.UpdateMarketValue(instrumentPrice);
            }

            // Calculate porfolio PNL for each Scenario
            IPayoff[] payoffByPosition = positions.Select(p => PayoffFactory.Creat(p.Instrument)).ToArray();
            ThreadLocal<Random> rng = new(() => new Random(Random.Shared.Next()));
            Parallel.For(0, NumberOfScenarios, i =>
            {
                double PorfolioPNLForScenario = 0.0;
                for (int j = 0; j < numberOfPositions; j++)
                {
                    Position position = positions[j];
                    IPayoff payoff = payoffByPosition[j];

                    double[] UnderlyingPath = SimulatorByUnderlying[position.Instrument.Underlying].SimulatePath(rng.Value);
                    double actualisationRate = Math.Exp(-position.Instrument.YearsToMaturity * SimulatorByUnderlying[position.Instrument.Underlying].Rate);
                    double instrumentPrice = payoff.Compute(UnderlyingPath) * actualisationRate;
                    double psitionPNL = instrumentPrice * position.NetQuantity - position.CurrentMarketValue;
                    PorfolioPNLForScenario += psitionPNL;
                }
                PnlsPerScenario[i] = PorfolioPNLForScenario;
            });

            Array.Sort(PnlsPerScenario);
            return PnlsPerScenario;
        }

    }
}
