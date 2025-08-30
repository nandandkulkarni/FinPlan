namespace FinPlan.Shared.Models
{
    public class IntervalSummary
    {
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public int StartAge { get; set; }
        public int EndAge { get; set; }
        public decimal FinalBalance { get; set; }
        public decimal TotalGrowth { get; set; }
        public decimal TotalContributions { get; set; }
        public List<YearlyBreakdown> YearlyDetails { get; set; } = new();
        public string MilestoneAchieved { get; set; } = "";
    }

}
