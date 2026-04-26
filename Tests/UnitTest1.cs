using System;
using TradingDashboard.Core.Entities;
using Xunit;

namespace TradingDashboard.Tests
{
    public class GreeksTests
    {
        [Fact]
        public void Delta_ValidValue_ShouldBeAssigned()
        {
            Greeks g = new Greeks();

            g.Delta = 0.5;

            Assert.Equal(0.5, g.Delta);
        }

        [Fact]
        public void Delta_InvalidValue_ShouldThrowException()
        {
            Greeks g = new Greeks();

            Assert.Throws<Exception>(() => g.Delta = 2);
        }

        [Fact]
        public void Gamma_NegativeValue_ShouldThrowException()
        {
            Greeks g = new Greeks();

            Assert.Throws<Exception>(() => g.Gamma = -1);
        }

        [Fact]
        public void Theta_PositiveValue_ShouldThrowException()
        {
            Greeks g = new Greeks();

            Assert.Throws<Exception>(() => g.Theta = 1);
        }

        [Fact]
        public void Zero_ShouldReturnAllValuesAtZero()
        {
            Greeks g = Greeks.Zero();

            Assert.Equal(0, g.Delta);
            Assert.Equal(0, g.Gamma);
            Assert.Equal(0, g.Vega);
            Assert.Equal(0, g.Theta);
            Assert.Equal(0, g.Rho);
        }

        [Fact]
        public void Addition_ShouldSumAllGreeks()
        {
            Greeks g1 = new Greeks();
            g1.Delta = 0.2;
            g1.Gamma = 1;
            g1.Vega = 2;
            g1.Theta = -1;
            g1.Rho = 3;

            Greeks g2 = new Greeks();
            g2.Delta = 0.3;
            g2.Gamma = 2;
            g2.Vega = 1;
            g2.Theta = -2;
            g2.Rho = 1;

            Greeks result = g1 + g2;

            Assert.Equal(0.5, result.Delta);
            Assert.Equal(3, result.Gamma);
            Assert.Equal(3, result.Vega);
            Assert.Equal(-3, result.Theta);
            Assert.Equal(4, result.Rho);
        }
    }
}