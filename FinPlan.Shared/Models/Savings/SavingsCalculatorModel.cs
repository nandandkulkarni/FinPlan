using FinPlan.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinPlan.Shared.Models.Savings
{


    public class SavingsCalculatorModel
    {


        public bool HasRealData { get; set; } = false;

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

        /// <summary>
        /// Determines if the model is completely empty (first-time user state)
        /// </summary>
        public bool IsModelEmpty()
        {
            return CurrentAge == 0 || 
                   RetirementAge == 0 || 
                   (InitialTaxableAmount == 0 && InitialTraditionalAmount == 0 && InitialRothAmount == 0) ||
                   (MonthlyTaxableContribution == 0 && MonthlyTraditionalContribution == 0 && MonthlyRothContribution == 0);
        }

        /// <summary>
        /// Determines if the model has some data but is not complete (partial state)
        /// </summary>
        public bool IsModelPartiallyComplete()
        {
            if (IsModelEmpty()) return false;
            if (IsModelComplete()) return false;
            
            // Has some basic data but missing essential components
            bool hasBasicAges = CurrentAge > 0 && RetirementAge > 0 && CurrentAge < RetirementAge;
            bool hasAnyMoney = InitialTaxableAmount > 0 || InitialTraditionalAmount > 0 || InitialRothAmount > 0;
            bool hasAnyContributions = MonthlyTaxableContribution > 0 || MonthlyTraditionalContribution > 0 || MonthlyRothContribution > 0;
            
            // Partial if has some but not all essential elements
            return (hasBasicAges && !hasAnyMoney) || 
                   (hasBasicAges && !hasAnyContributions) ||
                   (hasAnyMoney && !hasBasicAges);
        }

        /// <summary>
        /// Determines if the model has all essential data for meaningful calculations
        /// </summary>
        public bool IsModelComplete()
        {
            // Essential requirements for a complete savings model
            bool hasValidAges = CurrentAge > 0 && 
                               RetirementAge > 0 && 
                               CurrentAge >= 18 && 
                               CurrentAge < RetirementAge && 
                               RetirementAge <= 100;
            
            bool hasInitialMoney = InitialTaxableAmount > 0 || 
                                  InitialTraditionalAmount > 0 || 
                                  InitialRothAmount > 0;
            
            bool hasContributions = MonthlyTaxableContribution > 0 || 
                                   MonthlyTraditionalContribution > 0 || 
                                   MonthlyRothContribution > 0;
            
            bool hasReasonableGrowthRates = AnnualGrowthRateTaxable >= 0 && 
                                           AnnualGrowthRateTraditional >= 0 && 
                                           AnnualGrowthRateRoth >= 0 &&
                                           AnnualGrowthRateTaxable <= 30 && 
                                           AnnualGrowthRateTraditional <= 30 && 
                                           AnnualGrowthRateRoth <= 30;
            
            return hasValidAges && (hasInitialMoney || hasContributions) && hasReasonableGrowthRates;
        }

        public bool AutoCalculate { get; set; } = false;

        [Required]
        [Range(18, 100, ErrorMessage = "Please enter your current age (18-100)")]
        public int CurrentAge { get; set; } = 0; // Changed from 30 to 0 for empty state

        [Required]
        [Range(50, 100, ErrorMessage = "Retirement age should be between 50-100")]
        public int RetirementAge { get; set; } = 0; // Changed from 65 to 0 for empty state

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Initial Post Taxamount must be positive")]
        public decimal InitialTaxableAmount { get; set; } = 0; // Changed from 500000 to 0 for empty state

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Initial traditional amount must be positive")]
        public decimal InitialTraditionalAmount { get; set; } = 0; // Changed from 150000 to 0 for empty state

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
        public decimal MonthlyTraditionalContribution { get; set; } = 0; // Changed from 3000 to 0 for empty state

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Monthly Roth contribution must be positive")]
        public decimal MonthlyRothContribution { get; set; } = 0;

        // Legacy property for backwards compatibility
        public decimal MonthlyContribution
        {
            get => MonthlyTaxableContribution + MonthlyTraditionalContribution + MonthlyRothContribution;
        }

        [Required]
        public decimal AnnualGrowthRate { get; set; } = 7; // Keep reasonable defaults for growth rates

        // New: per-account growth rates (annual %). Default to previous AnnualGrowthRate for compatibility.
        [Required]
        public decimal AnnualGrowthRateTaxable { get; set; } = 7; // Keep reasonable defaults for growth rates

        [Required]
        public decimal AnnualGrowthRateTraditional { get; set; } = 7; // Keep reasonable defaults for growth rates

        [Required]
        public decimal AnnualGrowthRateRoth { get; set; } = 7; // Keep reasonable defaults for growth rates

        public bool UseTaxAdvantaged { get; set; } = false;

        [Range(0, double.MaxValue, ErrorMessage = "Tax-deferred contribution must be positive")]
        public decimal AnnualTaxDeferredContribution { get; set; } = 6000; // Keep for backwards compatibility

        [Range(0, double.MaxValue, ErrorMessage = "Post Taxcontribution must be positive")]
        public decimal AnnualTaxableContribution { get; set; } = 6000; // Keep for backwards compatibility

        public IncomeType TaxableIncomeType { get; set; } = IncomeType.MixedInvestment; // Keep reasonable defaults
        public TaxBracket TaxBracket { get; set; } = TaxBracket.Medium; // Keep reasonable defaults

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
