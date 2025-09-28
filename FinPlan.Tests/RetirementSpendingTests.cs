using FinPlan.Shared.Models.Spending;

namespace FinPlan.Tests
{
    public class RetirementSpendingTests
    {
        [Fact]
        public void Calculate_ShouldPopulateYearRows_WithExpectedValues()
        {
            // Arrange
            var model = new CalendarSpendingModel
            {
                CurrentAgeYou = 55,
                CurrentAgePartner = 52,
                RetirementAgeYou = 67,
                RetirementAgePartner = 67,
                LifeExpectancyYou = 99,
                LifeExpectancyPartner = 99,
                TaxableBalance = 200_000m,
                TraditionalBalance = 100_000m,
                RothBalance = 100_000m,
                SocialSecurityMonthlyYou = 4000,
                SocialSecurityMonthlyPartner = 3000m,
                AnnualWithdrawalOne = 120_000m,
                AnnualWithdrawalBoth = 150_000m,
                InvestmentReturn = 5.0m,
                InflationRate = 2.5m,
                TraditionalTaxRate = 20.0m,
                ReverseMortgageMonthly = 5000m,
                ReverseMortgageStartYear=2038,
                SimulationStartYear = 2032

            };

            // Act
            model.Calculate();

            // Assert
            Assert.NotEmpty(model.YearRows);
            var firstRow = model.YearRows[0];
            Assert.Equal(model.SimulationStartYear, firstRow.Year);
            Assert.True(firstRow.AgeYou >= model.CurrentAgeYou);
            Assert.True(firstRow.AgePartner >= model.CurrentAgePartner);
            // Check that AmountNeededForCostOfLiving is zero before retirement
            Assert.Equal(0m, firstRow.AmountNeededForCostOfLiving);
            // Check that AmountNeededForCostOfLiving is set after retirement
            var retirementRow = model.YearRows.Find(r => r.Year == model.RetirementYearYou);
            Assert.NotNull(retirementRow);
            Assert.True(retirementRow.AmountNeededForCostOfLiving > 0);
        }
    }
}
