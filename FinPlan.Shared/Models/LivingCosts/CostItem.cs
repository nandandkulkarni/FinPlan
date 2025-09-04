using System;
using System.Collections.Generic;

namespace FinPlan.Shared.Models.LivingCosts

{

    public class CostItem
    {
        public string Category { get; set; } = string.Empty;
        public string Subcategory { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public RetirementAdjustOption AdjustOption { get; set; } = RetirementAdjustOption.Same;
        // Optional per-item inflation percent (e.g., 2.5 for 2.5%). If null, use global inflation.
        public decimal? PerItemInflationPercent { get; set; }
        // Whether to use global inflation or the per-item percent
        public InflationSource PerItemInflationSource { get; set; } = InflationSource.UseGlobal;
        // For CustomPercentage option: enter multiplier as percentage (e.g., 50 for 50%)
        public decimal CustomPercentage { get; set; } = 100m;
        // For Manual option: specify the retirement monthly amount directly
        public decimal? ManualRetirementValue { get; set; }
        public bool IncludeInRetirement { get; set; } = true;

        public decimal GetRetirementValue(int yearsToRetirement, decimal inflationRate)
        {
            if (!IncludeInRetirement) return 0m;

            // choose inflation rate: only use per-item percent when the item is configured to use Custom
            decimal effectiveInflation;
            if (PerItemInflationSource == InflationSource.Custom && PerItemInflationPercent.HasValue)
            {
                effectiveInflation = PerItemInflationPercent.Value;
            }
            else
            {
                effectiveInflation = inflationRate;
            }

            return AdjustOption switch
            {
                RetirementAdjustOption.Same => CurrentValue,
                RetirementAdjustOption.AdjustForInflation =>
                    CalculateInflationAdjusted(CurrentValue, yearsToRetirement, effectiveInflation),
                RetirementAdjustOption.CustomPercentage => Math.Round(CurrentValue * (CustomPercentage / 100m), 2),
                RetirementAdjustOption.Manual => ManualRetirementValue.HasValue ? Math.Round(ManualRetirementValue.Value, 2) : CurrentValue,
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
}
