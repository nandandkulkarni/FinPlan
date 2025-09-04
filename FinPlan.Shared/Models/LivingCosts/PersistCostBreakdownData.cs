namespace FinPlan.Shared.Models.LivingCosts
{
    public class CostBreakdownData
    {
        public List<CostItem> Items { get; set; } = new List<CostItem>();
        public List<string> CollapsedCategories { get; set; } = new List<string>();
        public int YearsToRetirement { get; set; }
        public decimal InflationRate { get; set; }
    }
}
