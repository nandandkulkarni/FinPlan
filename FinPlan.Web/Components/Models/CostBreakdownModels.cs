using System;
using System.Collections.Generic;

namespace FinPlan.Web.Components.Models
{
    public enum RetirementAdjustOption
    {
        Same,
        AdjustForInflation,
        Remove,
        CustomPercentage
    }

    public class CostItem
    {
        public string Category { get; set; } = string.Empty;
        public string Subcategory { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public RetirementAdjustOption AdjustOption { get; set; } = RetirementAdjustOption.Same;
    // Optional per-item inflation percent (e.g., 2.5 for 2.5%). If null, use global inflation.
    public decimal? PerItemInflationPercent { get; set; }
        // For CustomPercentage option: enter multiplier as percentage (e.g., 50 for 50%)
        public decimal CustomPercentage { get; set; } = 100m;
        public bool IncludeInRetirement { get; set; } = true;

        public decimal GetRetirementValue(int yearsToRetirement, decimal inflationRate)
        {
            if (!IncludeInRetirement) return 0m;

            // choose inflation rate: per-item if set, otherwise global
            var effectiveInflation = PerItemInflationPercent.HasValue ? PerItemInflationPercent.Value : inflationRate;

            return AdjustOption switch
            {
                RetirementAdjustOption.Same => CurrentValue,
                RetirementAdjustOption.Remove => 0m,
                RetirementAdjustOption.AdjustForInflation =>
                    CalculateInflationAdjusted(CurrentValue, yearsToRetirement, effectiveInflation),
                RetirementAdjustOption.CustomPercentage => Math.Round(CurrentValue * (CustomPercentage / 100m), 2),
                _ => CurrentValue,
            };
        }

        private static decimal CalculateInflationAdjusted(decimal current, int years, decimal inflationRate)
        {
            // inflationRate is expected as percent (e.g., 2.5 means 2.5%)
            var rate = inflationRate / 100m;
            try
            {
                var factor = Math.Pow((double)(1 + rate), years);
                return Math.Round((decimal)factor * current, 2);
            }
            catch
            {
                return current;
            }
        }
    }

    public static class StandardCostCategories
    {
        public static List<CostItem> GetDefaults()
        {
            var list = new List<CostItem>
            {
                // Housing
                new CostItem { Category = "Housing", Subcategory = "Mortgage/Rent", CurrentValue = 1500m },
                new CostItem { Category = "Housing", Subcategory = "Utilities", CurrentValue = 300m },
                new CostItem { Category = "Housing", Subcategory = "Maintenance", CurrentValue = 100m },

                // Food
                new CostItem { Category = "Food", Subcategory = "Groceries", CurrentValue = 600m },
                new CostItem { Category = "Food", Subcategory = "Dining Out", CurrentValue = 200m },

                // Transportation
                new CostItem { Category = "Transportation", Subcategory = "Car Payment", CurrentValue = 350m },
                new CostItem { Category = "Transportation", Subcategory = "Fuel", CurrentValue = 150m },
                new CostItem { Category = "Transportation", Subcategory = "Insurance", CurrentValue = 120m },

                // Healthcare
                new CostItem { Category = "Healthcare", Subcategory = "Insurance", CurrentValue = 400m },
                new CostItem { Category = "Healthcare", Subcategory = "Out-of-pocket", CurrentValue = 50m },

                // Insurance
                new CostItem { Category = "Insurance", Subcategory = "Life Insurance", CurrentValue = 50m },
                new CostItem { Category = "Insurance", Subcategory = "Disability Insurance", CurrentValue = 30m },

                // Taxes & Savings
                new CostItem { Category = "Taxes & Savings", Subcategory = "Taxes", CurrentValue = 800m },
                new CostItem { Category = "Taxes & Savings", Subcategory = "Retirement Savings (401k/IRA)", CurrentValue = 600m, IncludeInRetirement = false },

                // Lifestyle
                new CostItem { Category = "Lifestyle", Subcategory = "Entertainment", CurrentValue = 150m },
                new CostItem { Category = "Lifestyle", Subcategory = "Education", CurrentValue = 100m },
                new CostItem { Category = "Lifestyle", Subcategory = "Miscellaneous", CurrentValue = 100m }
            };

            return list;
        }
    }
}
