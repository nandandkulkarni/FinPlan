namespace FinPlan.Shared.Models.Savings
{
    public class YearlyBreakdown
    {
        public int Year { get; set; }
        public decimal Balance { get; set; }
        public decimal InterestEarned { get; set; }
        public decimal ContributionsThisYear { get; set; }
        public decimal TaxDeferredBalance { get; set; }
        public decimal TaxableBalance { get; set; }
        public decimal Traditionalnterest { get; set; }
        public decimal TaxableInterest { get; set; }
        public decimal RothInterest { get; set; }
        public decimal TraditionalContribution { get; set; }
        public decimal TaxableContribution { get; set; }
        public decimal RothContribution { get; set; }
        public decimal QualifiedDividendIncome { get; set; }
        public decimal NonQualifiedIncome { get; set; }
        public decimal LongTermGains { get; set; }
        public decimal ShortTermGains { get; set; }
        public decimal TaxesPaid { get; set; }


        public decimal TaxableEOYBalance;

        public decimal TraditionalEOYBalance;

        public decimal TaxableBOYBalance;

        public decimal TraditionalBOYBalance;
        public decimal RothBOYBalance { get; set; }

        public decimal RothEOYBalance { get; set; }

    }
}

