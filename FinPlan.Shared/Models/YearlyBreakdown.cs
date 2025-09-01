namespace FinPlan.Shared.Models
{
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
        public decimal QualifiedDividendIncome { get; set; }
        public decimal NonQualifiedIncome { get; set; }
        public decimal LongTermGains { get; set; }
        public decimal ShortTermGains { get; set; }
        public decimal TaxesPaid { get; set; }


        // End-of-year taxable balance should include starting taxable balance plus contributions and interest, minus taxes paid.
        public decimal TaxableEOYBalance => TaxableBalance + TaxableContribution + TaxableInterest - TaxesPaid;

        public decimal TaxDeferredEOYBalance => TaxDeferredBalance + TaxableContribution + TaxDeferredInterest;

        public decimal RothEOYBalance => RothBalance + RothContribution + RothInterest;

    }
}

