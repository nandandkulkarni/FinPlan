namespace FinPlan.Web.Data.Models
{
    public class SurveyResponse
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserGuid { get; set; } = string.Empty;
        public string SurveyType { get; set; } = string.Empty;
        public string SurveyJson { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? IpAddress { get; set; }
    }
}
