namespace FinPlan.Shared.Models.Spending
{
    public class YearlySpendingBreakdown
    {
        public int Year { get; set; }
        public int Age { get; set; }

        // Starting balances by account type
        public decimal StartingTaxableBalance { get; set; }
        public decimal StartingTraditionalBalance { get; set; }
        public decimal StartingRothBalance { get; set; }
        public decimal StartingTotalBalance => StartingTaxableBalance + StartingTraditionalBalance + StartingRothBalance;

        // Withdrawals by account type
        public decimal TaxableWithdrawal { get; set; }
        public decimal TraditionalWithdrawal { get; set; }
        public decimal RothWithdrawal { get; set; }
        public decimal TotalWithdrawal => TaxableWithdrawal + TraditionalWithdrawal + RothWithdrawal;

        // Tax on traditional withdrawals
        public decimal TaxPaid { get; set; }

        // Growth by account type

        public int SocialSecurityIncomeYour { get; set; }
        public int SocialSecurityIncomePartner { get; set; }

        public int SocialSecurityIncomeJoint { get; set; }

        public decimal TaxableGrowth { get; set; }
        public decimal TraditionalGrowth { get; set; }
        public decimal RothGrowth { get; set; }
        public decimal TotalGrowth => TaxableGrowth + TraditionalGrowth + RothGrowth;


        // Ending balances by account type
        public decimal EndingTaxableBalance { get; set; }
        public decimal EndingTraditionalBalance { get; set; }
        public decimal EndingRothBalance { get; set; }
        public decimal EndingTotalBalance => EndingTaxableBalance + EndingTraditionalBalance + EndingRothBalance;

        // Legacy properties for compatibility with existing UI
        public decimal StartingBalance => StartingTotalBalance;
        public decimal Withdrawal => TotalWithdrawal;
        public decimal InvestmentGrowth => TotalGrowth;
        public decimal EndingBalance => EndingTotalBalance;
        public decimal PartTimeIncome { get; set; }
        public bool IsPartialRetirement { get; set; }

        // Helper properties
        public bool FundsRemaining => EndingTotalBalance > 0;
        public decimal WithdrawalRate => StartingTotalBalance > 0 ? TotalWithdrawal / StartingTotalBalance * 100 : 0;

        public decimal EndingSocialSecurityBalanceYour { get; internal set; }
        public decimal EndingSocialSecurityBalancePartner { get; internal set; }
        public decimal EndingSocialSecurityBalanceJoint { get; internal set; }
        public decimal EndingSocialSecurityBalanceIndividual { get; internal set; }
    }
}
