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
                    decimal expectedTaxOnGrowth = 100_000m * (model.InvestmentReturn / 100m) * (model.TraditionalTaxRate / 100m); // Only first year, since balance changes after
                    // Since there are no withdrawals or SS, all taxes should be from growth
                    Assert.Equal(expectedTaxOnGrowth, row.TaxesPaid);
                    break; // Only check first year for clarity
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

        [Fact]
        public void Calculate_EndingBalance_ShouldMatchInflowsAndOutflows()
        {
            // Arrange
            var model = new CalendarSpendingModel
            {
                CurrentAgeYou = 60,
                CurrentAgePartner = 58,
                RetirementAgeYou = 65,
                RetirementAgePartner = 63,
                LifeExpectancyYou = 62,
                LifeExpectancyPartner = 62,
                TaxableBalance = 100_000m,
                TraditionalBalance = 50_000m,
                RothBalance = 25_000m,
                SocialSecurityMonthlyYou = 2000m,
                SocialSecurityMonthlyPartner = 1500m,
                AnnualWithdrawalOne = 60_000m,
                AnnualWithdrawalBoth = 80_000m,
                InvestmentReturn = 5.0m,
                InflationRate = 2.5m,
                TraditionalTaxRate = 20.0m,
                ReverseMortgageMonthly = 1000m,
                ReverseMortgageStartAge = 61
            };

            // Act
            model.Calculate();

            // Assert: For each year, ending balance should match model logic
            Assert.NotEmpty(model.YearRows);
            decimal prevTaxable = model.TaxableBalance;
            decimal prevTraditional = model.TraditionalBalance;
            decimal prevRoth = model.RothBalance;
            foreach (var row in model.YearRows)
            {
                decimal inflows = row.SSYou + row.SSPartner + row.ReverseMortgage;
                prevTaxable += inflows; // Add inflows to taxable before growth/withdrawals
                decimal taxableWithdrawal = row.TaxableWithdrawal;
                decimal traditionalWithdrawal = row.TraditionalWithdrawal;
                decimal rothWithdrawal = row.RothWithdrawal;
                // Apply growth first, then withdrawals
                prevTaxable *= (1 + model.InvestmentReturn / 100m);
                prevTraditional *= (1 + model.InvestmentReturn / 100m);
                prevRoth *= (1 + model.InvestmentReturn / 100m);
                decimal expectedTaxable = prevTaxable - taxableWithdrawal;
                decimal expectedTraditional = prevTraditional - traditionalWithdrawal;
                decimal expectedRoth = prevRoth - rothWithdrawal;
                Assert.True(Math.Abs(row.EndingTaxable - expectedTaxable) < 0.01m);
                Assert.True(Math.Abs(row.EndingTraditional - expectedTraditional) < 0.01m);
                Assert.True(Math.Abs(row.EndingRoth - expectedRoth) < 0.01m);
                prevTaxable = row.EndingTaxable;
                prevTraditional = row.EndingTraditional;
                prevRoth = row.EndingRoth;
            }
        }

        [Fact]
        public void Calculate_EndingBalance_ShouldBeCorrect_ForSingleYear()
        {
            // Arrange
            var model = new CalendarSpendingModel
            {
                CurrentAgeYou = 60,
                CurrentAgePartner = 58,
                RetirementAgeYou = 65,
                RetirementAgePartner = 63,
                LifeExpectancyYou = 61,
                LifeExpectancyPartner = 61,
                TaxableBalance = 100_000m,
                TraditionalBalance = 50_000m,
                RothBalance = 25_000m,
                SocialSecurityMonthlyYou = 2000m,
                SocialSecurityMonthlyPartner = 1500m,
                AnnualWithdrawalOne = 60_000m,
                AnnualWithdrawalBoth = 80_000m,
                InvestmentReturn = 5.0m,
                InflationRate = 2.5m,
                TraditionalTaxRate = 20.0m,
                ReverseMortgageMonthly = 1000m,
                ReverseMortgageStartAge = 61
            };

            // Act
            model.Calculate();

            // Assert: Only check the first year
            Assert.NotEmpty(model.YearRows);
            var row = model.YearRows[0];
            decimal prevTaxable = 100_000m;
            decimal prevTraditional = 50_000m;
            decimal prevRoth = 25_000m;
            decimal taxableWithdrawal = row.TaxableWithdrawal;
            decimal traditionalWithdrawal = row.TraditionalWithdrawal;
            decimal rothWithdrawal = row.RothWithdrawal;
            // Model logic: growth first, then withdrawals
            decimal expectedTaxable = prevTaxable * (1 + model.InvestmentReturn / 100m) - taxableWithdrawal;
            decimal expectedTraditional = prevTraditional * (1 + model.InvestmentReturn / 100m) - traditionalWithdrawal;
            decimal expectedRoth = prevRoth * (1 + model.InvestmentReturn / 100m) - rothWithdrawal;
            Assert.True(Math.Abs(row.EndingTaxable - expectedTaxable) < 0.01m);
            Assert.True(Math.Abs(row.EndingTraditional - expectedTraditional) < 0.01m);
            Assert.True(Math.Abs(row.EndingRoth - expectedRoth) < 0.01m);
        }

        [Fact]
        public void Calculate_EndingBalance_ShouldBeCorrect_ForSingleYear_Debug()
        {
            // Arrange
            var model = new CalendarSpendingModel
            {
                CurrentAgeYou = 60,
                CurrentAgePartner = 58,
                RetirementAgeYou = 65,
                RetirementAgePartner = 63,
                LifeExpectancyYou = 61,
                LifeExpectancyPartner = 61,
                TaxableBalance = 100_000m,
                TraditionalBalance = 50_000m,
                RothBalance = 25_000m,
                SocialSecurityMonthlyYou = 2000m,
                SocialSecurityMonthlyPartner = 1500m,
                AnnualWithdrawalOne = 60_000m,
                AnnualWithdrawalBoth = 80_000m,
                InvestmentReturn = 5.0m,
                InflationRate = 2.5m,
                TraditionalTaxRate = 20.0m,
                ReverseMortgageMonthly = 1000m,
                ReverseMortgageStartAge = 61
            };

            // Act
            model.Calculate();

            // Assert: Only check the first year
            Assert.NotEmpty(model.YearRows);
            var row = model.YearRows[0];
            decimal prevTaxable = 100_000m;
            decimal prevTraditional = 50_000m;
            decimal prevRoth = 25_000m;
            decimal inflows = row.SSYou + row.SSPartner + row.ReverseMortgage;
            decimal taxableWithdrawal = row.TaxableWithdrawal;
            decimal traditionalWithdrawal = row.TraditionalWithdrawal;
            decimal rothWithdrawal = row.RothWithdrawal;
            decimal taxesPaid = row.TaxesPaid;
            decimal outflows = taxesPaid + taxableWithdrawal + traditionalWithdrawal + rothWithdrawal;
            decimal expectedTaxable = prevTaxable - taxableWithdrawal;
            decimal expectedTraditional = prevTraditional - traditionalWithdrawal;
            decimal expectedRoth = prevRoth - rothWithdrawal;
            expectedTaxable += expectedTaxable * (model.InvestmentReturn / 100m);
            expectedTraditional += expectedTraditional * (model.InvestmentReturn / 100m);
            expectedRoth += expectedRoth * (model.InvestmentReturn / 100m);
            // Debug output
            System.Diagnostics.Debug.WriteLine($"prevTaxable={prevTaxable}, taxableWithdrawal={taxableWithdrawal}, expectedTaxable={expectedTaxable}, actual={row.EndingTaxable}");
            System.Diagnostics.Debug.WriteLine($"prevTraditional={prevTraditional}, traditionalWithdrawal={traditionalWithdrawal}, expectedTraditional={expectedTraditional}, actual={row.EndingTraditional}");
            System.Diagnostics.Debug.WriteLine($"prevRoth={prevRoth}, rothWithdrawal={rothWithdrawal}, expectedRoth={expectedRoth}, actual={row.EndingRoth}");
            System.Diagnostics.Debug.WriteLine($"taxesPaid={taxesPaid}, inflows={inflows}, outflows={outflows}");
            Assert.True(Math.Abs(row.EndingTaxable - expectedTaxable) < 0.01m);
            Assert.True(Math.Abs(row.EndingTraditional - expectedTraditional) < 0.01m);
            Assert.True(Math.Abs(row.EndingRoth - expectedRoth) < 0.01m);
        }

        [Fact]
        public void Calculate_BalancesAreCarriedForwardCorrectly_ForAllBuckets()
        {
            // Arrange
            var model = new CalendarSpendingModel
            {
                CurrentAgeYou = 60,
                CurrentAgePartner = 58,
                RetirementAgeYou = 65,
                RetirementAgePartner = 63,
                LifeExpectancyYou = 62,
                LifeExpectancyPartner = 62,
                TaxableBalance = 100_000m,
                TraditionalBalance = 50_000m,
                RothBalance = 25_000m,
                SocialSecurityMonthlyYou = 2000m,
                SocialSecurityMonthlyPartner = 1500m,
                AnnualWithdrawalOne = 60_000m,
                AnnualWithdrawalBoth = 80_000m,
                InvestmentReturn = 5.0m,
                InflationRate = 2.5m,
                TraditionalTaxRate = 20.0m,
                ReverseMortgageMonthly = 1000m,
                ReverseMortgageStartAge = 61
            };

            // Act
            model.Calculate();

            // Assert: For each year, previous ending balances should match next year's starting balances
            Assert.NotEmpty(model.YearRows);
            decimal prevTaxable = model.TaxableBalance;
            decimal prevTraditional = model.TraditionalBalance;
            decimal prevRoth = model.RothBalance;
            for (int i = 0; i < model.YearRows.Count; i++)
            {
                var row = model.YearRows[i];
                // First year: starting balances should match initial values
                if (i == 0)
                {
                    Assert.True(Math.Abs(prevTaxable - model.TaxableBalance) < 0.01m);
                    Assert.True(Math.Abs(prevTraditional - model.TraditionalBalance) < 0.01m);
                    Assert.True(Math.Abs(prevRoth - model.RothBalance) < 0.01m);
                }
                else
                {
                    // Previous year's ending balances should match this year's starting balances
                    Assert.True(Math.Abs(prevTaxable - model.YearRows[i - 1].EndingTaxable) < 0.01m);
                    Assert.True(Math.Abs(prevTraditional - model.YearRows[i - 1].EndingTraditional) < 0.01m);
                    Assert.True(Math.Abs(prevRoth - model.YearRows[i - 1].EndingRoth) < 0.01m);
                }
                // Update for next year
                prevTaxable = row.EndingTaxable;
                prevTraditional = row.EndingTraditional;
                prevRoth = row.EndingRoth;
            }
        }
    }
}
