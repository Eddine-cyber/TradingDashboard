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
        public void UpdateConfig(MonteCarloConfig config)
        {
            PricingEngine.Config = config;
        }
        public int NumberOfScenarios { get; set; }
        public PricingEngine PricingEngine { get; set; }

        public PortfolioSimulator(int numberOfScenarios, MonteCarloConfig config=null)
        {
            NumberOfScenarios = numberOfScenarios;
            PricingEngine = new PricingEngine(config: config);
        }

        // The argument Horizon used in the following methods is expressed in trading days, so we divide it by 252.0.
        // This follows the same convention as Instrument.YearsToMaturity.
        // If we switch the convention to use calendar days, we should divide by 365.25 instead.
        async Task<VaRResult> ComputeVarAsync(Position[] positions, IReadOnlyDictionary<string, MarketSnapshot> snapshotsByUnderlying, int horizon=1, double confidencelevel=0.99, CancellationToken ct = default)
        {
            double[] portfolioreturns = await SimulatePortfolioReturns(positions, snapshotsByUnderlying, horizon, ct);
            int tailIndex = (int)((1 - confidencelevel) * portfolioreturns.Length);
            double Var = -portfolioreturns[tailIndex];
            return new VaRResult(Var, confidencelevel, horizon, NumberOfScenarios, DateTimeOffset.UtcNow);
        }
        async Task<CVaRResult> ComputeCVarAsync(Position[] positions, IReadOnlyDictionary<string, MarketSnapshot> snapshotsByUnderlying, int horizon = 1, double confidencelevel = 0.99, CancellationToken ct = default)
        {
            double[] portfolioreturns = await SimulatePortfolioReturns(positions, snapshotsByUnderlying, horizon, ct);
            int tailIndex = (int)((1 - confidencelevel) * portfolioreturns.Length);
            double Var = -portfolioreturns[tailIndex];
            double[] tailLosses = portfolioreturns[..tailIndex].Select(loss => -loss).ToArray(); // tailLosses should not include the index tailIndex (the Var point) : [0 ; tailIndex [
            double Cvar = tailLosses.Average();
            return new CVaRResult(Var, confidencelevel, horizon, NumberOfScenarios, DateTimeOffset.UtcNow, Cvar, tailLosses);
        }
        async Task<double[]> SimulatePortfolioReturns(Position[] positions, IReadOnlyDictionary<string, MarketSnapshot> snapshotsByUnderlying, int horizon = 1, CancellationToken ct = default)
        {
            // For consistent calculations in this method, we capture an immutable local copy for PricingEngine.
            // External modifications to PricingEngine will not affect this computation.
            PricingEngine pricingEngineCopy = PricingEngine;

            double[] PnlsPerScenario = new double[NumberOfScenarios];
            int numberOfPositions = positions.Length;

            Dictionary<string, GBMPathSimulator> SimulatorByUnderlying = new();
            foreach (string underlying in snapshotsByUnderlying.Keys)
            {
                MarketSnapshot snapshot = snapshotsByUnderlying[underlying];
                SimulatorByUnderlying[underlying] = new GBMPathSimulator(snapshot.SpotPrice, snapshot.RiskFreeRate, snapshot.ImpliedVolatility, horizon / 252.0, pricingEngineCopy.Config.Steps);
            }

            // Update position theorical prices to match the current snapshot (Mark-to-model approach due to the absence of observable market prices for exotic options)
            // We should create a new array to store the "position.CurrentMarketValue" because position is a reference and it attributes can be changed before the next ParallelFor which is not consistant
            double[] positionsCurrentMarketValue = new double[numberOfPositions];
            for (int k = 0; k < numberOfPositions; k++)
            {
                Position position = positions[k];
                double instrumentPrice = await pricingEngineCopy.PriceOnlyAsync(position.Instrument, snapshotsByUnderlying[position.Instrument.Underlying.Name]);
                position.UpdateMarketValue(instrumentPrice);
                positionsCurrentMarketValue[k] = position.CurrentMarketValue;
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

                    double[] UnderlyingPath = SimulatorByUnderlying[position.Instrument.Underlying.Name].SimulatePath(rng.Value);
                    double actualisationRate = Math.Exp(-position.Instrument.YearsToMaturity * SimulatorByUnderlying[position.Instrument.Underlying.Name].Rate);
                    double instrumentPrice = payoff.Compute(UnderlyingPath) * actualisationRate;
                    double positionPNL = instrumentPrice * position.NetQuantity - positionsCurrentMarketValue[j] ;
                    PorfolioPNLForScenario += positionPNL;
                }
                PnlsPerScenario[i] = PorfolioPNLForScenario;
            });

            Array.Sort(PnlsPerScenario);
            return PnlsPerScenario;
        }

    }
}
