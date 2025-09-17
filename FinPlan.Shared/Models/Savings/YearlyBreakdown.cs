namespace FinPlan.Shared.Models.Savings
{
    public class YearlyBreakdown
    {
        public int Year { get; set; }
        public decimal TraditionalInterest { get; set; }
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

        public decimal TotalBOYBalance => TaxableBOYBalance + TraditionalBOYBalance + RothBOYBalance;


        public decimal TotalContributions => TaxableContribution + TraditionalContribution + RothContribution;
        public decimal TotalGrowth => TaxableInterest + TraditionalInterest + RothInterest;

        public decimal TotalTaxesPaid=> TaxesPaid;
        public decimal TotalEOYBalance => TaxableEOYBalance + TraditionalEOYBalance + RothEOYBalance;

    }
}

