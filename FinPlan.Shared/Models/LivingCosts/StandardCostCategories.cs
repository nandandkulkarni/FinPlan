namespace FinPlan.Shared.Models.LivingCosts
{
    public static class StandardCostCategories
    {
        public static List<CostItem> GetDefaults()
        {
            var list = new List<CostItem>
            {
                // Housing
                new CostItem { Category = "Housing", Subcategory = "Mortgage/Rent", CurrentValue = 2500, AdjustOption = RetirementAdjustOption.Inflation },
                new CostItem { Category = "Housing", Subcategory = "Utilities", CurrentValue = 300m, AdjustOption = RetirementAdjustOption.Inflation },
                new CostItem { Category = "Housing", Subcategory = "Maintenance", CurrentValue = 5000m, Frequency= Frequency.Yearly, AdjustOption = RetirementAdjustOption.Inflation },
                new CostItem { Category = "Housing", Subcategory = "Property Tax", CurrentValue = 10000m, Frequency = Frequency.Yearly, AdjustOption = RetirementAdjustOption.Inflation },

                // Food
                new CostItem { Category = "Food", Subcategory = "Groceries", CurrentValue = 600m , AdjustOption = RetirementAdjustOption.Inflation },
                new CostItem { Category = "Food", Subcategory = "Dining Out", CurrentValue = 300m , AdjustOption = RetirementAdjustOption.Inflation },
                new CostItem { Category = "Food", Subcategory = "Take Out", CurrentValue = 200m , AdjustOption = RetirementAdjustOption.Inflation },

                // Transportation
                new CostItem { Category = "Transportation", Subcategory = "Car Payment", CurrentValue = 550m , AdjustOption = RetirementAdjustOption.Inflation },
                new CostItem { Category = "Transportation", Subcategory = "Fuel", CurrentValue = 150m , AdjustOption = RetirementAdjustOption.Inflation },
                new CostItem { Category = "Transportation", Subcategory = "Maintenance", CurrentValue = 150m , AdjustOption = RetirementAdjustOption.Inflation },
                new CostItem { Category = "Transportation", Subcategory = "Insurance", CurrentValue = 300m , AdjustOption = RetirementAdjustOption.Inflation },

                // Travel
                new CostItem { Category = "Travel", Subcategory = "Airfare", CurrentValue = 2000m , AdjustOption = RetirementAdjustOption.Inflation },
                new CostItem { Category = "Travel", Subcategory = "Accommodation", CurrentValue = 800m , AdjustOption = RetirementAdjustOption.Inflation },
                new CostItem { Category = "Travel", Subcategory = "Food", CurrentValue = 200m , AdjustOption = RetirementAdjustOption.Inflation },
                new CostItem { Category = "Travel", Subcategory = "Activities", CurrentValue = 150m , AdjustOption = RetirementAdjustOption.Inflation },


                // Healthcare
                new CostItem { Category = "Healthcare", Subcategory = "Insurance", CurrentValue = 400m , AdjustOption = RetirementAdjustOption.Inflation },
                new CostItem { Category = "Healthcare", Subcategory = "Out-of-pocket", CurrentValue = 50m , AdjustOption = RetirementAdjustOption.Inflation },

                // Insurance
                new CostItem { Category = "Insurance", Subcategory = "Term Life Insurance", CurrentValue = 50m , AdjustOption = RetirementAdjustOption.Same },
                new CostItem { Category = "Insurance", Subcategory = "Disability Insurance", CurrentValue = 30m , AdjustOption = RetirementAdjustOption.Inflation },

                // Taxes & Savings
                new CostItem { Category = "Savings & Investments", Subcategory = "Broker/Bank Savings", CurrentValue = 200m , AdjustOption = RetirementAdjustOption.Inflation },
                new CostItem { Category = "Savings & Investments", Subcategory = "Retirement Savings (401k/IRA)", CurrentValue = 600m, IncludeInRetirement = false , AdjustOption = RetirementAdjustOption.Inflation },

                // Lifestyle
                new CostItem { Category = "Lifestyle", Subcategory = "Entertainment", CurrentValue = 150m , AdjustOption = RetirementAdjustOption.Inflation },
                new CostItem { Category = "Lifestyle", Subcategory = "Education", CurrentValue = 100m , IncludeInRetirement=false},
                new CostItem { Category = "Lifestyle", Subcategory = "Miscellaneous", CurrentValue = 100m , AdjustOption = RetirementAdjustOption.Inflation },

                
                // Education
                new CostItem { Category = "Education", Subcategory = "Tuition", CurrentValue = 2000m, Frequency = Frequency.Yearly , AdjustOption = RetirementAdjustOption.Inflation },
                new CostItem { Category = "Education", Subcategory = "Housing", CurrentValue = 300m, Frequency = Frequency.Yearly , AdjustOption = RetirementAdjustOption.Inflation },
                new CostItem { Category = "Education", Subcategory = "Supplies", CurrentValue = 100m, Frequency = Frequency.Yearly }
            };

            return list;
        }

        public static List<CostItem> GetBlankDefaults()
        {
            var list = new List<CostItem>
            {
                // Housing
                new CostItem { Category = "Housing", Subcategory = "Mortgage/Rent", CurrentValue = 2500, AdjustOption = RetirementAdjustOption.Inflation },

                // Food
                new CostItem { Category = "Food", Subcategory = "Groceries", CurrentValue = 600m , AdjustOption = RetirementAdjustOption.Inflation },

                // Transportation
                new CostItem { Category = "Transportation", Subcategory = "Car Payment", CurrentValue = 550m , AdjustOption = RetirementAdjustOption.Inflation },

            
                // Healthcare
                new CostItem { Category = "Healthcare", Subcategory = "Insurance", CurrentValue = 400m , AdjustOption = RetirementAdjustOption.Inflation },

                // Insurance
                new CostItem { Category = "Insurance", Subcategory = " Life Insurance", CurrentValue = 50m , AdjustOption = RetirementAdjustOption.Same },

                // Lifestyle
                new CostItem { Category = "Lifestyle", Subcategory = "Entertainment", CurrentValue = 150m , AdjustOption = RetirementAdjustOption.Inflation },
            };

            return list;
        }
    }
}
