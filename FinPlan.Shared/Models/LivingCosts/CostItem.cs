using System;
using System.Collections.Generic;

namespace FinPlan.Shared.Models.LivingCosts

{

    public enum Frequency
    {
        Monthly = 0,
        Yearly = 1,
        Quarterly = 2,
        BiWeekly = 3,
        Weekly = 4
    }

    public class CostItem
    {
        public string Category { get; set; } = string.Empty;
        public string Subcategory { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public Frequency Frequency { get; set; } = Frequency.Monthly;
        public RetirementAdjustOption AdjustOption { get; set; } = RetirementAdjustOption.Same;
        public decimal? PerItemInflationPercent { get; set; }
        public InflationSource PerItemInflationSource { get; set; } = InflationSource.UseGlobal;
        public decimal CustomPercentage { get; set; } = 100m;
        public decimal? ManualRetirementValue { get; set; }
        public bool IncludeInRetirement { get; set; } = true;

        // Convert CurrentValue (expressed in the configured Frequency) to a per-month amount
        //public decimal GetMonthlyEquivalent()
        //{
        //    var amount = CurrentValue;
        //    decimal perMonth = Frequency switch
        //    {
        //        Frequency.Monthly => amount,
        //        Frequency.Yearly => amount / 12m,
        //        Frequency.Quarterly => amount / 3m,
        //        Frequency.BiWeekly => amount * 26m / 12m,
        //        Frequency.Weekly => amount * 52m / 12m,
        //        _ => amount
        //    };
        //    return Math.Round(perMonth, 2);
        //}

        public decimal GetMonthlyEquivalent
        {
            get
            {
                var amount = CurrentValue;
                decimal perMonth = Frequency switch
                {
                    Frequency.Monthly => amount,
                    Frequency.Yearly => amount / 12m,
                    Frequency.Quarterly => amount / 3m,
                    Frequency.BiWeekly => amount * 26m / 12m,
                    Frequency.Weekly => amount * 52m / 12m,
                    _ => amount
                };
                return Math.Round(perMonth, 2);
            }
        }

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

            // Use the monthly-equivalent as the base for retirement projections
            var baseMonthly = GetMonthlyEquivalent;

            return AdjustOption switch
            {
                RetirementAdjustOption.Same => Math.Round(baseMonthly, 2),
                RetirementAdjustOption.AdjustForInflation =>
                    CalculateInflationAdjusted(baseMonthly, yearsToRetirement, effectiveInflation),
                RetirementAdjustOption.CustomPercentage => Math.Round(baseMonthly * (CustomPercentage / 100m), 2),
                RetirementAdjustOption.Manual => ManualRetirementValue.HasValue ? Math.Round(ManualRetirementValue.Value, 2) : Math.Round(baseMonthly, 2),
                _ => Math.Round(baseMonthly, 2),
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
