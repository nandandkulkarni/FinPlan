namespace FinPlan.ApiService.Data
{
    // DTOs and EF Model
    public class SurveySaveRequest
    {
        // accept UserGuid as string from clients (cookies / JS) and parse on server
        public string UserGuid { get; set; } = string.Empty;
        public string SurveyType { get; set; } = "";
        public object SurveyJson { get; set; } = new();
    }
}
