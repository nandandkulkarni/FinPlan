using FinPlan.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinPlan.Shared.Models.Savings
{
    public class SavingsCalculatorModel
    {
        public void RoundDecimals(int decimals = 2)
        {
            InitialTaxableAmount = Math.Round(InitialTaxableAmount, decimals);
            InitialTraditionalAmount = Math.Round(InitialTraditionalAmount, decimals);
            InitialRothAmount = Math.Round(InitialRothAmount, decimals);
            MonthlyTaxableContribution = Math.Round(MonthlyTaxableContribution, decimals);
            MonthlyTraditionalContribution = Math.Round(MonthlyTraditionalContribution, decimals);
            MonthlyRothContribution = Math.Round(MonthlyRothContribution, decimals);
            AnnualGrowthRate = Math.Round(AnnualGrowthRate, decimals);
            AnnualTaxDeferredContribution = Math.Round(AnnualTaxDeferredContribution, decimals);
            AnnualTaxableContribution = Math.Round(AnnualTaxableContribution, decimals);
            // Add other decimal properties as needed

        }
        public bool AutoCalculate { get; set; } = false;

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
        public decimal InitialAmount
        {
            get => InitialTaxableAmount + InitialTraditionalAmount + InitialRothAmount;
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
        }

        [Required]
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

}
