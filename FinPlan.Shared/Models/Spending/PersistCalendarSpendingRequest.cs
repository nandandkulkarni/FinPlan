namespace FinPlan.Shared.Models.Spending
{
    public class PersistCalendarSpendingRequest
    {
        public string UserGuid { get; set; } = string.Empty;
        public string CalculatorType { get; set; } = string.Empty;
        public CalendarSpendingModel Data { get; set; } = new CalendarSpendingModel();
    }
}
