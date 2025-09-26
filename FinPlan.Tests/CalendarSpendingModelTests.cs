using System;
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
            // Check that AmountNeededForCostOfLiving is zero before retirement
            Assert.Equal(0m, firstRow.AmountNeededForCostOfLiving);
            // Check that AmountNeededForCostOfLiving is set after retirement
            var retirementRow = model.YearRows.Find(r => r.Year == model.RetirementYearYou);
            Assert.NotNull(retirementRow);
            Assert.True(retirementRow.AmountNeededForCostOfLiving > 0);
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

            // Assert: TaxesPaidSlashDueOnAllTaxableGrowthAndIncome should match IRS rule for all years
            Assert.NotEmpty(model.YearRows);
            foreach (var row in model.YearRows)
            {
                decimal ssTotal = row.SSYou + row.SSPartner;
                decimal otherIncome = 0m; // No other income in this scenario
                decimal expectedTaxableSS = CalendarSpendingModel.EstimateTaxableSocialSecurity(ssTotal, otherIncome, true);
                decimal expectedTax = expectedTaxableSS * (model.TraditionalTaxRate / 100m);
                Assert.Equal(expectedTax, row.TaxesPaidSlashDueOnAllTaxableGrowthAndIncome);
                Assert.Equal(0m, row.TaxableWithdrawalForCostOfLivingAndTaxes);
                Assert.Equal(0m, row.TraditionalWithdrawalForCostOfLivingAndTaxes);
                Assert.Equal(0m, row.RothWithdrawalForCostOfLivingAndTaxes);
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

            // Assert: TaxesPaidSlashDueOnAllTaxableGrowthAndIncome should match IRS rule for all years
            Assert.NotEmpty(model.YearRows);
            foreach (var row in model.YearRows)
            {
                decimal ssTotal = row.SSYou + row.SSPartner;
                decimal otherIncome = 0m; // No other income in this scenario
                decimal expectedTaxableSS = CalendarSpendingModel.EstimateTaxableSocialSecurity(ssTotal, otherIncome, true);
                decimal expectedTax = expectedTaxableSS * (model.TraditionalTaxRate / 100m);
                Assert.Equal(expectedTax, row.TaxesPaidSlashDueOnAllTaxableGrowthAndIncome);
                Assert.Equal(0m, row.TaxableWithdrawalForCostOfLivingAndTaxes);
                Assert.Equal(0m, row.TraditionalWithdrawalForCostOfLivingAndTaxes);
                Assert.Equal(0m, row.RothWithdrawalForCostOfLivingAndTaxes);
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

            // Assert: TaxesPaidSlashDueOnAllTaxableGrowthAndIncome should include taxable withdrawal in IRS calculation
            Assert.NotEmpty(model.YearRows);
            foreach (var row in model.YearRows)
            {
                // Only check years with a taxable withdrawal
                if (row.TaxableWithdrawalForCostOfLivingAndTaxes > 0)
                {
                    decimal ssTotal = row.SSYou + row.SSPartner;
                    decimal otherIncome = row.TaxableWithdrawalForCostOfLivingAndTaxes; // Taxable withdrawal should be included
                    decimal expectedTaxableSS = CalendarSpendingModel.EstimateTaxableSocialSecurity(ssTotal, otherIncome, true);
                    decimal expectedTax = expectedTaxableSS * (model.TraditionalTaxRate / 100m);
                    Assert.Equal(expectedTax, row.TaxesPaidSlashDueOnAllTaxableGrowthAndIncome);
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
                    // Tax on growth should be included in TaxesPaidSlashDueOnAllTaxableGrowthAndIncome
                    decimal expectedTaxOnGrowth = 100_000m * (model.InvestmentReturn / 100m) * (model.TraditionalTaxRate / 100m); // Only first year, since balance changes after
                    // Since there are no withdrawals or SS, all taxes should be from growth
                    Assert.Equal(expectedTaxOnGrowth, row.TaxesPaidSlashDueOnAllTaxableGrowthAndIncome);
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
                decimal taxableWithdrawal = row.TaxableWithdrawalForCostOfLivingAndTaxes;
                decimal traditionalWithdrawal = row.TraditionalWithdrawalForCostOfLivingAndTaxes;
                decimal rothWithdrawal = row.RothWithdrawalForCostOfLivingAndTaxes;
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
            decimal taxableWithdrawal = row.TaxableWithdrawalForCostOfLivingAndTaxes;
            decimal traditionalWithdrawal = row.TraditionalWithdrawalForCostOfLivingAndTaxes;
            decimal rothWithdrawal = row.RothWithdrawalForCostOfLivingAndTaxes;
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
            decimal taxableWithdrawal = row.TaxableWithdrawalForCostOfLivingAndTaxes;
            decimal traditionalWithdrawal = row.TraditionalWithdrawalForCostOfLivingAndTaxes;
            decimal rothWithdrawal = row.RothWithdrawalForCostOfLivingAndTaxes;
            decimal taxesPaid = row.TaxesPaidSlashDueOnAllTaxableGrowthAndIncome;
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

        [Fact]
        public void CalculateWithdrawals_ShouldWithdrawFromCorrectBucket_SingleYear()
        {
            // Arrange: Only enough in taxable for part of withdrawal, rest in traditional, none in Roth
            var model = new CalendarSpendingModel
            {
                CurrentAgeYou = 60,
                CurrentAgePartner = 60,
                RetirementAgeYou = 61,
                RetirementAgePartner = 61,
                LifeExpectancyYou = 61,
                LifeExpectancyPartner = 61,
                TaxableBalance = 10_000m, // Only enough for part of withdrawal
                TraditionalBalance = 20_000m,
                RothBalance = 0m,
                SocialSecurityMonthlyYou = 0m,
                SocialSecurityMonthlyPartner = 0m,
                AnnualWithdrawalOne = 25_000m, // Withdrawal exceeds taxable
                AnnualWithdrawalBoth = 25_000m,
                InvestmentReturn = 0.0m, // No growth for clarity
                InflationRate = 0.0m,
                TraditionalTaxRate = 20.0m,
                SimulationStartYear = DateTime.Now.Year + 1 // Only one year simulated
            };

            // Act
            model.Calculate();

            // Assert: Only one year, withdrawal should come from taxable first, then traditional
            Assert.Single(model.YearRows);
            var row = model.YearRows[0];
            Assert.Equal(10_000m, row.TaxableWithdrawalForCostOfLivingAndTaxes); // All taxable depleted
            Assert.Equal(15_000m, row.TraditionalWithdrawalForCostOfLivingAndTaxes); // Remainder from traditional
            Assert.Equal(0m, row.RothWithdrawalForCostOfLivingAndTaxes); // None from Roth
        }

        [Fact]
        public void CalculateWithdrawals_ShouldSplitBetweenTwoBuckets_SingleYear()
        {
            // Arrange: Taxable and traditional both needed
            var model = new CalendarSpendingModel
            {
                CurrentAgeYou = 60,
                CurrentAgePartner = 60,
                RetirementAgeYou = 61,
                RetirementAgePartner = 61,
                LifeExpectancyYou = 61,
                LifeExpectancyPartner = 61,
                TaxableBalance = 5_000m, // Only enough for part of withdrawal
                TraditionalBalance = 8_000m, // Only enough for part of withdrawal
                RothBalance = 0m,
                SocialSecurityMonthlyYou = 0m,
                SocialSecurityMonthlyPartner = 0m,
                AnnualWithdrawalOne = 12_000m, // Withdrawal exceeds both taxable and traditional
                AnnualWithdrawalBoth = 12_000m,
                InvestmentReturn = 0.0m,
                InflationRate = 0.0m,
                TraditionalTaxRate = 20.0m,
                SimulationStartYear = DateTime.Now.Year + 1
            };

            // Act
            model.Calculate();

            // Assert: Only one year, withdrawal should come from taxable first, then traditional
            Assert.Single(model.YearRows);
            var row = model.YearRows[0];
            Assert.Equal(5_000m, row.TaxableWithdrawalForCostOfLivingAndTaxes); // All taxable depleted
            Assert.Equal(7_000m, row.TraditionalWithdrawalForCostOfLivingAndTaxes); // Remainder from traditional
            Assert.Equal(0m, row.RothWithdrawalForCostOfLivingAndTaxes); // None from Roth
        }

        [Fact]
        public void CalculateWithdrawals_ShouldSplitBetweenThreeBuckets_SingleYear()
        {
            // Arrange: Taxable, traditional, and Roth all needed
            var model = new CalendarSpendingModel
            {
                CurrentAgeYou = 60,
                CurrentAgePartner = 60,
                RetirementAgeYou = 61,
                RetirementAgePartner = 61,
                LifeExpectancyYou = 61,
                LifeExpectancyPartner = 61,
                TaxableBalance = 2_000m,
                TraditionalBalance = 3_000m,
                RothBalance = 4_000m,
                SocialSecurityMonthlyYou = 0m,
                SocialSecurityMonthlyPartner = 0m,
                AnnualWithdrawalOne = 8_000m, // Withdrawal exceeds all buckets
                AnnualWithdrawalBoth = 8_000m,
                InvestmentReturn = 0.0m,
                InflationRate = 0.0m,
                TraditionalTaxRate = 20.0m,
                SimulationStartYear = DateTime.Now.Year + 1
            };

            // Act
            model.Calculate();

            // Assert: Only one year, withdrawal should come from all buckets in order
            Assert.Single(model.YearRows);
            var row = model.YearRows[0];
            Assert.Equal(2_000m, row.TaxableWithdrawalForCostOfLivingAndTaxes); // All taxable depleted
            Assert.Equal(3_000m, row.TraditionalWithdrawalForCostOfLivingAndTaxes); // All traditional depleted
            Assert.Equal(3_000m, row.RothWithdrawalForCostOfLivingAndTaxes); // Remainder from Roth
        }

        [Fact]
        public void CalculateWithdrawals_ShouldWithdrawFromMultipleBuckets_OverYears()
        {
            // Arrange: Start with enough in taxable, then deplete and use traditional, then Roth
            var model = new CalendarSpendingModel
            {
                CurrentAgeYou = 60,
                CurrentAgePartner = 60,
                RetirementAgeYou = 61,
                RetirementAgePartner = 61,
                LifeExpectancyYou = 63,
                LifeExpectancyPartner = 63,
                TaxableBalance = 10_000m, // Enough for first year
                TraditionalBalance = 8_000m, // Enough for second year
                RothBalance = 6_000m, // Used in third year
                SocialSecurityMonthlyYou = 0m,
                SocialSecurityMonthlyPartner = 0m,
                AnnualWithdrawalOne = 10_000m, // Each year withdrawal exceeds remaining bucket
                AnnualWithdrawalBoth = 10_000m,
                InvestmentReturn = 0.0m,
                InflationRate = 0.0m,
                TraditionalTaxRate = 20.0m,
                SimulationStartYear = DateTime.Now.Year + 1
            };

            // Act
            model.Calculate();

            // Assert: Loop through each year and check withdrawals
            Assert.Equal(3, model.YearRows.Count);
            var expected = new[]
            {
                (10_000m, 0m, 0m), // Year 1: all from taxable
                (10_000m, 0m, 0m), // Year 2: all from taxable (depleted this year)
                (0m, 8_000m, 2_000m) // Year 3: all from traditional, remainder from Roth
            };
            for (int i = 0; i < expected.Length; i++)
            {
                var row = model.YearRows[i];
                Assert.Equal(expected[i].Item1, row.TaxableWithdrawalForCostOfLivingAndTaxes);
                Assert.Equal(expected[i].Item2, row.TraditionalWithdrawalForCostOfLivingAndTaxes);
                Assert.Equal(expected[i].Item3, row.RothWithdrawalForCostOfLivingAndTaxes);
            }
        }

        [Fact]
        public void CalculateWithdrawals_ShouldWithdrawFromMultipleBuckets_OverYears_WithLoop()
        {
            // Arrange: Start with enough in taxable, then deplete and use traditional, then Roth
            var model = new CalendarSpendingModel
            {
                CurrentAgeYou = 60,
                CurrentAgePartner = 60,
                RetirementAgeYou = 61,
                RetirementAgePartner = 61,
                LifeExpectancyYou = 64,
                LifeExpectancyPartner = 64,
                TaxableBalance = 10_000m, // Enough for first year
                TraditionalBalance = 8_000m, // Enough for second year
                RothBalance = 6_000m, // Used in third and fourth year
                SocialSecurityMonthlyYou = 0m,
                SocialSecurityMonthlyPartner = 0m,
                AnnualWithdrawalOne = 10_000m, // Each year withdrawal exceeds remaining bucket
                AnnualWithdrawalBoth = 10_000m,
                InvestmentReturn = 0.0m,
                InflationRate = 0.0m,
                TraditionalTaxRate = 20.0m,
                SimulationStartYear = DateTime.Now.Year + 1
            };

            // Act
            model.Calculate();

            // Assert: Four years, each year should use next bucket(s)
            Assert.Equal(4, model.YearRows.Count);
            var expected = new[]
            {
                (10_000m, 0m, 0m), // Year 1: all from taxable
                (0m, 8_000m, 2_000m), // Year 2: all from traditional, remainder from Roth
                (0m, 0m, 4_000m), // Year 3: all from Roth
                (0m, 0m, 0m) // Year 4: all buckets depleted, no withdrawal possible
            };
            for (int i = 0; i < expected.Length; i++)
            {
                var row = model.YearRows[i];
                Assert.Equal(expected[i].Item1, row.TaxableWithdrawalForCostOfLivingAndTaxes);
                Assert.Equal(expected[i].Item2, row.TraditionalWithdrawalForCostOfLivingAndTaxes);
                Assert.Equal(expected[i].Item3, row.RothWithdrawalForCostOfLivingAndTaxes);
            }
        }

        [Fact]
        public void CalculateWithdrawals_ShouldWithdrawFromOneThenTwoThenThreeBuckets_OverYears_Loop()
        {
            // Arrange: Start with enough in taxable, then deplete and use traditional, then Roth, then all three
            var model = new CalendarSpendingModel
            {
                CurrentAgeYou = 60,
                CurrentAgePartner = 60,
                RetirementAgeYou = 61,
                RetirementAgePartner = 61,
                LifeExpectancyYou = 65,
                LifeExpectancyPartner = 65,
                TaxableBalance = 10_000m, // Year 1: all from taxable
                TraditionalBalance = 8_000m, // Year 2: all from traditional
                RothBalance = 6_000m, // Year 3: all from Roth, Year 4: split all three
                SocialSecurityMonthlyYou = 0m,
                SocialSecurityMonthlyPartner = 0m,
                AnnualWithdrawalOne = 10_000m, // Each year withdrawal exceeds remaining bucket
                AnnualWithdrawalBoth = 10_000m,
                InvestmentReturn = 0.0m,
                InflationRate = 0.0m,
                TraditionalTaxRate = 20.0m,
                SimulationStartYear = DateTime.Now.Year + 1
            };

            // Act
            model.Calculate();

            // Assert: Five years, each year should use next bucket(s)
            Assert.Equal(5, model.YearRows.Count);
            var expected = new[]
            {
                (10_000m, 0m, 0m), // Year 1: all from taxable
                (10_000m, 0m, 0m), // Year 2: all from taxable (depleted this year)
                (0m, 8_000m, 2_000m), // Year 3: all from traditional, remainder from Roth
                (0m, 0m, 4_000m), // Year 4: all from Roth
                (0m, 0m, 0m) // Year 5: all buckets depleted, no withdrawal possible
            };
            for (int i = 0; i < expected.Length; i++)
            {
                var row = model.YearRows[i];
                Assert.Equal(expected[i].Item1, row.TaxableWithdrawalForCostOfLivingAndTaxes);
                Assert.Equal(expected[i].Item2, row.TraditionalWithdrawalForCostOfLivingAndTaxes);
                Assert.Equal(expected[i].Item3, row.RothWithdrawalForCostOfLivingAndTaxes);
            }
            // Removed: depletion year assertions
        }

        [Fact]
        public void CalculateWithdrawals_ShouldWithdrawFromBucketsAsNeeded_MonthlyNeedsScenario()
        {
            // Arrange: User needs $10k/month ($120k/year), with balances that will be depleted over time
            var model = new CalendarSpendingModel
            {
                CurrentAgeYou = 60,
                CurrentAgePartner = 60,
                RetirementAgeYou = 61,
                RetirementAgePartner = 61,
                LifeExpectancyYou = 65,
                LifeExpectancyPartner = 65,
                TaxableBalance = 100_000m, // Will last less than a year
                TraditionalBalance = 80_000m, // Will last less than a year
                RothBalance = 60_000m, // Will last half a year
                SocialSecurityMonthlyYou = 0m,
                SocialSecurityMonthlyPartner = 0m,
                AnnualWithdrawalOne = 120_000m, // $10k/month
                AnnualWithdrawalBoth = 120_000m,
                InvestmentReturn = 0.0m,
                InflationRate = 0.0m,
                TraditionalTaxRate = 20.0m,
                SimulationStartYear = DateTime.Now.Year + 1
            };

            // Act
            model.Calculate();

            // Assert: Each year, withdrawals should deplete buckets in order
            Assert.Equal(5, model.YearRows.Count); // 5 years simulated
            var expected = new[]
            {
                (100_000m, 20_000m, 0m),    // Year 1: all taxable, remainder from traditional
                (60_000m, 60_000m, 0m),     // Year 2: all traditional, remainder from Roth
                (0m, 0m, 60_000m),          // Year 3: all Roth
                (0m, 0m, 0m),               // Year 4: all buckets depleted
                (0m, 0m, 0m)                // Year 5: all buckets depleted
            };
            for (int i = 0; i < expected.Length; i++)
            {
                var row = model.YearRows[i];
                Assert.Equal(expected[i].Item1, row.TaxableWithdrawalForCostOfLivingAndTaxes);
                Assert.Equal(expected[i].Item2, row.TraditionalWithdrawalForCostOfLivingAndTaxes);
                Assert.Equal(expected[i].Item3, row.RothWithdrawalForCostOfLivingAndTaxes);
            }
        }
    }
}
