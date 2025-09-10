using System.ComponentModel.DataAnnotations;

namespace FinPlan.ApiService.Data
{
    // Entity for finplan.User table
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        // Unique user identifier from client (e.g., claims-based GUID)
        [Required]
        public string UserGuid { get; set; } = string.Empty;

        // Email address (may be null if not provided)
        public string? Email { get; set; }

        // First and last name separated for easier queries
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // Display name (fallback / convenience)
        public string? DisplayName { get; set; }

        // Provider (e.g., Google)
        public string? Provider { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastSignInAt { get; set; }
    }
}