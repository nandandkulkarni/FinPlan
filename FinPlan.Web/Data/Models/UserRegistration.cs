using System.ComponentModel.DataAnnotations;

namespace FinPlan.Web.Data.Models
{
    public class UserRegistration
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [MaxLength(256)]
        public string UserEmail { get; set; } = string.Empty;
        [MaxLength(256)]
        public string? CookieGuid { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
