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
        
        // JSON storage for children ages (database only)
        public string? ChildrenAgesJSON { get; set; }
        
        // Working property - not mapped to database
        [NotMapped]
        public List<int> ChildrenAges { get; set; } = new List<int>();
        
        // JSON storage for sample expenses (database only)
        public string? SampleExpensesJSON { get; set; }
        
        // Working property - not mapped to database
        [NotMapped]
        public List<CostItem> SampleExpenses { get; set; } = new List<CostItem>();
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Method to serialize lists to JSON before saving to database
        public void SerializeForDatabase()
        {
            ChildrenAgesJSON = ChildrenAges?.Count > 0 
                ? JsonSerializer.Serialize(ChildrenAges) 
                : null;
                
            SampleExpensesJSON = SampleExpenses?.Count > 0 
                ? JsonSerializer.Serialize(SampleExpenses) 
                : null;
        }

        // Method to deserialize JSON to lists after loading from database
        public void DeserializeFromDatabase()
        {
            ChildrenAges = string.IsNullOrEmpty(ChildrenAgesJSON) 
                ? new List<int>() 
                : JsonSerializer.Deserialize<List<int>>(ChildrenAgesJSON) ?? new List<int>();
                
            SampleExpenses = string.IsNullOrEmpty(SampleExpensesJSON) 
                ? new List<CostItem>() 
                : JsonSerializer.Deserialize<List<CostItem>>(SampleExpensesJSON) ?? new List<CostItem>();
        }
    }
}
