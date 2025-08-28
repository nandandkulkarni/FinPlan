using System.ComponentModel.DataAnnotations;

namespace FinPlan.Web.Models
{
    public class SavingsCalculatorModel
    {
        [Required]
        [Range(18, 100, ErrorMessage = "Please enter your current age (18-100)")]
        public int CurrentAge { get; set; } = 30;

        [Required]
        [Range(50, 100, ErrorMessage = "Retirement age should be between 50-100")]
        public int RetirementAge { get; set; } = 65;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Initial Post Taxamount must be positive")]
        public decimal InitialTaxableAmount { get; set; } = 500000;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Initial traditional amount must be positive")]
        public decimal InitialTraditionalAmount { get; set; } = 150000;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Initial Roth amount must be positive")]
        public decimal InitialRothAmount { get; set; } = 0;

        // Legacy property for backwards compatibility
        public decimal InitialAmount
        {
            get => InitialTaxableAmount + InitialTraditionalAmount + InitialRothAmount;
            set
            {
                decimal portion = value / 3;
                InitialTaxableAmount = portion;
                InitialTraditionalAmount = portion;
                InitialRothAmount = portion;
            }
        }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Monthly Post Taxcontribution must be positive")]
        public decimal MonthlyTaxableContribution { get; set; } = 0;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Monthly traditional contribution must be positive")]
        public decimal MonthlyTraditionalContribution { get; set; } = 3000;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Monthly Roth contribution must be positive")]
        public decimal MonthlyRothContribution { get; set; } = 0;

        // Legacy property for backwards compatibility
        public decimal MonthlyContribution
        {
            get => MonthlyTaxableContribution + MonthlyTraditionalContribution + MonthlyRothContribution;
            set
            {
                decimal portion = value / 3;
                MonthlyTaxableContribution = portion;
                MonthlyTraditionalContribution = portion;
                MonthlyRothContribution = portion;
            }
        }

        [Required]
        [Range(0, 50, ErrorMessage = "Growth rate must be between 0 and 50%")]
        public decimal AnnualGrowthRate { get; set; } = 7;

        public bool UseTaxAdvantaged { get; set; } = false;

        [Range(0, double.MaxValue, ErrorMessage = "Tax-deferred contribution must be positive")]
        public decimal AnnualTaxDeferredContribution { get; set; } = 6000;

        [Range(0, double.MaxValue, ErrorMessage = "Post Taxcontribution must be positive")]
        public decimal AnnualTaxableContribution { get; set; } = 6000;

        public IncomeType TaxableIncomeType { get; set; } = IncomeType.MixedInvestment;
        public TaxBracket TaxBracket { get; set; } = TaxBracket.Medium;

        public int Years
        {
            get
            {
                var result = Math.Max(0, RetirementAge - CurrentAge);
                return result;
            }
        }

        public int CompoundingFrequency { get; set; } = 12;

        public DateTime LastUpdateDate { get; set; }
    }

    public enum IncomeType
    {
        MixedInvestment,
        MostlyDividends,
        MostlyLongTermGains,
        MostlyInterest
    }

    public enum TaxBracket
    {
        Low,
        Medium,
        High
    }

    public class SavingsResults
    {
        public decimal FinalAmount { get; set; }
        public decimal TotalContributions { get; set; }
        public decimal TotalInterestEarned { get; set; }
        public decimal TaxDeferredBalance { get; set; }
        public decimal TaxableBalance { get; set; }
        public decimal RothBalance { get; set; }
        public decimal TaxDeferredInterestEarned { get; set; }
        public decimal TaxableInterestEarned { get; set; }
        public decimal RothInterestEarned { get; set; }
        public decimal EstimatedTaxSavings { get; set; }
        public decimal QualifiedDividendIncome { get; set; }
        public decimal NonQualifiedIncome { get; set; }
        public decimal LongTermCapitalGains { get; set; }
        public decimal ShortTermCapitalGains { get; set; }
        public decimal TotalTaxesPaid { get; set; }
        public decimal EffectiveTaxRate { get; set; }
    }

    public class YearlyBreakdown
    {
        public int Year { get; set; }
        public decimal Balance { get; set; }
        public decimal InterestEarned { get; set; }
        public decimal ContributionsThisYear { get; set; }
        public decimal TaxDeferredBalance { get; set; }
        public decimal TaxableBalance { get; set; }
        public decimal RothBalance { get; set; }
        public decimal TaxDeferredInterest { get; set; }
        public decimal TaxableInterest { get; set; }
        public decimal RothInterest { get; set; }
        public decimal TaxDeferredContribution { get; set; }
        public decimal TaxableContribution { get; set; }
        public decimal RothContribution { get; set; }
        public decimal QualifiedDividends { get; set; }
        public decimal NonQualifiedIncome { get; set; }
        public decimal LongTermGains { get; set; }
        public decimal ShortTermGains { get; set; }
        public decimal TaxesPaid { get; set; }
    }

    public class IntervalSummary
    {
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public int StartAge { get; set; }
        public int EndAge { get; set; }
        public decimal FinalBalance { get; set; }
        public decimal TotalGrowth { get; set; }
        public decimal TotalContributions { get; set; }
        public List<YearlyBreakdown> YearlyDetails { get; set; } = new();
        public string MilestoneAchieved { get; set; } = "";
    }

    public class SavingsCalculationEngine
    {
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
            decimal monthlyRate = model.AnnualGrowthRate / 100 / 12;
            int totalMonths = model.Years * 12;
            decimal monthlyTaxableContribution = model.MonthlyTaxableContribution;
            decimal monthlyTraditionalContribution = model.MonthlyTraditionalContribution;
            decimal monthlyRothContribution = model.MonthlyRothContribution;
            decimal totalTaxableContributions = taxableBalance + (monthlyTaxableContribution * totalMonths);
            decimal totalTraditionalContributions = traditionalBalance + (monthlyTraditionalContribution * totalMonths);
            decimal totalRothContributions = rothBalance + (monthlyRothContribution * totalMonths);
            decimal taxableInterest = 0;
            decimal traditionalInterest = 0;
            decimal rothInterest = 0;
            decimal totalQualifiedDividends = 0;
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
                    decimal monthlyInterest = traditionalBalance * monthlyRate;
                    yearlyTraditionalInterest += monthlyInterest;
                    traditionalBalance += monthlyInterest + monthlyTraditionalContribution;
                }
                traditionalInterest += yearlyTraditionalInterest;
                decimal yearlyRothInterest = 0;
                decimal yearlyRothContribution = monthlyRothContribution * 12;
                for (int month = 1; month <= 12; month++)
                {
                    decimal monthlyInterest = rothBalance * monthlyRate;
                    yearlyRothInterest += monthlyInterest;
                    rothBalance += monthlyInterest + monthlyRothContribution;
                }
                rothInterest += yearlyRothInterest;
                decimal yearlyTaxableInterest = 0;
                decimal yearlyTaxableContribution = monthlyTaxableContribution * 12;
                for (int month = 1; month <= 12; month++)
                {
                    decimal monthlyInterest = taxableBalance * monthlyRate;
                    yearlyTaxableInterest += monthlyInterest;
                    taxableBalance += monthlyInterest + monthlyTaxableContribution;
                }
                decimal qualifiedDividends = yearlyTaxableInterest * qualifiedPercent;
                decimal nonQualifiedIncome = yearlyTaxableInterest * nonQualifiedPercent;
                decimal longTermGains = yearlyTaxableInterest * longTermPercent;
                decimal shortTermGains = yearlyTaxableInterest * shortTermPercent;
                decimal qualifiedDividendsTax = qualifiedDividends * longTermGainsTaxRate;
                decimal nonQualifiedTax = nonQualifiedIncome * ordinaryTaxRate;
                decimal longTermGainsTax = longTermGains * longTermGainsTaxRate;
                decimal shortTermGainsTax = shortTermGains * ordinaryTaxRate;
                decimal yearlyTaxes = qualifiedDividendsTax + nonQualifiedTax + longTermGainsTax + shortTermGainsTax;
                taxableBalance -= yearlyTaxes;
                totalQualifiedDividends += qualifiedDividends;
                totalNonQualifiedIncome += nonQualifiedIncome;
                totalLongTermGains += longTermGains;
                totalShortTermGains += shortTermGains;
                totalTaxesPaid += yearlyTaxes;
                taxableInterest += yearlyTaxableInterest - yearlyTaxes;
            }
            decimal totalTaxableIncome = totalQualifiedDividends + totalNonQualifiedIncome +
                                       totalLongTermGains + totalShortTermGains;
            decimal effectiveTaxRate = (totalTaxableIncome > 0) ? (totalTaxesPaid / totalTaxableIncome) : 0;
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
                QualifiedDividendIncome = Math.Round(totalQualifiedDividends, 2),
                NonQualifiedIncome = Math.Round(totalNonQualifiedIncome, 2),
                LongTermCapitalGains = Math.Round(totalLongTermGains, 2),
                ShortTermCapitalGains = Math.Round(totalShortTermGains, 2),
                TotalTaxesPaid = Math.Round(totalTaxesPaid, 2),
                EffectiveTaxRate = Math.Round(effectiveTaxRate * 100, 2)
            };
        }

        public List<YearlyBreakdown> GetYearlyBreakdown(SavingsCalculatorModel model)
        {
            var breakdown = new List<YearlyBreakdown>();
            decimal ordinaryTaxRate = GetOrdinaryTaxRate(model.TaxBracket);
            decimal longTermGainsTaxRate = GetLongTermGainsTaxRate(model.TaxBracket);
            var (qualifiedPercent, nonQualifiedPercent, longTermPercent, shortTermPercent) =
                GetIncomeDistribution(model.TaxableIncomeType);
            decimal taxableBalance = model.InitialTaxableAmount;
            decimal traditionalBalance = model.InitialTraditionalAmount;
            decimal rothBalance = model.InitialRothAmount;
            decimal monthlyRate = model.AnnualGrowthRate / 100 / 12;
            decimal monthlyTaxableContribution = model.MonthlyTaxableContribution;
            decimal monthlyTraditionalContribution = model.MonthlyTraditionalContribution;
            decimal monthlyRothContribution = model.MonthlyRothContribution;
            for (int year = 1; year <= model.Years; year++)
            {
                decimal yearlyTraditionalInterest = 0;
                decimal yearlyTraditionalContribution = monthlyTraditionalContribution * 12;
                for (int month = 1; month <= 12; month++)
                {
                    decimal monthlyInterest = traditionalBalance * monthlyRate;
                    yearlyTraditionalInterest += monthlyInterest;
                    traditionalBalance += monthlyInterest + monthlyTraditionalContribution;
                }
                decimal yearlyRothInterest = 0;
                decimal yearlyRothContribution = monthlyRothContribution * 12;
                for (int month = 1; month <= 12; month++)
                {
                    decimal monthlyInterest = rothBalance * monthlyRate;
                    yearlyRothInterest += monthlyInterest;
                    rothBalance += monthlyInterest + monthlyRothContribution;
                }
                decimal yearlyTaxableInterest = 0;
                decimal yearlyTaxableContribution = monthlyTaxableContribution * 12;
                for (int month = 1; month <= 12; month++)
                {
                    decimal monthlyInterest = taxableBalance * monthlyRate;
                    yearlyTaxableInterest += monthlyInterest;
                    taxableBalance += monthlyInterest + monthlyTaxableContribution;
                }
                decimal qualifiedDividends = yearlyTaxableInterest * qualifiedPercent;
                decimal nonQualifiedIncome = yearlyTaxableInterest * nonQualifiedPercent;
                decimal longTermGains = yearlyTaxableInterest * longTermPercent;
                decimal shortTermGains = yearlyTaxableInterest * shortTermPercent;
                decimal qualifiedDividendsTax = qualifiedDividends * longTermGainsTaxRate;
                decimal nonQualifiedTax = nonQualifiedIncome * ordinaryTaxRate;
                decimal longTermGainsTax = longTermGains * longTermGainsTaxRate;
                decimal shortTermGainsTax = shortTermGains * ordinaryTaxRate;
                decimal yearlyTaxes = qualifiedDividendsTax + nonQualifiedTax + longTermGainsTax + shortTermGainsTax;
                taxableBalance -= yearlyTaxes;
                decimal totalBalance = taxableBalance + traditionalBalance + rothBalance;
                decimal totalYearlyInterest = yearlyTaxableInterest + yearlyTraditionalInterest + yearlyRothInterest - yearlyTaxes;
                decimal totalYearlyContributions = yearlyTaxableContribution + yearlyTraditionalContribution + yearlyRothContribution;
                breakdown.Add(new YearlyBreakdown
                {
                    Year = year,
                    Balance = Math.Round(totalBalance, 2),
                    InterestEarned = Math.Round(totalYearlyInterest, 2),
                    ContributionsThisYear = totalYearlyContributions,
                    TaxableBalance = Math.Round(taxableBalance, 2),
                    TaxDeferredBalance = Math.Round(traditionalBalance, 2),
                    RothBalance = Math.Round(rothBalance, 2),
                    TaxableInterest = Math.Round(yearlyTaxableInterest - yearlyTaxes, 2),
                    TaxDeferredInterest = Math.Round(yearlyTraditionalInterest, 2),
                    RothInterest = Math.Round(yearlyRothInterest, 2),
                    TaxableContribution = Math.Round(yearlyTaxableContribution, 2),
                    TaxDeferredContribution = Math.Round(yearlyTraditionalContribution, 2),
                    RothContribution = Math.Round(yearlyRothContribution, 2),
                    QualifiedDividends = Math.Round(qualifiedDividends, 2),
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
