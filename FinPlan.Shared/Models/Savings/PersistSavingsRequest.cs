namespace FinPlan.Shared.Models.Savings
{
    public class PersistSavingsRequest
    {
        public string UserGuid { get; set; } = string.Empty;
        public string CalculatorType { get; set; } = string.Empty;
        public SavingsCalculatorModel Data { get; set; } = new();
    }
}