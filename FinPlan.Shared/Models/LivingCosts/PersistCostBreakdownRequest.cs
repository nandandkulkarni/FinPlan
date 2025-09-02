using System.Collections.Generic;

namespace FinPlan.Shared.Models.LivingCosts
{
    public class PersistCostBreakdownRequest
    {
        public string UserGuid { get; set; } = string.Empty;
        public string CalculatorType { get; set; } = string.Empty;
        public PersistCostBreakdownData Data { get; set; } = new PersistCostBreakdownData();
    }

    public class PersistCostBreakdownData
    {
        public List<CostItemDto> Items { get; set; } = new List<CostItemDto>();
        public List<string> CollapsedCategories { get; set; } = new List<string>();
        public int YearsToRetirement { get; set; }
        public decimal InflationRate { get; set; }
    }

    public enum RetirementAdjustOptionDto
    {
        Same,
        AdjustForInflation,
        Remove,
        CustomPercentage
    }

    public class CostItemDto
    {
        public string Category { get; set; } = string.Empty;
        public string Subcategory { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public RetirementAdjustOptionDto AdjustOption { get; set; } = RetirementAdjustOptionDto.Same;
        public decimal? PerItemInflationPercent { get; set; }
        public decimal CustomPercentage { get; set; } = 100m;
        public bool IncludeInRetirement { get; set; } = true;
    }
}
