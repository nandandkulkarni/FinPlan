namespace FinPlan.Shared.Models
{
    public class SpendingResults
    {
        public decimal FinalBalance { get; set; }
        public decimal TotalWithdrawals { get; set; }
        public decimal TotalGrowth { get; set; }
        public decimal TotalPartTimeIncome { get; set; }

        public decimal TotalSocialSecurityIncome { get; set; }
        public decimal TotalTaxesPaid { get; set; }
        public bool IsSustainable { get; set; }
        public int MoneyRunsOutAge { get; set; }

        // Account-specific final balances
        public decimal TaxableBalance { get; set; }
        public decimal TraditionalBalance { get; set; }
        public decimal RothBalance { get; set; }
    }
}
