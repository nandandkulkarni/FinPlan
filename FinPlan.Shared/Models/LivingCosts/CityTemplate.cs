using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinPlan.Shared.Models.LivingCosts
{
    public class CityTemplate
    {
        public string CityId { get; set; } = string.Empty; // e.g., "NYC-USA", "LON-UK"
        public string CityName { get; set; } = string.Empty; // e.g., "New York City"
        public string Country { get; set; } = string.Empty; // e.g., "United States"
        public string Currency { get; set; } = "USD"; // e.g., "USD", "GBP", "EUR"
        public decimal CostOfLivingIndex { get; set; } = 100m; // Base 100 (NYC reference)
        
        // Navigation property - EF Core will handle this as a separate table
        [NotMapped]
        public List<DemographicProfile> Profiles { get; set; } = new();
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = "system";
    }
}
