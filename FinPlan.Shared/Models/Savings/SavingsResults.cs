namespace FinPlan.Shared.Models.Savings
{
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
}
