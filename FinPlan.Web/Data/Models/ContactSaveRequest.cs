namespace FinPlan.Web.Data.Models
{
    public class ContactSaveRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Message { get; set; }
        public string? UserGuid { get; set; }
    }
}
