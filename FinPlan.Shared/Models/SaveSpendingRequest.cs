namespace FinPlan.Shared.Models
{
    public class SaveSpendingRequest
    {
        public string UserGuid { get; set; } = string.Empty;
        public string CalculatorType { get; set; } = string.Empty;
        public SpendingPlanModel Data { get; set; } = new SpendingPlanModel();
    }
}
