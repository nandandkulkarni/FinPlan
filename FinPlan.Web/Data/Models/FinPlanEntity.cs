using System.ComponentModel.DataAnnotations;

namespace FinPlan.Web.Data.Models
{
    // Entity for finplan.FinPlan table
    public class FinPlanEntity
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string UserGuid { get; set; } = string.Empty;
        [Required]
        public string CalculatorType { get; set; } = string.Empty;
        [Required]
        public string Data { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
