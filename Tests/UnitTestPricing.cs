using System;
using System.Collections.Generic;
using System.Text;
using TradingDashboard.Core.Entities;
using TradingDashboard.Core.Enums;
using TradingDashbord.Pricing;
using TradingDashbord.Pricing.MonteCarlo;
using TradingDashbord.Pricing.MonteCarlo.Payoffs;
using TradingDashbord.Pricing.Strategy;
using TradingDashbord.Pricing.BlackScholes;

namespace Tests
{
    internal static class UnitTestPricing
    {
        // Instrument vanille standard : S=100, K=100, T=1an, σ=20%, r=5%
        public static MarketSnapshot DefaultSnapshot() =>
            new MarketSnapshot("TEST", 100.0, 0.20, 0.05, DateTimeOffset.UtcNow, "Test");

        public static Instrument CallInstrument(double strike = 100.0, int daysToMaturity = 365) =>
            new Instrument(Currency.USD, "SPX", DateOnly.FromDateTime(DateTime.Now.AddDays(daysToMaturity)),
                strike, null, Guid.Empty, "TEST_CALL", ProductType.Call);

        public static Instrument PutInstrument(double strike = 100.0, int daysToMaturity = 365) =>
            new Instrument(Currency.USD, "SPX", DateOnly.FromDateTime(DateTime.Now.AddDays(daysToMaturity)),
                strike, null, Guid.Empty, "TEST_PUT", ProductType.Put);

        public static Instrument AsianCallInstrument(double strike = 100.0, int observationCount = 12, int daysToMaturity = 365) =>
            new Instrument(Currency.USD, "SPX", DateOnly.FromDateTime(DateTime.Now.AddDays(daysToMaturity)),
                strike, observationCount, Guid.Empty, "TEST_ASIAN_CALL", ProductType.AsianCall);

        public static Instrument LookbackCallInstrument(double strike = 90.0, int daysToMaturity = 365) =>
            new Instrument(Currency.USD, "SPX", DateOnly.FromDateTime(DateTime.Now.AddDays(daysToMaturity)),
                strike, null, Guid.Empty, "TEST_LOOKBACK", ProductType.LookbackCall);

        public static GBMPathSimulator DefaultSimulator(double spot = 100.0) =>
            new GBMPathSimulator(spot, 0.05, 0.20, 1.0, 252);
    }

    // =============================================================
    // 1. GBMPathSimulator
    // =============================================================
    public class GBMPathSimulatorTests
    {
        // --- Construction ---

        [Fact]
        public void Constructor_NegativeSpot_ShouldThrow()
        {
            Assert.Throws<Exception>(() => new GBMPathSimulator(-1, 0.05, 0.2, 1.0, 252));
        }

        [Fact]
        public void Constructor_ZeroSpot_ShouldThrow()
        {
            Assert.Throws<Exception>(() => new GBMPathSimulator(0, 0.05, 0.2, 1.0, 252));
        }

        [Fact]
        public void Constructor_NegativeVolatility_ShouldThrow()
        {
            Assert.Throws<Exception>(() => new GBMPathSimulator(100, 0.05, -0.1, 1.0, 252));
        }

        [Fact]
        public void Constructor_ZeroMaturity_ShouldThrow()
        {
            Assert.Throws<Exception>(() => new GBMPathSimulator(100, 0.05, 0.2, 0.0, 252));
        }

        [Fact]
        public void Constructor_NegativeMaturity_ShouldThrow()
        {
            Assert.Throws<Exception>(() => new GBMPathSimulator(100, 0.05, 0.2, -1.0, 252));
        }

        [Fact]
        public void Constructor_ZeroSteps_ShouldThrow()
        {
            Assert.Throws<Exception>(() => new GBMPathSimulator(100, 0.05, 0.2, 1.0, 0));
        }

        [Fact]
        public void Constructor_ZeroVolatility_ShouldNotThrow()
        {
            // vol=0 est mathématiquement valide : prix final = S*e^{rT} déterministe
            var sim = new GBMPathSimulator(100, 0.05, 0.0, 1.0, 252);
            Assert.NotNull(sim);
        }

        [Fact]
        public void Constructor_NegativeRate_ShouldNotThrow()
        {
            // taux négatif est valide (contexte taux japonais, suisses)
            var sim = new GBMPathSimulator(100, -0.01, 0.2, 1.0, 252);
            Assert.NotNull(sim);
        }

        // --- SimulatePath ---

        [Fact]
        public void SimulatePath_LengthShouldBe_StepsPlusOne()
        {
            var sim = UnitTestPricing.DefaultSimulator();
            var rng = new Random(42);
            double[] path = sim.SimulatePath(rng);
            Assert.Equal(253, path.Length); // steps=252 → 253 points (S0 inclus)
        }

        [Fact]
        public void SimulatePath_FirstElementShouldBe_Spot()
        {
            var sim = UnitTestPricing.DefaultSimulator(spot: 150.0);
            var rng = new Random(42);
            double[] path = sim.SimulatePath(rng);
            Assert.Equal(150.0, path[0], precision: 10);
        }

        [Fact]
        public void SimulatePath_AllValuesShouldBeStrictlyPositive()
        {
            // GBM : S_t = S_0 * exp(...) > 0 toujours
            var sim = UnitTestPricing.DefaultSimulator();
            var rng = new Random(42);
            double[] path = sim.SimulatePath(rng);
            Assert.All(path, price => Assert.True(price > 0, $"Prix négatif ou nul : {price}"));
        }

        [Fact]
        public void SimulatePath_ZeroVol_ShouldProduceDeterministicPath()
        {
            // vol=0 → S_t = S_0 * e^{r*t}, pas d'aléatoire
            var sim = new GBMPathSimulator(100, 0.05, 0.0, 1.0, 252);
            var path1 = sim.SimulatePath(new Random(1));
            var path2 = sim.SimulatePath(new Random(999));
            // Les deux paths doivent être identiques car pas de terme stochastique
            Assert.Equal(path1, path2);
        }

        [Fact]
        public void SimulatePath_TwoDifferentSeeds_ShouldProduceDifferentPaths()
        {
            var sim = UnitTestPricing.DefaultSimulator();
            var path1 = sim.SimulatePath(new Random(1));
            var path2 = sim.SimulatePath(new Random(2));
            Assert.False(path1.SequenceEqual(path2));
        }

        [Fact]
        public void SimulatePath_SameSeed_ShouldBeReproducible()
        {
            var sim = UnitTestPricing.DefaultSimulator();
            var path1 = sim.SimulatePath(new Random(42));
            var path2 = sim.SimulatePath(new Random(42));
            Assert.Equal(path1, path2);
        }

        // --- SimulatePaths ---

        [Fact]
        public void SimulatePaths_CountShouldMatch_NumberOfPaths()
        {
            var sim = UnitTestPricing.DefaultSimulator();
            var paths = sim.SimulatePaths(500, new Random(42));
            Assert.Equal(500, paths.Length);
        }

        [Fact]
        public void SimulatePaths_EachPathShouldStartAtSpot()
        {
            var sim = UnitTestPricing.DefaultSimulator(spot: 120.0);
            var paths = sim.SimulatePaths(100, new Random(42));
            Assert.All(paths, path => Assert.Equal(120.0, path[0], precision: 10));
        }

        // --- SimulatePathsParallel ---

        [Fact]
        public void SimulatePathsParallel_CountShouldMatch_NumberOfPaths()
        {
            var sim = UnitTestPricing.DefaultSimulator();
            var paths = sim.SimulatePathsParallel(1000);
            Assert.Equal(1000, paths.Length);
        }

        [Fact]
        public void SimulatePathsParallel_NoNullPaths()
        {
            var sim = UnitTestPricing.DefaultSimulator();
            var paths = sim.SimulatePathsParallel(500);
            Assert.All(paths, path => Assert.NotNull(path));
        }

        [Fact]
        public void SimulatePathsParallel_AllPathsStartAtSpot()
        {
            var sim = UnitTestPricing.DefaultSimulator(spot: 200.0);
            var paths = sim.SimulatePathsParallel(200);
            Assert.All(paths, path => Assert.Equal(200.0, path[0], precision: 10));
        }

        [Fact]
        public void SimulatePathsParallel_AllValuesStrictlyPositive()
        {
            var sim = UnitTestPricing.DefaultSimulator();
            var paths = sim.SimulatePathsParallel(100);
            foreach (var path in paths)
                Assert.All(path, price => Assert.True(price > 0));
        }
    }

    // =============================================================
    // 2. AsianCallPayoff
    // =============================================================
    public class AsianCallPayoffTests
    {
        [Fact]
        public void Constructor_ZeroObservationCount_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => new ArithmeticAsianCallPayoff(100, 0));
        }

        [Fact]
        public void Compute_ObservationCountGreaterThanPath_ShouldThrow()
        {
            var payoff = new ArithmeticAsianCallPayoff(100, 500);
            double[] path = new double[10];
            Assert.Throws<Exception>(() => payoff.Compute(path));
        }

        [Fact]
        public void Compute_PayoffShouldBeNonNegative()
        {
            // Le payoff d'une option est toujours >= 0 par définition
            var payoff = new ArithmeticAsianCallPayoff(strike: 100, observationCount: 12);
            double[] path = Enumerable.Range(0, 253).Select(i => 80.0).ToArray(); // ITM=false, prix=80
            double result = payoff.Compute(path);
            Assert.True(result >= 0);
        }

        [Fact]
        public void Compute_ConstantPathAboveStrike_ShouldReturnMeanMinusStrike()
        {
            // path constant à 110, strike 100 → payoff = 10
            var payoff = new ArithmeticAsianCallPayoff(strike: 100, observationCount: 4);
            double[] path = Enumerable.Repeat(110.0, 5).ToArray(); // 5 points, steps=4
            double result = payoff.Compute(path);
            Assert.Equal(10.0, result, precision: 6);
        }

        [Fact]
        public void Compute_ConstantPathBelowStrike_ShouldReturnZero()
        {
            var payoff = new ArithmeticAsianCallPayoff(strike: 100, observationCount: 4);
            double[] path = Enumerable.Repeat(90.0, 5).ToArray();
            Assert.Equal(0.0, payoff.Compute(path));
        }

        [Fact]
        public void Compute_SamplingIndices_ShouldNotUseAllPoints()
        {
            // steps=4, observationCount=2 → indices {2, 4} (pas 1,2,3,4)
            // path = [100, 999, 110, 999, 90] → indices {2,4} → moyenne = (110+90)/2 = 100 → payoff = 0
            var payoff = new ArithmeticAsianCallPayoff(strike: 100, observationCount: 2);
            double[] path = [100, 999, 110, 999, 90];
            double result = payoff.Compute(path);
            Assert.Equal(0.0, result, precision: 6);
        }
    }

    // =============================================================
    // 3. LookbackFixedStrikeCallPayoff
    // =============================================================
    public class LookbackCallPayoffTests
    {
        [Fact]
        public void Compute_EmptyPath_ShouldThrow()
        {
            var payoff = new LookbackFixedStrikeCallPayoff(100.0);
            Assert.Throws<ArgumentException>(() => payoff.Compute(Array.Empty<double>()));
        }

        [Fact]
        public void Compute_PayoffShouldBeNonNegative()
        {
            var payoff = new LookbackFixedStrikeCallPayoff(100.0);
            double[] path = [100, 90, 85, 88, 92]; // max=100, payoff=0
            Assert.True(payoff.Compute(path) >= 0);
        }

        [Fact]
        public void Compute_MaxBelowStrike_ShouldReturnZero()
        {
            var payoff = new LookbackFixedStrikeCallPayoff(strike: 120.0);
            double[] path = [100, 105, 110, 108, 112]; // max=112 < strike=120
            Assert.Equal(0.0, payoff.Compute(path));
        }

        [Fact]
        public void Compute_KnownMax_ShouldReturnMaxMinusStrike()
        {
            var payoff = new LookbackFixedStrikeCallPayoff(strike: 100.0);
            double[] path = [100, 120, 90, 150, 80]; // max=150
            Assert.Equal(50.0, payoff.Compute(path), precision: 10);
        }

        [Fact]
        public void Compute_ObservesEntirePath_NotJustLastPoint()
        {
            // Le max est au milieu, pas à la fin
            var payoff = new LookbackFixedStrikeCallPayoff(strike: 100.0);
            double[] path = [100, 200, 110, 105, 100]; // max=200, ST=100
            Assert.Equal(100.0, payoff.Compute(path), precision: 10); // et pas 0.0
        }
    }

    // =============================================================
    // 4. VanillaOptionPricer — Black-Scholes
    // =============================================================
    public class VanillaOptionPricerTests
    {
        private readonly MarketSnapshot _snap = UnitTestPricing.DefaultSnapshot();

        [Fact]
        public async Task CalculatePrice_Call_ShouldBePositive()
        {
            var instrument = UnitTestPricing.CallInstrument();
            var pricer = new VanillaOptionPricer(ProductType.Call);
            double price = await pricer.CalculatePriceOnly(instrument, _snap);
            Assert.True(price > 0, $"Call price négatif : {price}");
        }

        [Fact]
        public async Task CalculatePrice_Put_ShouldBePositive()
        {
            var instrument = UnitTestPricing.PutInstrument();
            var pricer = new VanillaOptionPricer(ProductType.Put);
            double price = await pricer.CalculatePriceOnly(instrument, _snap);
            Assert.True(price > 0);
        }

        [Fact]
        public async Task CalculatePrice_WrongProductType_ShouldThrow()
        {
            // Pricer configuré pour Call mais reçoit un Put
            var instrument = UnitTestPricing.PutInstrument();
            var pricer = new VanillaOptionPricer(ProductType.Call);
            await Assert.ThrowsAsync<Exception>(() => pricer.CalculatePrice(instrument, _snap));
        }

        [Fact]
        public async Task CalculatePrice_CallPutParity_ShouldHoldTo_1e6()
        {
            // C - S + K*e^{-rT} = P
            var call_instr = UnitTestPricing.CallInstrument(strike: 100);
            var put_instr = UnitTestPricing.PutInstrument(strike: 100);
            double C = await new VanillaOptionPricer(ProductType.Call).CalculatePriceOnly(call_instr, _snap);
            double P = await new VanillaOptionPricer(ProductType.Put).CalculatePriceOnly(put_instr, _snap);
            double T = call_instr.YearsToMaturity;
            double K = 100.0;
            double parity = C - _snap.SpotPrice + K * Math.Exp(-_snap.RiskFreeRate * T);
            Assert.Equal(parity, P, precision: 6);
        }

        [Fact]
        public async Task CalculatePrice_CallMoneyness_OTMlessThanATMlessThanITM()
        {
            // Call OTM (K=120) < ATM (K=100) < ITM (K=80)
            var otm = UnitTestPricing.CallInstrument(strike: 120);
            var atm = UnitTestPricing.CallInstrument(strike: 100);
            var itm = UnitTestPricing.CallInstrument(strike: 80);
            var pricer = new VanillaOptionPricer(ProductType.Call);
            double pOTM = await pricer.CalculatePriceOnly(otm, _snap);
            double pATM = await pricer.CalculatePriceOnly(atm, _snap);
            double pITM = await pricer.CalculatePriceOnly(itm, _snap);
            Assert.True(pOTM < pATM && pATM < pITM,
                $"OTM={pOTM:F4}, ATM={pATM:F4}, ITM={pITM:F4}");
        }

        [Fact]
        public async Task CalculatePrice_HigherVol_ShouldIncreaseCallPrice()
        {
            var instrument = UnitTestPricing.CallInstrument();
            var snapLowVol = new MarketSnapshot("T", 100, 0.10, 0.05, DateTimeOffset.UtcNow, "T");
            var snapHighVol = new MarketSnapshot("T", 100, 0.40, 0.05, DateTimeOffset.UtcNow, "T");
            var pricer = new VanillaOptionPricer(ProductType.Call);
            double pLow = await pricer.CalculatePriceOnly(instrument, snapLowVol);
            double pHigh = await pricer.CalculatePriceOnly(instrument, snapHighVol);
            Assert.True(pHigh > pLow);
        }

        // --- Greeks ---

        [Fact]
        public async Task Greeks_CallDelta_ShouldBeBetween_0_And_1()
        {
            var instrument = UnitTestPricing.CallInstrument();
            var greeks = await new VanillaOptionPricer(ProductType.Call).CalculateGreeks(instrument, _snap);
            Assert.InRange(greeks.Delta, 0.0, 1.0);
        }

        [Fact]
        public async Task Greeks_PutDelta_ShouldBeBetween_Minus1_And_0()
        {
            var instrument = UnitTestPricing.PutInstrument();
            var greeks = await new VanillaOptionPricer(ProductType.Put).CalculateGreeks(instrument, _snap);
            Assert.InRange(greeks.Delta, -1.0, 0.0);
        }

        [Fact]
        public async Task Greeks_Gamma_ShouldBeStrictlyPositive()
        {
            var instrument = UnitTestPricing.CallInstrument();
            var greeks = await new VanillaOptionPricer(ProductType.Call).CalculateGreeks(instrument, _snap);
            Assert.True(greeks.Gamma > 0, $"Gamma={greeks.Gamma}");
        }

        [Fact]
        public async Task Greeks_Vega_ShouldBePositive()
        {
            var instrument = UnitTestPricing.CallInstrument();
            var greeks = await new VanillaOptionPricer(ProductType.Call).CalculateGreeks(instrument, _snap);
            Assert.True(greeks.Vega > 0, $"Vega={greeks.Vega}");
        }

        [Fact]
        public async Task Greeks_CallTheta_ShouldBeNegative()
        {
            // Time decay : valeur diminue quand T diminue
            var instrument = UnitTestPricing.CallInstrument();
            var greeks = await new VanillaOptionPricer(ProductType.Call).CalculateGreeks(instrument, _snap);
            Assert.True(greeks.Theta < 0, $"Theta={greeks.Theta}");
        }

        [Fact]
        public async Task Greeks_CallRho_ShouldBePositive()
        {
            var instrument = UnitTestPricing.CallInstrument();
            var greeks = await new VanillaOptionPricer(ProductType.Call).CalculateGreeks(instrument, _snap);
            Assert.True(greeks.Rho > 0, $"Rho={greeks.Rho}");
        }

        [Fact]
        public async Task Greeks_ATMCallDelta_ShouldBeApprox_0point5()
        {
            // ATM call : N(d1) ≈ 0.5 + ε (légèrement au-dessus)
            var instrument = UnitTestPricing.CallInstrument(strike: 100);
            var greeks = await new VanillaOptionPricer(ProductType.Call).CalculateGreeks(instrument, _snap);
            Assert.InRange(greeks.Delta, 0.45, 0.65);
        }
    }

    // =============================================================
    // 5. MonteCarloOptionPricer
    // =============================================================
    public class MonteCarloOptionPricerTests
    {
        private static MonteCarloOptionPricer BuildAsianPricer(int paths = 10_000) =>
            new MonteCarloOptionPricer(
                ProductType.AsianCall,
                new ArithmeticAsianCallPayoff(strike: 100, observationCount: 12),
                new MonteCarloConfig { NumberOfPaths = paths }
            );

        [Fact]
        public async Task CalculatePrice_ShouldReturnPositivePrice()
        {
            var result = await BuildAsianPricer()
                .CalculatePrice(UnitTestPricing.AsianCallInstrument(), UnitTestPricing.DefaultSnapshot());
            Assert.True(result.TheoreticalPrice > 0, $"Prix={result.TheoreticalPrice}");
        }

        [Fact]
        public async Task CalculatePrice_PricingMethodShouldBe_MonteCarlo()
        {
            var result = await BuildAsianPricer()
                .CalculatePrice(UnitTestPricing.AsianCallInstrument(), UnitTestPricing.DefaultSnapshot());
            Assert.Equal("MonteCarlo", result.PricingMethod);
        }

        [Fact]
        public async Task CalculatePrice_ConfidenceIntervalShouldBePositive()
        {
            var result = await BuildAsianPricer()
                .CalculatePrice(UnitTestPricing.AsianCallInstrument(), UnitTestPricing.DefaultSnapshot());
            Assert.True(result.ConfidenceInterval.HasValue);
            Assert.True(result.ConfidenceInterval.Value > 0);
        }

        [Fact]
        public async Task CalculatePrice_ConfidenceIntervalShouldDecrease_WithMorePaths()
        {
            var instr = UnitTestPricing.AsianCallInstrument();
            var snap = UnitTestPricing.DefaultSnapshot();
            var payoff = new ArithmeticAsianCallPayoff(strike: 100, observationCount: 12);

            var pricer_small = new MonteCarloOptionPricer(ProductType.AsianCall, payoff, new MonteCarloConfig { NumberOfPaths = 1_000 });
            var pricer_large = new MonteCarloOptionPricer(ProductType.AsianCall, payoff, new MonteCarloConfig { NumberOfPaths = 50_000 });

            var r_small = await pricer_small.CalculatePrice(instr, snap);
            var r_large = await pricer_large.CalculatePrice(instr, snap);

            Assert.True(r_large.ConfidenceInterval < r_small.ConfidenceInterval,
                $"CI small={r_small.ConfidenceInterval:F6}, CI large={r_large.ConfidenceInterval:F6}");
        }

        [Fact]
        public async Task CalculatePrice_ComputedAt_ShouldBeRecentTimestamp()
        {
            var before = DateTimeOffset.UtcNow.AddSeconds(-1);
            var result = await BuildAsianPricer(1_000)
                .CalculatePrice(UnitTestPricing.AsianCallInstrument(), UnitTestPricing.DefaultSnapshot());
            Assert.True(result.ComputedAt >= before);
        }

        [Fact]
        public async Task CalculatePrice_HigherSpot_ShouldIncreaseAsianCallPrice()
        {
            var payoff = new ArithmeticAsianCallPayoff(100, 12);
            var instr = UnitTestPricing.AsianCallInstrument();
            var config = new MonteCarloConfig { NumberOfPaths = 20_000 };

            // Le spot vient du snapshot maintenant, plus du simulator
            var snap_low = new MarketSnapshot("TEST", 80.0, 0.20, 0.05, DateTimeOffset.UtcNow, "Test");
            var snap_high = new MarketSnapshot("TEST", 120.0, 0.20, 0.05, DateTimeOffset.UtcNow, "Test");

            var pricer = new MonteCarloOptionPricer(ProductType.AsianCall, payoff, config);
            var r_low = await pricer.CalculatePrice(instr, snap_low);
            var r_high = await pricer.CalculatePrice(instr, snap_high);

            Assert.True(r_high.TheoreticalPrice > r_low.TheoreticalPrice,
                $"Low={r_low.TheoreticalPrice:F4}, High={r_high.TheoreticalPrice:F4}");
        }
    }

    // =============================================================
    // 6. Tests structurels — invariants domaine
    // =============================================================
    public class DomainInvariantTests
    {
        [Fact]
        public async Task AsianCall_ShouldBePricedLessThan_VanillaCall()
        {
            var snap = UnitTestPricing.DefaultSnapshot();
            var instr_v = UnitTestPricing.CallInstrument();
            var instr_a = UnitTestPricing.AsianCallInstrument(observationCount: 252);

            double vanilla = await new VanillaOptionPricer(ProductType.Call).CalculatePriceOnly(instr_v, snap);

            var pricer_a = new MonteCarloOptionPricer(
                ProductType.AsianCall,
                new ArithmeticAsianCallPayoff(100, 252),
                new MonteCarloConfig { NumberOfPaths = 50_000 }
            );
            var asian = await pricer_a.CalculatePrice(instr_a, snap);

            Assert.True((double)asian.TheoreticalPrice < vanilla,
                $"Asian={asian.TheoreticalPrice:F4}, Vanilla={vanilla:F4}");
        }

        [Fact]
        public async Task LookbackCall_ShouldBePricedMoreThan_VanillaCall()
        {
            var snap = UnitTestPricing.DefaultSnapshot();
            var instr_v = UnitTestPricing.CallInstrument(strike: 90);
            var instr_l = UnitTestPricing.LookbackCallInstrument(strike: 90);

            double vanilla = await new VanillaOptionPricer(ProductType.Call).CalculatePriceOnly(instr_v, snap);

            var pricer_l = new MonteCarloOptionPricer(
                ProductType.LookbackCall,
                new LookbackFixedStrikeCallPayoff(90.0),
                new MonteCarloConfig { NumberOfPaths = 50_000 }
            );
            var lookback = await pricer_l.CalculatePrice(instr_l, snap);

            Assert.True((double)lookback.TheoreticalPrice > vanilla,
                $"Lookback={lookback.TheoreticalPrice:F4}, Vanilla={vanilla:F4}");
        }

        [Fact]
        public async Task MonteCarlo_VanillaCall_ShouldConverge_To_BSPrice_Within2Percent()
        {
            var snap = UnitTestPricing.DefaultSnapshot();
            var instr = UnitTestPricing.CallInstrument();
            double bs = await new VanillaOptionPricer(ProductType.Call).CalculatePriceOnly(instr, snap);

            // VanillaCallPayoff doit être implémenté si absent : max(S_T - K, 0)
            var pricer_mc = new MonteCarloOptionPricer(
                ProductType.Call, // nécessite d'assouplir la vérification IsExotic si on veut tester ça
                new VanillaCallPayoff(100.0),
                new MonteCarloConfig { NumberOfPaths = 100_000 }
            );
            var mc = await pricer_mc.CalculatePrice(instr, snap);

            double tolerance = bs * 0.02;
            Assert.InRange((double)mc.TheoreticalPrice, bs - tolerance, bs + tolerance);
        }
    }

    // =============================================================
    // 7. PricingEngine
    // =============================================================
    public class PricingEngineTests
    {
        private static PricingEngine BuildEngine() =>
            new PricingEngine(new BlackScholesPricingStrategy(), new MonteCarloPricingStrategy());

        [Fact]
        public async Task PriceAsync_VanillaCall_ShouldUseBlackScholes()
        {
            var result = await BuildEngine().PriceAsync(UnitTestPricing.CallInstrument(), UnitTestPricing.DefaultSnapshot());
            Assert.Equal("BlackScholes", result.PricingMethod);
        }

        [Fact]
        public async Task PriceAsync_AsianCall_ShouldUseMonteCarlo()
        {
            var result = await BuildEngine().PriceAsync(UnitTestPricing.AsianCallInstrument(), UnitTestPricing.DefaultSnapshot());
            Assert.Equal("MonteCarlo", result.PricingMethod);
        }

        [Fact]
        public async Task PricePortfolioAsync_MixedPortfolio_ShouldReturnAllResults()
        {
            var portfolio = new List<Instrument>
        {
            UnitTestPricing.CallInstrument(),
            UnitTestPricing.PutInstrument(),
            UnitTestPricing.AsianCallInstrument(),
            UnitTestPricing.LookbackCallInstrument()
        };
            var results = await BuildEngine().PricePortfolioAsync(portfolio, UnitTestPricing.DefaultSnapshot());
            Assert.Equal(4, results.Count);
            Assert.All(results, r => Assert.True(r.TheoreticalPrice > 0));
        }

        [Fact]
        public void SwitchStrategy_ShouldBeThreadSafe()
        {
            var engine = BuildEngine();
            var strategy_a = new MonteCarloPricingStrategy(new MonteCarloConfig { NumberOfPaths = 1_000 });
            var strategy_b = new MonteCarloPricingStrategy(new MonteCarloConfig { NumberOfPaths = 5_000 });
            Parallel.For(0, 100, i => engine.SwitchStrategy(i % 2 == 0 ? strategy_a : strategy_b));
        }

        [Fact]
        public async Task PricePortfolioAsync_ShouldFinishWithinOneMinute()
        {
            var portfolio = Enumerable.Repeat(UnitTestPricing.AsianCallInstrument(), 4).ToList();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            await BuildEngine().PricePortfolioAsync(portfolio, UnitTestPricing.DefaultSnapshot());
            sw.Stop();
            Assert.True(sw.ElapsedMilliseconds < 60000);
        }
    }

    // =============================================================
    // 8. PricingResult — invariants du record
    // =============================================================
    public class PricingResultTests
    {
        [Fact]
        public void Constructor_InvalidPricingMethod_ShouldThrow()
        {
            Assert.Throws<Exception>(() =>
                new PricingResult(100.0, new Greeks(), "FiniteDifference", null));
        }

        [Fact]
        public void Constructor_BlackScholes_ConfidenceIntervalShouldBeNull()
        {
            var result = new PricingResult(100.0, new Greeks(), "BlackScholes", 0.5);
            Assert.Null(result.ConfidenceInterval); // doit être forcé à null pour BS
        }

        [Fact]
        public void Constructor_MonteCarlo_ConfidenceIntervalShouldBePreserved()
        {
            var result = new PricingResult(100.0, new Greeks(), "MonteCarlo", 0.25);
            Assert.Equal(0.25, result.ConfidenceInterval);
        }

    }
}