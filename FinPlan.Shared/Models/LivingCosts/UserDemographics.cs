using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace FinPlan.Shared.Models.LivingCosts
{
    public class UserDemographics
    {
        public string UserGuid { get; set; } = string.Empty;
        public int Age { get; set; }
        public MaritalStatus MaritalStatus { get; set; }
        
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
        
        public string PreferredCurrency { get; set; } = "USD";
        public string SelectedCityId { get; set; } = string.Empty; // Which city they live in
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
