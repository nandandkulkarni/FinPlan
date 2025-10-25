namespace FinPlan.Web.Data.Models
{
    public class ContactMessage
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UserGuid { get; set; }
    }
}
