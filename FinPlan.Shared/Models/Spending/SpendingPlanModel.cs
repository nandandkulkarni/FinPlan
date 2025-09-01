using System.ComponentModel.DataAnnotations;

namespace FinPlan.Shared.Models
{
    public class SpendingPlanModel
    {
        public string ModelMemberType { get; set; }
        public decimal SocialSecurityMonthlyAmountIndividual { get; set; } = 1000;
        public decimal SocialSecurityMonthlyAmountYour { get; set; } = 5000;
        public decimal SocialSecurityMonthlyAmountPartner { get; set; } = 3000;

        [Required]
        [Range(50, 100, ErrorMessage = "Retirement age should be between 50-100")]
        public int RetirementAge { get; set; } = 65;

        [Required]
        [Range(55, 120, ErrorMessage = "Life expectancy should be between 55-120")]
        public int LifeExpectancy { get; set; } = 95;

        // Individual account balances
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Taxable balance must be positive")]
        public decimal TaxableBalance { get; set; } = 250000;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Traditional balance must be positive")]
        public decimal TraditionalBalance { get; set; } = 500000;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Roth balance must be positive")]
        public decimal RothBalance { get; set; } = 250000;

        // Legacy property for backward compatibility, now a computed property
        public decimal StartingBalance => TaxableBalance + TraditionalBalance + RothBalance;

        // Tax rate for traditional withdrawals
        [Required]
        [Range(0, 50, ErrorMessage = "Tax rate must be between 0 and 50%")]
        public decimal TraditionalTaxRate { get; set; } = 22.0m;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Annual withdrawal must be positive")]
        public decimal AnnualWithdrawal { get; set; } = 120000;

        [Required]
        [Range(0, 20, ErrorMessage = "Inflation rate must be between 0 and 20%")]
        public decimal InflationRate { get; set; } = 2.5m;

        [Required]
        [Range(0, 20, ErrorMessage = "Investment return must be between 0 and 20%")]
        public decimal InvestmentReturn { get; set; } = 5.0m;

        // Partial retirement settings
        public bool HasPartialRetirement { get; set; } = false;

        [Range(0, 120, ErrorMessage = "Partial retirement end age should be between 0-120")]
        public int PartialRetirementEndAge { get; set; } = 70; // Default 5 years of partial retirement

        [Range(0, double.MaxValue, ErrorMessage = "Part-time income must be positive")]
        public decimal PartialRetirementIncome { get; set; } = 25000; // Default $25,000/year

        public int SocialSecurityStartAgeIndividual { get; set; } = 65;

        // Social Security settings
        [Range(50, 100, ErrorMessage = "Social Security start age should be between 50-100")]
        public int SocialSecurityStartAgeYour { get; set; } = 67;

        [Range(50, 100, ErrorMessage = "Social Security start age should be between 50-100")]
        public int SocialSecurityStartAgePartner { get; set; } = 67;

        // For joint scenarios we allow two start ages (primary & partner)
        [Range(50, 100, ErrorMessage = "Social Security start age should be between 50-100")]
        public int SocialSecurityStartAgeJointPrimary { get; set; } = 67;

        [Range(50, 100, ErrorMessage = "Social Security start age should be between 50-100")]
        public int SocialSecurityStartAgeJointPartner { get; set; } = 67;

        // Estimated monthly benefits (entered by user)
        [Range(0, double.MaxValue, ErrorMessage = "Monthly amount must be positive")]
        public decimal SocialSecurityMonthlyYour { get; set; } = 0m;

        [Range(0, double.MaxValue, ErrorMessage = "Monthly amount must be positive")]
        public decimal SocialSecurityMonthlyPartner { get; set; } = 0m;

        // Withdrawal strategies
        public enum WithdrawalStrategy
        {
            FixedAmount,         // Same dollar amount each year
            FixedPercentage,     // Same percentage of remaining balance
            InflationAdjusted    // Fixed amount adjusted for inflation
        }

        public WithdrawalStrategy Strategy { get; set; } = WithdrawalStrategy.InflationAdjusted;

        // Account withdrawal priority strategies
        public enum WithdrawalPriorityStrategy
        {
            TaxOptimized,      // Prioritize based on tax efficiency (typically Taxable, then Traditional, then Roth)
            ProportionalSplit, // Withdraw proportionally from all accounts
            CustomOrder        // User-defined order
        }

        public WithdrawalPriorityStrategy PriorityStrategy { get; set; } = WithdrawalPriorityStrategy.TaxOptimized;

        // For CustomOrder strategy
        public string[] WithdrawalOrder { get; set; } = { "Taxable", "Traditional", "Roth" };

        // Fixed percentage if that strategy is selected
        [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100")]
        public decimal WithdrawalPercentage { get; set; } = 4.0m;

        // Years the plan needs to cover
        public int PlanYears => Math.Max(0, LifeExpectancy - RetirementAge);
        public DateTime LastUpdateDate { get; set; }

    }
}
