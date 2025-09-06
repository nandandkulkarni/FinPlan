namespace FinPlan.Shared.Models.LivingCosts
{
    public class CostOfLivingData
    {
        public List<CostItem> Items { get; set; } = new List<CostItem>();
        public List<string> CollapsedCategories { get; set; } = new List<string>();
        public int YearsToRetirement { get; set; }
        public decimal InflationRate { get; set; }
        // Header for this saved tab (separate from internal tab id/name)
        public string Header { get; set; } = string.Empty;
    }
}
