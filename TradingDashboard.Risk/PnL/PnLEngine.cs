using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Entities;
using TradingDashbord.Pricing;
using TradingDashbord.Pricing.MonteCarlo;
using TradingDashbord.Pricing.Strategy;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TradingDashboard.Risk.PnL
{
    internal class PnLEngine
    {
        IDisposable Subscription { get; init; }
        public Dictionary<Guid,Position> positions { get; set; }
        public void UpdateConfig(MonteCarloConfig config)
        {
            PricingEngine.Config = config;
        }
        public PricingEngine PricingEngine { get; set; }
        Dictionary<string, MarketSnapshot> SnapshotsByUnderlying { get; set; }
        public PnLEngine(IDisposable subscription, MonteCarloConfig config=null)
        {
            Subscription = subscription;
            PricingEngine = new PricingEngine(config: config);
        }

        public async Task OnNext(MarketSnapshot snapshot)
        {
            // For consistent calculations in this method, we capture an immutable local copy for PricingEngine.
            // External modifications to PricingEngine will not affect this computation.
            PricingEngine pricingEngineCopy = PricingEngine;

            // update snapshot dictionary
            SnapshotsByUnderlying[snapshot.Underlying] = snapshot;

            // Update position theorical prices to match the current snapshot (Mark-to-model approach due to the absence of observable market prices for exotic options)
            foreach(Guid positionId in positions.Keys)
            {
                if(positions[positionId].Instrument.Underlying.Name == snapshot.Underlying)
                {
                    Position position = positions[positionId];
                    PricingResult instrumentPrice = await pricingEngineCopy.PriceAsync(position.Instrument, snapshot);
                    position.UpdateMarketValue(instrumentPrice.TheoreticalPrice); // this will update also the current position in the array positions -reference object-
                    // update position Greeks
                    position.UpdateGreeks(instrumentPrice.Greeks);
                }
            }
        }

        void OnError(Exception error)
        {

        }
        void OnCompleted()
        {

        }
        public Task<PnLSummary> GetCurrentPnLAsync(Guid positionId, CancellationToken ct)
        {
            if (!positions.ContainsKey(positionId)) throw new ArgumentException($"{positionId} doesn't existe in the portfolio");
            else
            {
                Position position = positions[positionId];
                return Task.FromResult(new PnLSummary(position.PositionId, "Ticker : not yet implemented", position.DailyPnL, 0.0, 0.0, position.LastGreeks, DateTimeOffset.UtcNow));
            }
        }
        Task<PortfolioPnL> GetPortfolioPnLAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

    }
}
