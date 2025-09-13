namespace FinPlan.ApiService.Data
{
    public class SurveyResponse
    {
        public Guid Id { get; set; }
        public Guid UserGuid { get; set; }
        public string SurveyType { get; set; } = "";
        public string SurveyJson { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
