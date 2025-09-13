namespace FinPlan.ApiService.Data
{
    // DTOs and EF Model
    public class SurveySaveRequest
    {
        public Guid UserGuid { get; set; }
        public string SurveyType { get; set; } = "";
        public object SurveyJson { get; set; } = new();
    }
}
