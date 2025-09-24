using FinPlan.Shared.Enums;
using FinPlan.Shared.Models.Savings;

namespace FinPlan.Shared.Services
{
    public class SavingsCalculationEngine
    {
        private readonly object _breakdownLock = new();

        private decimal GetOrdinaryTaxRate(TaxBracket bracket)
        {
            return bracket switch
            {
                TaxBracket.Low => 0.12m,
                TaxBracket.Medium => 0.24m,
                TaxBracket.High => 0.35m,
                _ => 0.24m
            };
        }

        private decimal GetLongTermGainsTaxRate(TaxBracket bracket)
        {
            return bracket switch
            {
                TaxBracket.Low => 0.0m,
                TaxBracket.Medium => 0.15m,
                TaxBracket.High => 0.20m,
                _ => 0.15m
            };
        }

        private (decimal qualifiedDividends, decimal nonQualifiedIncome, decimal longTermGains, decimal shortTermGains)
            GetIncomeDistribution(IncomeType incomeType)
        {
            return incomeType switch
            {
                IncomeType.MixedInvestment => (0.25m, 0.25m, 0.40m, 0.10m),
                IncomeType.MostlyDividends => (0.60m, 0.20m, 0.15m, 0.05m),
                IncomeType.MostlyLongTermGains => (0.15m, 0.10m, 0.70m, 0.05m),
                IncomeType.MostlyInterest => (0.05m, 0.65m, 0.10m, 0.20m),
                _ => (0.25m, 0.25m, 0.40m, 0.10m)
            };
        }

        public SavingsResults Calculate(SavingsCalculatorModel model)
        {
            decimal ordinaryTaxRate = GetOrdinaryTaxRate(model.TaxBracket);
            decimal longTermGainsTaxRate = GetLongTermGainsTaxRate(model.TaxBracket);
            var (qualifiedPercent, nonQualifiedPercent, longTermPercent, shortTermPercent) =
                GetIncomeDistribution(model.TaxableIncomeType);
            decimal taxableBalance = model.InitialTaxableAmount;
            decimal traditionalBalance = model.InitialTraditionalAmount;
            decimal rothBalance = model.InitialRothAmount;

            // Use independent monthly rates per bucket
            decimal monthlyRateTaxable = model.AnnualGrowthRateTaxable / 100m / 12m;
            decimal monthlyRateTraditional = model.AnnualGrowthRateTraditional / 100m / 12m;
            decimal monthlyRateRoth = model.AnnualGrowthRateRoth / 100m / 12m;

            int totalMonths = model.Years * 12;
            decimal monthlyTaxableContribution = model.MonthlyTaxableContribution;
            decimal monthlyTraditionalContribution = model.MonthlyTraditionalContribution;
            decimal monthlyRothContribution = model.MonthlyRothContribution;
            decimal totalTaxableContributions = taxableBalance + monthlyTaxableContribution * totalMonths;
            decimal totalTraditionalContributions = traditionalBalance + monthlyTraditionalContribution * totalMonths;
            decimal totalRothContributions = rothBalance + monthlyRothContribution * totalMonths;
            decimal taxableInterest = 0;
            decimal traditionalInterest = 0;
            decimal rothInterest = 0;
            decimal totalQualifiedDividendIncome = 0;
            decimal totalNonQualifiedIncome = 0;
            decimal totalLongTermGains = 0;
            decimal totalShortTermGains = 0;
            decimal totalTaxesPaid = 0;
            for (int year = 1; year <= model.Years; year++)
            {
                decimal yearlyTraditionalInterest = 0;
                decimal yearlyTraditionalContribution = monthlyTraditionalContribution * 12;
                for (int month = 1; month <= 12; month++)
                {
                    decimal monthlyInterest = traditionalBalance * monthlyRateTraditional;
                    yearlyTraditionalInterest += monthlyInterest;
                    traditionalBalance += monthlyInterest + monthlyTraditionalContribution;
                }
                traditionalInterest += yearlyTraditionalInterest;
                decimal yearlyRothInterest = 0;
                decimal yearlyRothContribution = monthlyRothContribution * 12;
                for (int month = 1; month <= 12; month++)
                {
                    decimal monthlyInterest = rothBalance * monthlyRateRoth;
                    yearlyRothInterest += monthlyInterest;
                    rothBalance += monthlyInterest + monthlyRothContribution;
                }
                rothInterest += yearlyRothInterest;
                decimal yearlyTaxableInterest = 0;
                decimal yearlyTaxableContribution = monthlyTaxableContribution * 12;
                for (int month = 1; month <= 12; month++)
                {
                    decimal monthlyInterest = taxableBalance * monthlyRateTaxable;
                    yearlyTaxableInterest += monthlyInterest;
                    taxableBalance += monthlyInterest + monthlyTaxableContribution;
                }
                decimal qualifiedDividendIncome = yearlyTaxableInterest * qualifiedPercent;
                decimal nonQualifiedIncome = yearlyTaxableInterest * nonQualifiedPercent;
                decimal longTermGains = yearlyTaxableInterest * longTermPercent;
                decimal shortTermGains = yearlyTaxableInterest * shortTermPercent;
                decimal qualifiedDividendsTax = qualifiedDividendIncome * longTermGainsTaxRate;
                decimal nonQualifiedTax = nonQualifiedIncome * ordinaryTaxRate;
                decimal longTermGainsTax = longTermGains * longTermGainsTaxRate;
                decimal shortTermGainsTax = shortTermGains * ordinaryTaxRate;
                decimal yearlyTaxes = qualifiedDividendsTax + nonQualifiedTax + longTermGainsTax + shortTermGainsTax;
                taxableBalance -= yearlyTaxes;
                totalQualifiedDividendIncome += qualifiedDividendIncome;
                totalNonQualifiedIncome += nonQualifiedIncome;
                totalLongTermGains += longTermGains;
                totalShortTermGains += shortTermGains;
                totalTaxesPaid += yearlyTaxes;
                // FIXED: Don't subtract taxes from taxable interest - they're already subtracted from balance
                taxableInterest += yearlyTaxableInterest;
            }
            decimal totalTaxableIncome = totalQualifiedDividendIncome + totalNonQualifiedIncome +
                                       totalLongTermGains + totalShortTermGains;
            decimal effectiveTaxRate = totalTaxableIncome > 0 ? totalTaxesPaid / totalTaxableIncome : 0;
            decimal totalRegularTaxes = (traditionalInterest + rothInterest + totalTaxableIncome) * effectiveTaxRate;
            decimal estimatedTaxSavings = totalRegularTaxes - totalTaxesPaid;
            decimal totalFutureValue = traditionalBalance + rothBalance + taxableBalance;
            decimal totalContributions = totalTaxableContributions + totalTraditionalContributions + totalRothContributions;
            decimal totalInterestEarned = taxableInterest + traditionalInterest + rothInterest;
            return new SavingsResults
            {
                FinalAmount = Math.Round(totalFutureValue, 2),
                TotalContributions = totalContributions,
                TotalInterestEarned = Math.Round(totalInterestEarned, 2),
                TaxDeferredBalance = Math.Round(traditionalBalance, 2),
                RothBalance = Math.Round(rothBalance, 2),
                TaxableBalance = Math.Round(taxableBalance, 2),
                TaxDeferredInterestEarned = Math.Round(traditionalInterest, 2),
                RothInterestEarned = Math.Round(rothInterest, 2),
                TaxableInterestEarned = Math.Round(taxableInterest, 2),
                EstimatedTaxSavings = Math.Round(estimatedTaxSavings, 2),
                QualifiedDividendIncome = Math.Round(totalQualifiedDividendIncome, 2),
                NonQualifiedIncome = Math.Round(totalNonQualifiedIncome, 2),
                LongTermCapitalGains = Math.Round(totalLongTermGains, 2),
                ShortTermCapitalGains = Math.Round(totalShortTermGains, 2),
                TotalTaxesPaid = Math.Round(totalTaxesPaid, 2),
                EffectiveTaxRate = Math.Round(effectiveTaxRate * 100, 2)
            };
        }

        public List<YearlyBreakdown> GetYearlyBreakdown(SavingsCalculatorModel model)
        {
          //  lock (_breakdownLock)
            {
                var breakdown = new List<YearlyBreakdown>();
                decimal ordinaryTaxRate = GetOrdinaryTaxRate(model.TaxBracket);
                decimal longTermGainsTaxRate = GetLongTermGainsTaxRate(model.TaxBracket);
                var (qualifiedPercent, nonQualifiedPercent, longTermPercent, shortTermPercent) =
                    GetIncomeDistribution(model.TaxableIncomeType);

                // Use independent monthly rates for each bucket
                decimal monthlyRateTaxable = model.AnnualGrowthRateTaxable / 100m / 12m;
                decimal monthlyRateTraditional = model.AnnualGrowthRateTraditional / 100m / 12m;
                decimal monthlyRateRoth = model.AnnualGrowthRateRoth / 100m / 12m;

                decimal monthlyTaxableContribution = model.MonthlyTaxableContribution;
                decimal monthlyTraditionalContribution = model.MonthlyTraditionalContribution;
                decimal monthlyRothContribution = model.MonthlyRothContribution;

                for (int year = 1; year <= model.Years; year++)
                {
                    // Beginning-of-Year Balances
                    decimal rothBOYBalance = year == 1 ? model.InitialRothAmount : breakdown[year - 2].RothEOYBalance;
                    decimal taxableBOYBalance = year == 1 ? model.InitialTaxableAmount : breakdown[year - 2].TaxableEOYBalance;
                    decimal traditionalBOYBalance = year == 1 ? model.InitialTraditionalAmount : breakdown[year - 2].TraditionalEOYBalance;

                    // Roth
                    decimal yearlyRothInterest = 0;
                    decimal yearlyRothContribution = monthlyRothContribution * 12;
                    decimal rothEOYBalance = rothBOYBalance;
                    for (int month = 1; month <= 12; month++)
                    {
                        decimal rothMonthlyGrowth = rothEOYBalance * monthlyRateRoth;
                        yearlyRothInterest += rothMonthlyGrowth;
                        rothEOYBalance += rothMonthlyGrowth + monthlyRothContribution;
                    }

                    // Traditional (Tax-Deferred)
                    decimal yearlyTraditionalInterest = 0;
                    decimal yearlyTraditionalContribution = monthlyTraditionalContribution * 12;
                    decimal traditionalEOYBalance = traditionalBOYBalance;
                    for (int month = 1; month <= 12; month++)
                    {
                        decimal monthlyInterest = traditionalEOYBalance * monthlyRateTraditional;
                        yearlyTraditionalInterest += monthlyInterest;
                        traditionalEOYBalance += monthlyInterest + monthlyTraditionalContribution;
                    }

                    // Taxable
                    decimal yearlyTaxableInterest = 0;
                    decimal yearlyTaxableContribution = monthlyTaxableContribution * 12;
                    decimal taxableEOYBalance = taxableBOYBalance;

                    for (int month = 1; month <= 12; month++)
                    {
                        decimal monthlyInterest = taxableEOYBalance * monthlyRateTaxable;
                        yearlyTaxableInterest += monthlyInterest;
                        taxableEOYBalance += monthlyInterest + monthlyTaxableContribution;
                    }

                    // Taxes
                    decimal qualifiedDividends = yearlyTaxableInterest * qualifiedPercent;
                    decimal nonQualifiedIncome = yearlyTaxableInterest * nonQualifiedPercent;
                    decimal longTermGains = yearlyTaxableInterest * longTermPercent;
                    decimal shortTermGains = yearlyTaxableInterest * shortTermPercent;

                    decimal qualifiedDividendsTax = qualifiedDividends * longTermGainsTaxRate;
                    decimal nonQualifiedTax = nonQualifiedIncome * ordinaryTaxRate;
                    decimal longTermGainsTax = longTermGains * longTermGainsTaxRate;
                    decimal shortTermGainsTax = shortTermGains * ordinaryTaxRate;

                    decimal yearlyTaxes = qualifiedDividendsTax + nonQualifiedTax + longTermGainsTax + shortTermGainsTax;

                    taxableEOYBalance -= yearlyTaxes;

                    // Totals
                    decimal totalBalance = taxableEOYBalance + traditionalEOYBalance + rothEOYBalance;
                    decimal totalYearlyInterest = yearlyTaxableInterest + yearlyTraditionalInterest + yearlyRothInterest - yearlyTaxes;
                    decimal totalYearlyContributions = yearlyTaxableContribution + yearlyTraditionalContribution + yearlyRothContribution;

                    breakdown.Add(new YearlyBreakdown
                    {
                        Year = year,

                        TaxableBOYBalance = Math.Round(taxableBOYBalance, 2),
                        TaxableContribution = Math.Round(yearlyTaxableContribution, 2),
                        TaxableInterest = Math.Round(yearlyTaxableInterest, 2),
                        TaxableEOYBalance = Math.Round(taxableEOYBalance, 2),

                        TraditionalBOYBalance = Math.Round(traditionalBOYBalance, 2),
                        TraditionalContribution = Math.Round(yearlyTraditionalContribution, 2),
                        TraditionalInterest = Math.Round(yearlyTraditionalInterest, 2),
                        TraditionalEOYBalance = Math.Round(traditionalEOYBalance, 2),

                        RothBOYBalance = Math.Round(rothBOYBalance, 2),
                        RothContribution = Math.Round(yearlyRothContribution, 2),
                        RothInterest = Math.Round(yearlyRothInterest, 2),
                        RothEOYBalance = Math.Round(rothEOYBalance, 2),

                        QualifiedDividendIncome = Math.Round(qualifiedDividends, 2),
                        NonQualifiedIncome = Math.Round(nonQualifiedIncome, 2),
                        LongTermGains = Math.Round(longTermGains, 2),
                        ShortTermGains = Math.Round(shortTermGains, 2),
                        TaxesPaid = Math.Round(yearlyTaxes, 2)
                    });
                }

                return breakdown;
            }
        }
    }
}
