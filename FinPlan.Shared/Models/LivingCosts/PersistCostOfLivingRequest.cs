using System.Collections.Generic;

namespace FinPlan.Shared.Models.LivingCosts
{
    public class PersistCostOfLivingRequest
    {
        public string UserGuid { get; set; } = string.Empty;
        public string CalculatorType { get; set; } = string.Empty;
        public CostOfLivingData Data { get; set; } = new CostOfLivingData();
    }
}
