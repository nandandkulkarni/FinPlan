namespace FinPlan.Web.Data.Models
{
    public class SurveySaveRequest
    {
        public string UserGuid { get; set; } = string.Empty;
        public string SurveyType { get; set; } = string.Empty;
        public object SurveyJson { get; set; } = new();
    }
}
