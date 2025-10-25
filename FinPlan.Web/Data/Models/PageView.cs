namespace FinPlan.Web.Data.Models
{
    public class PageView
    {
        public Guid Id { get; set; }
        public string Page { get; set; } = string.Empty;
        public string? Route { get; set; }
        public string? UserGuid { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Referrer { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
