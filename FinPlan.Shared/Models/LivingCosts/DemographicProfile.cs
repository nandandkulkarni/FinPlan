using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace FinPlan.Shared.Models.LivingCosts
{
    public class DemographicProfile
    {
        public string ProfileId { get; set; } = Guid.NewGuid().ToString();
        public string CityId { get; set; } = string.Empty; // Foreign key to CityTemplate
        public string ProfileName { get; set; } = string.Empty; // e.g., "Single, 25-35", "Family, 2 kids"
        
        // Demographics
        public int AgeMin { get; set; }
        public int AgeMax { get; set; }
        public MaritalStatus MaritalStatus { get; set; }
        public int ChildrenCount { get; set; }
        
        // JSON storage for children ages
        public string? ChildrenAgesJSON { get; set; }
        
        [NotMapped]
        public List<int> ChildrenAges
        {
            get => string.IsNullOrEmpty(ChildrenAgesJSON) 
                ? new List<int>() 
                : JsonSerializer.Deserialize<List<int>>(ChildrenAgesJSON) ?? new List<int>();
            set => ChildrenAgesJSON = JsonSerializer.Serialize(value);
        }
        
        // JSON storage for sample expenses
        public string? SampleExpensesJSON { get; set; }
        
        [NotMapped]
        public List<CostItem> SampleExpenses
        {
            get => string.IsNullOrEmpty(SampleExpensesJSON) 
                ? new List<CostItem>() 
                : JsonSerializer.Deserialize<List<CostItem>>(SampleExpensesJSON) ?? new List<CostItem>();
            set => SampleExpensesJSON = JsonSerializer.Serialize(value);
        }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
