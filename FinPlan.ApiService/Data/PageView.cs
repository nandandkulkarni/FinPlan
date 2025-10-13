namespace FinPlan.ApiService.Data
{
    public class PageView
    {
        public Guid Id { get; set; }
        public string Page { get; set; } = string.Empty; // logical page name
        public string? Route { get; set; } // app route path
        public string? UserGuid { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Referrer { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
