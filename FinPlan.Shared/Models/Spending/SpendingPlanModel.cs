using System.ComponentModel.DataAnnotations;

namespace FinPlan.Shared.Models
{
    public class SpendingPlanModel
    {
        public string ModelMemberType { get; set; }

        public decimal SocialSecurityMonthlyAmountIndividual { get; set; } = 0; // Changed from 5000 to 0 for empty state
        
        public decimal SocialSecurityMonthlyAmountYour { get; set; } = 0; // Changed from 5000 to 0 for empty state
        public decimal SocialSecurityMonthlyAmountPartner { get; set; } = 0; // Changed from 3000 to 0 for empty state

        [Required]
        [Range(50, 100, ErrorMessage = "Retirement age should be between 50-100")]
        public int RetirementAge { get; set; } = 0; // Changed from 65 to 0 for empty state

        [Required]
        [Range(55, 120, ErrorMessage = "Life expectancy should be between 55-120")]
        public int LifeExpectancy { get; set; } = 0; // Changed from 95 to 0 for empty state

        // Individual account balances - Updated for empty state management
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Taxable balance must be positive")]
        public decimal TaxableBalance { get; set; } = 0; // Changed from 250000 to 0 for empty state

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Traditional balance must be positive")]
        public decimal TraditionalBalance { get; set; } = 0; // Changed from 500000 to 0 for empty state

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Roth balance must be positive")]
        public decimal RothBalance { get; set; } = 0; // Changed from 250000 to 0 for empty state

        // Legacy property for backward compatibility, now a computed property
        public decimal StartingBalance => TaxableBalance + TraditionalBalance + RothBalance;

        // Tax rate for traditional withdrawals - Keep reasonable default
        [Required]
        [Range(0, 50, ErrorMessage = "Tax rate must be between 0 and 50%")]
        public decimal TraditionalTaxRate { get; set; } = 22.0m; // Keep reasonable default

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Annual withdrawal must be positive")]
        public decimal AnnualWithdrawal { get; set; } = 0; // Changed from 120000 to 0 for empty state

        [Required]
        [Range(0, 20, ErrorMessage = "Inflation rate must be between 0 and 20%")]
        public decimal InflationRate { get; set; } = 2.5m; // Keep reasonable default

        [Required]
        [Range(0, 20, ErrorMessage = "Investment return must be between 0 and 20%")]
        public decimal InvestmentReturn { get; set; } = 5.0m; // Keep reasonable default

        // Partial retirement settings
        public bool HasPartialRetirement { get; set; } = false;

        [Range(0, 120, ErrorMessage = "Partial retirement end age should be between 0-120")]
        public int PartialRetirementEndAge { get; set; } = 0; // Changed from 70 to 0 for empty state

        [Range(0, double.MaxValue, ErrorMessage = "Part-time income must be positive")]
        public decimal PartialRetirementIncome { get; set; } = 0; // Changed from 25000 to 0 for empty state

        public int SocialSecurityStartAgeIndividual { get; set; } = 67; // Keep reasonable default

        // Social Security settings - Keep reasonable defaults for ages
        [Range(50, 100, ErrorMessage = "Social Security start age should be between 50-100")]
        public int SocialSecurityStartAgeYour { get; set; } = 67; // Keep reasonable default

        [Range(50, 100, ErrorMessage = "Social Security start age should be between 50-100")]
        public int SocialSecurityStartAgePartner { get; set; } = 67; // Keep reasonable default

        // For joint scenarios we allow two start ages (primary & partner)
        [Range(50, 100, ErrorMessage = "Social Security start age should be between 50-100")]
        public int SocialSecurityStartAgeJointPrimary { get; set; } = 67; // Keep reasonable default

        [Range(50, 100, ErrorMessage = "Social Security start age should be between 50-100")]
        public int SocialSecurityStartAgeJointPartner { get; set; } = 67; // Keep reasonable default

        // Estimated monthly benefits (entered by user) - Empty state
        [Range(0, double.MaxValue, ErrorMessage = "Monthly amount must be positive")]
        public decimal SocialSecurityMonthlyYour { get; set; } = 0m;

        [Range(0, double.MaxValue, ErrorMessage = "Monthly amount must be positive")]
        public decimal SocialSecurityMonthlyPartner { get; set; } = 0m;

        // Withdrawal strategies - Keep reasonable defaults for settings
        public enum WithdrawalStrategy
        {
            FixedAmount,         // Same dollar amount each year
            FixedPercentage,     // Same percentage of remaining balance
            InflationAdjusted    // Fixed amount adjusted for inflation
        }

        public WithdrawalStrategy Strategy { get; set; } = WithdrawalStrategy.InflationAdjusted; // Keep reasonable default

        // Account withdrawal priority strategies
        public enum WithdrawalPriorityStrategy
        {
            TaxOptimized,      // Prioritize based on tax efficiency (typically Taxable, then Traditional, then Roth)
            ProportionalSplit, // Withdraw proportionally from all accounts
            CustomOrder        // User-defined order
        }

        public WithdrawalPriorityStrategy PriorityStrategy { get; set; } = WithdrawalPriorityStrategy.TaxOptimized; // Keep reasonable default

        // For CustomOrder strategy
        public string[] WithdrawalOrder { get; set; } = { "Taxable", "Traditional", "Roth" }; // Keep reasonable default

        // Fixed percentage if that strategy is selected
        [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100")]
        public decimal WithdrawalPercentage { get; set; } = 4.0m; // Keep reasonable default

        // Years the plan needs to cover
        public int PlanYears => Math.Max(0, LifeExpectancy - RetirementAge);
        public DateTime LastUpdateDate { get; set; }

    }
}
