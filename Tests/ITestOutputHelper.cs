using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Enums;
using TradingDashbord.Pricing.BlackScholes;
using TradingDashbord.Pricing.MonteCarlo;
using TradingDashbord.Pricing.MonteCarlo.Payoffs;
using Xunit.Abstractions;

namespace Tests
{
    public class PricingOutputTests
    {
        private readonly ITestOutputHelper _output;
        public PricingOutputTests(ITestOutputHelper output) => _output = output;

        [Fact]
        public async Task PrintAllPrices_DefaultParameters()
        {
            var snap = UnitTestPricing.DefaultSnapshot();
            // S=100, K=100, σ=20%, r=5%, T=1an
            _output.WriteLine("=== PARAMETRES ===");
            _output.WriteLine($"Spot={snap.SpotPrice}, Strike=100, Vol={snap.ImpliedVolatility}, Rate={snap.RiskFreeRate}, T=1an");
            _output.WriteLine("");

            // Black-Scholes
            var call_instr = UnitTestPricing.CallInstrument();
            var put_instr = UnitTestPricing.PutInstrument();
            double call = await new VanillaOptionPricer(ProductType.Call).CalculatePriceOnly(call_instr, snap);
            double put = await new VanillaOptionPricer(ProductType.Put).CalculatePriceOnly(put_instr, snap);
            var call_greeks = await new VanillaOptionPricer(ProductType.Call).CalculateGreeks(call_instr, snap);

            _output.WriteLine("=== BLACK-SCHOLES ===");
            _output.WriteLine($"Call price : {call:F6}  (référence théorique ATM ≈ 10.45)");
            _output.WriteLine($"Put  price : {put:F6}");
            _output.WriteLine($"Parité C - S + K*e^(-rT) = {call - snap.SpotPrice + 100 * Math.Exp(-snap.RiskFreeRate * call_instr.YearsToMaturity):F6}  (doit = Put)");
            _output.WriteLine($"Greeks BS Call — Delta={call_greeks.Delta:F6}, Gamma={call_greeks.Gamma:F6}, Vega={call_greeks.Vega:F6}, Theta={call_greeks.Theta:F6}, Rho={call_greeks.Rho:F6}");
            _output.WriteLine("");

            // Monte Carlo — Asian
            var asian_instr = UnitTestPricing.AsianCallInstrument(observationCount: 12);
            var pricer_asian = new MonteCarloOptionPricer(
                ProductType.AsianCall,
                new ArithmeticAsianCallPayoff(100, 12),
                new MonteCarloConfig { NumberOfPaths = 50_000 }
            );
            var asian_result = await pricer_asian.CalculatePrice(asian_instr, snap);

            _output.WriteLine("=== MONTE CARLO — ASIAN CALL (N=12) ===");
            _output.WriteLine($"Prix      : {asian_result.TheoreticalPrice:F6}  (doit < Call BS {call:F6})");
            _output.WriteLine($"CI 95%    : ±{asian_result.ConfidenceInterval:F6}");
            _output.WriteLine($"Greeks MC — Delta={asian_result.Greeks.Delta:F6}, Gamma={asian_result.Greeks.Gamma:F6}, Vega={asian_result.Greeks.Vega:F6}, Theta={asian_result.Greeks.Theta:F6}, Rho={asian_result.Greeks.Rho:F6}");
            _output.WriteLine("");

            // Monte Carlo — Lookback
            var lb_instr = UnitTestPricing.LookbackCallInstrument(strike: 90);
            var pricer_lb = new MonteCarloOptionPricer(
                ProductType.LookbackCall,
                new LookbackFixedStrikeCallPayoff(90.0),
                new MonteCarloConfig { NumberOfPaths = 50_000 }
            );
            var lb_result = await pricer_lb.CalculatePrice(lb_instr, snap);
            double vanilla_90 = await new VanillaOptionPricer(ProductType.Call).CalculatePriceOnly(
                UnitTestPricing.CallInstrument(strike: 90), snap);

            _output.WriteLine("=== MONTE CARLO — LOOKBACK CALL (K=90) ===");
            _output.WriteLine($"Prix      : {lb_result.TheoreticalPrice:F6}  (doit > Call BS K=90 : {vanilla_90:F6})");
            _output.WriteLine($"CI 95%    : ±{lb_result.ConfidenceInterval:F6}");
            _output.WriteLine($"Greeks MC — Delta={lb_result.Greeks.Delta:F6}, Gamma={lb_result.Greeks.Gamma:F6}, Vega={lb_result.Greeks.Vega:F6}, Theta={lb_result.Greeks.Theta:F6}, Rho={lb_result.Greeks.Rho:F6}");
        }
    }
}
