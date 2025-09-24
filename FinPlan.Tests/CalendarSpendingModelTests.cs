using Xunit;
using FinPlan.Shared.Models.Spending;

namespace FinPlan.Tests
{
    public class CalendarSpendingModelTests
    {
        [Fact]
        public void Calculate_ShouldPopulateYearRows_WithExpectedValues()
        {
            // Arrange
            var model = new CalendarSpendingModel
            {
                CurrentAgeYou = 60,
                CurrentAgePartner = 58,
                RetirementAgeYou = 65,
                RetirementAgePartner = 63,
                LifeExpectancyYou = 85,
                LifeExpectancyPartner = 83,
                TaxableBalance = 100_000m,
                TraditionalBalance = 200_000m,
                RothBalance = 50_000m,
                SocialSecurityMonthlyYou = 2000m,
                SocialSecurityMonthlyPartner = 1500m,
                AnnualWithdrawalOne = 60_000m,
                AnnualWithdrawalBoth = 80_000m,
                InvestmentReturn = 5.0m,
                InflationRate = 2.5m,
                TraditionalTaxRate = 20.0m
            };

            // Act
            model.Calculate();

            // Assert
            Assert.NotEmpty(model.YearRows);
            var firstRow = model.YearRows[0];
            Assert.Equal(model.SimulationStartYear, firstRow.Year);
            Assert.True(firstRow.AgeYou >= model.CurrentAgeYou);
            Assert.True(firstRow.AgePartner >= model.CurrentAgePartner);
            // Check that AmountNeeded is zero before retirement
            Assert.Equal(0m, firstRow.AmountNeeded);
            // Check that AmountNeeded is set after retirement
            var retirementRow = model.YearRows.Find(r => r.Year == model.RetirementYearYou);
            Assert.NotNull(retirementRow);
            Assert.True(retirementRow.AmountNeeded > 0);
        }

        [Fact]
        public void Calculate_TaxesPaid_ShouldBeCorrect_WhenOnlySocialSecurityIncome()
        {
            // Arrange: Only Social Security income, no account balances
            var model = new CalendarSpendingModel
            {
                CurrentAgeYou = 62,
                CurrentAgePartner = 62,
                RetirementAgeYou = 62,
                RetirementAgePartner = 62,
                LifeExpectancyYou = 70,
                LifeExpectancyPartner = 70,
                TaxableBalance = 0m,
                TraditionalBalance = 0m,
                RothBalance = 0m,
                SocialSecurityMonthlyYou = 2000m,
                SocialSecurityMonthlyPartner = 1500m,
                AnnualWithdrawalOne = 42000m, // 2000+1500 * 12
                AnnualWithdrawalBoth = 42000m,
                InvestmentReturn = 0.0m,
                InflationRate = 0.0m,
                TraditionalTaxRate = 20.0m
            };

            // Act
            model.Calculate();

            // Assert: TaxesPaid should match IRS rule for all years
            Assert.NotEmpty(model.YearRows);
            foreach (var row in model.YearRows)
            {
                decimal ssTotal = row.SSYou + row.SSPartner;
                decimal otherIncome = 0m; // No other income in this scenario
                decimal expectedTaxableSS = CalendarSpendingModel.EstimateTaxableSocialSecurity(ssTotal, otherIncome, true);
                decimal expectedTax = expectedTaxableSS * (model.TraditionalTaxRate / 100m);
                Assert.Equal(expectedTax, row.TaxesPaid);
                Assert.Equal(0m, row.TaxableWithdrawal);
                Assert.Equal(0m, row.TraditionalWithdrawal);
                Assert.Equal(0m, row.RothWithdrawal);
                // Only check Social Security income for years when it is actually paid
                if (row.SSYou > 0 || row.SSPartner > 0)
                {
                    Assert.Equal((model.SocialSecurityMonthlyYou + model.SocialSecurityMonthlyPartner) * 12, ssTotal);
                }
            }
        }

        [Fact]
        public void Calculate_TaxesPaid_ShouldMatchIRSRule_WhenOnlySocialSecurityIncome()
        {
            // Arrange: Only Social Security income, no account balances
            var model = new CalendarSpendingModel
            {
                CurrentAgeYou = 62,
                CurrentAgePartner = 62,
                RetirementAgeYou = 62,
                RetirementAgePartner = 62,
                LifeExpectancyYou = 70,
                LifeExpectancyPartner = 70,
                TaxableBalance = 0m,
                TraditionalBalance = 0m,
                RothBalance = 0m,
                SocialSecurityMonthlyYou = 2000m,
                SocialSecurityMonthlyPartner = 1500m,
                AnnualWithdrawalOne = 42000m, // 2000+1500 * 12
                AnnualWithdrawalBoth = 42000m,
                InvestmentReturn = 0.0m,
                InflationRate = 0.0m,
                TraditionalTaxRate = 20.0m
            };

            // Act
            model.Calculate();

            // Assert: TaxesPaid should match IRS rule for all years
            Assert.NotEmpty(model.YearRows);
            foreach (var row in model.YearRows)
            {
                decimal ssTotal = row.SSYou + row.SSPartner;
                decimal otherIncome = 0m; // No other income in this scenario
                decimal expectedTaxableSS = CalendarSpendingModel.EstimateTaxableSocialSecurity(ssTotal, otherIncome, true);
                decimal expectedTax = expectedTaxableSS * (model.TraditionalTaxRate / 100m);
                Assert.Equal(expectedTax, row.TaxesPaid);
                Assert.Equal(0m, row.TaxableWithdrawal);
                Assert.Equal(0m, row.TraditionalWithdrawal);
                Assert.Equal(0m, row.RothWithdrawal);
                // Only check Social Security income for years when it is actually paid
                if (row.SSYou > 0 || row.SSPartner > 0)
                {
                    Assert.Equal((model.SocialSecurityMonthlyYou + model.SocialSecurityMonthlyPartner) * 12, ssTotal);
                }
            }
        }

        [Fact]
        public void Calculate_TaxesPaid_ShouldIncludeTaxableWithdrawalInTaxCalculation()
        {
            // Arrange: Social Security + Taxable withdrawal
            var model = new CalendarSpendingModel
            {
                CurrentAgeYou = 65,
                CurrentAgePartner = 65,
                RetirementAgeYou = 65,
                RetirementAgePartner = 65,
                LifeExpectancyYou = 70,
                LifeExpectancyPartner = 70,
                TaxableBalance = 100_000m, // ensure taxable withdrawals
                TraditionalBalance = 0m,
                RothBalance = 0m,
                SocialSecurityMonthlyYou = 2000m,
                SocialSecurityMonthlyPartner = 1500m,
                AnnualWithdrawalOne = 50000m, // withdrawal exceeds SS income
                AnnualWithdrawalBoth = 50000m,
                InvestmentReturn = 0.0m,
                InflationRate = 0.0m,
                TraditionalTaxRate = 20.0m
            };

            // Act
            model.Calculate();

            // Assert: TaxesPaid should include taxable withdrawal in IRS calculation
            Assert.NotEmpty(model.YearRows);
            foreach (var row in model.YearRows)
            {
                // Only check years with a taxable withdrawal
                if (row.TaxableWithdrawal > 0)
                {
                    decimal ssTotal = row.SSYou + row.SSPartner;
                    decimal otherIncome = row.TaxableWithdrawal; // Taxable withdrawal should be included
                    decimal expectedTaxableSS = CalendarSpendingModel.EstimateTaxableSocialSecurity(ssTotal, otherIncome, true);
                    decimal expectedTax = expectedTaxableSS * (model.TraditionalTaxRate / 100m);
                    Assert.Equal(expectedTax, row.TaxesPaid);
                }
            }
        }

        [Fact]
        public void Calculate_TaxesPaid_ShouldIncludeTaxOnTaxableGrowth()
        {
            // Arrange: Taxable account with growth
            var model = new CalendarSpendingModel
            {
                CurrentAgeYou = 65,
                CurrentAgePartner = 65,
                RetirementAgeYou = 65,
                RetirementAgePartner = 65,
                LifeExpectancyYou = 66,
                LifeExpectancyPartner = 66,
                TaxableBalance = 100_000m, // initial taxable balance
                TraditionalBalance = 0m,
                RothBalance = 0m,
                SocialSecurityMonthlyYou = 0m,
                SocialSecurityMonthlyPartner = 0m,
                AnnualWithdrawalOne = 0m,
                AnnualWithdrawalBoth = 0m,
                InvestmentReturn = 10.0m, // high growth for clear effect
                InflationRate = 0.0m,
                TraditionalTaxRate = 20.0m
            };

            // Act
            model.Calculate();

            // Assert: Growth in taxable account should be taxed
            Assert.NotEmpty(model.YearRows);
            foreach (var row in model.YearRows)
            {
                // Only check years with growth
                if (row.Growth > 0)
                {
                    // Tax on growth should be included in TaxesPaid
                    decimal expectedTaxOnGrowth = row.Growth * (model.TraditionalTaxRate / 100m);
                    // Since there are no withdrawals or SS, all taxes should be from growth
                    Assert.True(row.TaxesPaid >= expectedTaxOnGrowth);
                }
            }
        }

        [Fact]
        public void EstimateTaxableSocialSecurity_ShouldMatchIRSRule()
        {
            // Example: Married filing jointly, Social Security $30,000, other income $20,000
            decimal ssBenefits = 30000m;
            decimal otherIncome = 20000m;
            bool marriedFilingJointly = true;

            // IRS thresholds: base = 32000, max = 44000
            // Combined income = 20000 + 0.5 * 30000 = 35000
            // 35000 > 32000, but < 44000, so up to 50% of excess over base is taxable
            // Taxable = 0.5 * (35000 - 32000) = 1500

            decimal expectedTaxable = 1500m;
            decimal actualTaxable = CalendarSpendingModel.EstimateTaxableSocialSecurity(ssBenefits, otherIncome, marriedFilingJointly);
            Assert.Equal(expectedTaxable, actualTaxable);

            // Example: Combined income above max threshold
            ssBenefits = 30000m;
            otherIncome = 20000m + 20000m; // 40000 other income
            // Combined income = 40000 + 0.5 * 30000 = 55000
            // Taxable portion capped at 85% of SS
            decimal maxTaxable = 0.85m * ssBenefits;
            actualTaxable = CalendarSpendingModel.EstimateTaxableSocialSecurity(ssBenefits, otherIncome, marriedFilingJointly);
            Assert.True(actualTaxable <= maxTaxable);
        }
    }
}
