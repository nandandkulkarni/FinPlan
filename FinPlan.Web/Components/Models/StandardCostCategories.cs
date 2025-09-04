namespace FinPlan.Web.Components.Models
{
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
