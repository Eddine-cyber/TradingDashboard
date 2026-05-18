# TradingDashboard

This project is a pricing and risk management system for a multi-asset and multi-currency portfolio (Vanilla, Exotic, FX, Quanto, Futures/Forwards options), developed in C# 12 / .NET 10.
The project covers the valuation of financial instruments using a vectorized architecture (analytical Black-Scholes, multi-dimensional Monte Carlo, LV, LS, LSV models (work in progress)), sensitivity calculations (Greeks), real-time PnL tracking, VaR/CVaR/TailLosses calculations, risk limit checking, and stress testing through shock scenarios with a global correlation matrix.
---

## Status 

This project is currently under development.

Completed modules:
- Core
- Pricing
- Risk

In progress:
- Data/MarketData
- Reporting
- Dashboard

## Architecture

The solution is organized into 8 projects, with unidirectional dependencies.

| Project                       | Type          | Responsibility                                                  |
|-------------------------------|---------------|-----------------------------------------------------------------|
| TradingDashboard.Core         | Class Library | Entities, interfaces, enums.                                     |
| TradingDashboard.Pricing      | Class Library | Black-Scholes, Monte Carlo, Greeks, LV, LS (Heston), LSV (WIP)   |
| TradingDashboard.Data         | Class Library | Entity Framework Core, migrations, Repository pattern.           |
| TradingDashboard.MarketData   | Class Library | Alpha Vantage client, memory cache.            |
| TradingDashboard.Risk         | Class Library | PnL Engine, LimitChecker, StressTestEngine, Observer pattern.    |
| TradingDashboard.Reports      | Class Library | Excel export via EPPlus, PDF export via QuestPDF.                |
| TradingDashboard.Dashboard    | Console App   | Entry point, dependency injection, real-time display.            |
| TradingDashboard.Tests        | xUnit Project | Unit and integration tests                                       |

---

## Features

**Pricing (v2.0)**

- Analytical and MC simulation valuation of Vanilla, Exotic (Asian, Barrier, Lookback, Cliquets), Digital, and Multi-asset options (Basket, Worst-Of, Best-Of...).
- Native support for FX options: FX Options and Quanto Options.
- Parallelized and vectorized Monte Carlo engine, powered by an N-dimensional stochastic path generator managing global correlation (Cholesky).
- Analytical or finite difference calculations of Greeks (Delta, Gamma, Vega, Theta, Rho), dynamically aggregated in the domestic currency.

**Market Data Management**
- Data engineering via asynchronous streams (`IObservable`).
- Unified underlying asset management: exchange rates (FX) and stocks (Equity) seamlessly coexist in a global market environment (`MarketEnvironment`).
- On-the-fly filtering and extraction of sub-correlation matrices to optimize memory and reduce cache misses during local pricing.
- Real-time price fetching via the Alpha Vantage API.

**PnL and Risk**
- Daily, MTD, and YTD PnL calculation per position and at the portfolio level.
- VaR, CVaR, TailLosses calculation for a multi-asset and multi-currency position portfolio.
- Continuous checking of risk limits (daily loss, net Delta, net Vega, position size).
- Stress testing on market shock scenarios (shocks on spots, volatilities, and correlations).

**Reporting**
- Export results to Excel and PDF files.

---

## Installation

### Prerequisites

- .NET 10 SDK
- An Alpha Vantage API key ([alphavantage.co](https://www.alphavantage.co))

### Configuration

```bash
git clone https://github.com/Eddine-cybe/TradingDashboard.git
cd TradingDashboard
```

The API key must be set via .NET User Secrets so it isn't version controlled:

```bash
cd TradingDashboard.Dashboard
dotnet user-secrets set "AlphaVantage:ApiKey" "API_KEY"
```

### Database

```bash
cd TradingDashboard.Data
dotnet ef database update
```

### Launch

```bash
cd TradingDashboard.Dashboard
dotnet run
```

### Tests

```bash
dotnet test
```


