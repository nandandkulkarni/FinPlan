namespace FinPlan.ApiService.Data
{
    public class SurveyResponse
    {
        // Default Id assigned when a new instance is created
        public Guid Id { get; set; } = Guid.NewGuid();

        // UserGuid should be provided by the caller (keeps as Guid type)
        public Guid UserGuid { get; set; }

        public string SurveyType { get; set; } = string.Empty;
        public string SurveyJson { get; set; } = string.Empty;

        // Default timestamps set to UTC now when created
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
