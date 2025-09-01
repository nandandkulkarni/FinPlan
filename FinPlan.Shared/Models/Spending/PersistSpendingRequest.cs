namespace FinPlan.Shared.Models.Spending
{
    public class PersistSpendingRequest
    {
        public string UserGuid { get; set; } = string.Empty;
        public string CalculatorType { get; set; } = string.Empty;
        public SpendingPlanModel Data { get; set; } = new SpendingPlanModel();
    }
}
