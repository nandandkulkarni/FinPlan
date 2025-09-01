using FinPlan.Shared.Models.Spending;

namespace FinPlan.Shared.Models
{
    public class SpendingIntervalSummary
    {
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public int StartAge { get; set; }
        public int EndAge { get; set; }
        public decimal FinalBalance { get; set; }
        public decimal TotalGrowth { get; set; }
        public decimal TotalWithdrawals { get; set; }
        public List<YearlySpendingBreakdown> YearlyDetails { get; set; } = new();
        public bool FundsDepletedInInterval { get; set; }
        public string StatusMessage { get; set; } = "";
    }
}
