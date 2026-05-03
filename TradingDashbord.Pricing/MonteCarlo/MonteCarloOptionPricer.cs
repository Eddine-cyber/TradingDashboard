using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TradingDashboard.Core.Entities;
using TradingDashboard.Core.Enums;
using TradingDashboard.Core.Interfaces;

namespace TradingDashbord.Pricing.MonteCarlo
{
    public class MonteCarloOptionPricer : IPricer<PricingResult>
    {
        private ProductType _supportedproduct;
        public ProductType SupportedProduct { get => _supportedproduct; }
        public IPayoff Payoff { get; init; }
        public int NumberOfPaths { get; set; }
        public int Steps { get; init; }
        public double[] Perturbations { get; init; }
        public MonteCarloOptionPricer(ProductType supportedproduct, IPayoff payoff, MonteCarloConfig _config)
        {
            Payoff = payoff;
            Steps = _config.Steps;
            NumberOfPaths = _config.NumberOfPaths;
            Perturbations = _config.Perturbations ?? [0.01, 0.01, 0.01, 0.01, 1.0]; // Delta, Gamma, Vega, Rho, Theta 
            if (!supportedproduct.IsOption())
                throw new Exception("ProductType must be an option");
            _supportedproduct = supportedproduct;
        }
        public async Task<PricingResult> CalculatePrice(Instrument instrument, MarketSnapshot snapshot)
        {
            GBMPathSimulator simulator = new GBMPathSimulator(snapshot.SpotPrice, snapshot.RiskFreeRate, snapshot.ImpliedVolatility, instrument.YearsToMaturity, Steps);
            var (price, confidenceInterval) = CalculatePriceAndStd(simulator, instrument);

            Greeks greeks = await CalculateGreeks(instrument, snapshot);
            PricingResult results = new PricingResult(price, greeks, "MonteCarlo", confidenceInterval);
            return results;
        }
        public Task<Greeks> CalculateGreeks(Instrument instrument, MarketSnapshot snapshot)
        {
            GBMPathSimulator Simulator = new GBMPathSimulator(snapshot.SpotPrice, snapshot.RiskFreeRate, snapshot.ImpliedVolatility, instrument.YearsToMaturity, Steps);
            // Delta & Gamma — bump on Spot
            double h_spot = Simulator.Spot * Perturbations[0];
            GBMPathSimulator sim_spot_left = new(Simulator.Spot - h_spot, Simulator.Rate, Simulator.Vol, Simulator.MaturityInYears, Simulator.Steps);
            GBMPathSimulator sim_spot_right = new(Simulator.Spot + h_spot, Simulator.Rate, Simulator.Vol, Simulator.MaturityInYears, Simulator.Steps);

            double price_left = CalculatePriceOnly(sim_spot_left, instrument);
            double price_right = CalculatePriceOnly(sim_spot_right, instrument);
            double price_center = CalculatePriceOnly(Simulator, instrument);

            double Delta = (price_right - price_left) / (2 * h_spot);
            double Gamma = (price_right - 2 * price_center + price_left) / (h_spot * h_spot);

            // Vega — bump on Vol
            double h_vol = Perturbations[2];
            GBMPathSimulator sim_vol_left = new(Simulator.Spot, Simulator.Rate, Simulator.Vol - h_vol, Simulator.MaturityInYears, Simulator.Steps);
            GBMPathSimulator sim_vol_right = new(Simulator.Spot, Simulator.Rate, Simulator.Vol + h_vol, Simulator.MaturityInYears, Simulator.Steps);

            double Vega = (CalculatePriceOnly(sim_vol_right, instrument) - CalculatePriceOnly(sim_vol_left, instrument)) / (2 * h_vol);

            // Rho — bump on Rate
            double h_rate = Perturbations[3];
            GBMPathSimulator sim_rate_left = new(Simulator.Spot, Simulator.Rate - h_rate, Simulator.Vol, Simulator.MaturityInYears, Simulator.Steps);
            GBMPathSimulator sim_rate_right = new(Simulator.Spot, Simulator.Rate + h_rate, Simulator.Vol, Simulator.MaturityInYears, Simulator.Steps);

            double Rho = (CalculatePriceOnly(sim_rate_right, instrument) - CalculatePriceOnly(sim_rate_left, instrument)) / (2 * h_rate);

            // Theta — bump on Maturity (on instrument, not on simulator)
            int h_days = (int)Perturbations[4];
            GBMPathSimulator sim_T_left = new(Simulator.Spot, Simulator.Rate, Simulator.Vol, Simulator.MaturityInYears - h_days / 365.0, Simulator.Steps);
            GBMPathSimulator sim_T_right = new(Simulator.Spot, Simulator.Rate, Simulator.Vol, Simulator.MaturityInYears + h_days / 365.0, Simulator.Steps);

            double Theta = -(CalculatePriceOnly(sim_T_right, instrument) - CalculatePriceOnly(sim_T_left, instrument)) / (2 * h_days / 365.0);

            return Task.FromResult(new Greeks(Delta, Gamma, Vega, Theta, Rho, DateTimeOffset.UtcNow));
        }

        public (double, double) CalculatePriceAndStd(GBMPathSimulator simulator, Instrument instrument)
        {
            if (!(this.SupportedProduct == instrument.ProductType))
                throw new Exception("the instrument typed is not the one supported by the pricer");

            double actualisationRate = Math.Exp(-instrument.YearsToMaturity * simulator.Rate);
            double[] payoffs = SimulatePayoffs(simulator);
            double mean = payoffs.Average();
            double price = mean * actualisationRate;

            double std_diviation = 0.0;
            for (int i = 0; i < NumberOfPaths; i++)
            {
                std_diviation += (payoffs[i] - mean) * (payoffs[i] - mean);
            }
            std_diviation /= (NumberOfPaths - 1);
            std_diviation = Math.Sqrt(std_diviation) * actualisationRate;

            double std_error = std_diviation / Math.Sqrt(NumberOfPaths);
            double confidenceInterval = 1.96 * std_error;

            return (price, confidenceInterval);
        }

        private double CalculatePriceOnly(GBMPathSimulator simulator, Instrument instrument)
        {
            if (!(this.SupportedProduct == instrument.ProductType))
                throw new Exception("the instrument typed is not the one supported by the pricer");

            double actualisationRate = Math.Exp(-simulator.Rate * simulator.MaturityInYears);
            double[] payoffs = SimulatePayoffs(simulator);
            return payoffs.Average() * actualisationRate;
        }

        private double[] SimulatePayoffs(GBMPathSimulator simulator)
        {
            double[][] paths = simulator.SimulatePathsParallel(NumberOfPaths);
            double[] payoffs = new double[NumberOfPaths];
            for (int i = 0; i < NumberOfPaths; i++)
    {
                payoffs[i] = Payoff.Compute(paths[i]);
            }
            return payoffs;
        }
    }
}
