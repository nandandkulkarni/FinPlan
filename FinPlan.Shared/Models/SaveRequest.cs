namespace FinPlan.Shared.Models
{
    public class SaveRequest
    {
        public string UserGuid { get; set; } = string.Empty;
        public string CalculatorType { get; set; } = string.Empty;
        public SavingsCalculatorModel Data { get; set; } = new();
    }
}