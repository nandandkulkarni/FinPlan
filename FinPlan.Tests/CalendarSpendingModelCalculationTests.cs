using Xunit;
using FinPlan.Shared.Models.Spending;

namespace FinPlan.Tests
{
    public class CalendarSpendingModelCalculationTests
    {
        [Fact]
        public void CalculateNetNeededFromAccounts_ShouldReturnCorrectValue()
        {
            var model = new CalendarSpendingModel { };
            Assert.Equal(500m, model.CalculateNetNeededFromAccounts(1000m, 500m));
            Assert.Equal(0m, model.CalculateNetNeededFromAccounts(500m, 1000m));
        }

        [Fact]
        public void CalculateWithdrawals_ShouldReturnCorrectValues()
        {
            var model = new CalendarSpendingModel { };
            var (taxable, trad, roth, x) = model.CalculateWithdrawals(1000m, 500m, 200m, 1200m);
            Assert.Equal(1000m, taxable);
            Assert.Equal(200m, trad);
            Assert.Equal(0m, roth);
        }

        [Fact]
        public void CalculateEstimatedTaxableSS_ShouldMatchStaticMethod()
        {
            var model = new CalendarSpendingModel { };
            decimal ssTotal = 30000m;
            decimal otherIncome = 20000m;
            Assert.Equal(CalendarSpendingModel.EstimateTaxableSocialSecurity(ssTotal, otherIncome, true), model.CalculateEstimatedTaxableSS(ssTotal, otherIncome));
        }

        [Fact]
        public void CalculateTaxOnTraditional_ShouldReturnCorrectValue()
        {
            var model = new CalendarSpendingModel { TraditionalTaxRate = 20m };
            Assert.Equal(200m, model.CalculateTaxOnTraditional(1000m));
        }

        [Fact]
        public void CalculateTaxOnSS_ShouldReturnCorrectValue()
        {
            var model = new CalendarSpendingModel { TraditionalTaxRate = 20m };
            Assert.Equal(40m, model.CalculateTaxOnSS(200m));
        }

        [Fact]
        public void CalculateTaxOnTaxableGrowth_ShouldReturnCorrectValue()
        {
            var model = new CalendarSpendingModel { InvestmentReturn = 10m, TraditionalTaxRate = 20m };
            Assert.Equal(200m, model.CalculateTaxOnTaxableGrowth(10000m));
        }

        [Fact]
        public void CalculateGrowth_ShouldReturnCorrectValue()
        {
            var model = new CalendarSpendingModel { InvestmentReturn = 5m };
            Assert.Equal(50m, model.CalculateGrowth(1000m));
        }

        [Fact]
        public void CalculateTotalGrowth_ShouldReturnSumOfGrowths()
        {
            var model = new CalendarSpendingModel { InvestmentReturn = 10m };
            Assert.Equal(300m, model.CalculateTotalGrowth(1000m, 1000m, 1000m));
        }

        [Fact]
        public void CalculateTaxesPaid_ShouldReturnSumOfTaxes()
        {
            var model = new CalendarSpendingModel { TraditionalTaxRate = 20m };
            // tradWithdraw = 1000, estimatedTaxableSS = 200, taxOnTaxableGrowth = 60
            // Expected: 1000*0.2 + 200*0.2 + 60 = 200 + 40 + 60 = 300
            Assert.Equal(300m, model.CalculateTaxesPaid(1000m, 200m, 60m));
        }

        [Fact]
        public void CalculateEndingBalances_ShouldReturnCorrectValues()
        {
            var model = new CalendarSpendingModel { };
            var (taxable, trad, roth) = model.CalculateEndingBalances(1000m, 500m, 200m, 100m, 50m, 20m);
            Assert.Equal(1100m, taxable);
            Assert.Equal(550m, trad);
            Assert.Equal(220m, roth);
        }
    }
}
