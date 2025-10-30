namespace FinPlan.Web.Components.Pages.QuickRetirementComponents;

public class RetirementCalculationResult
{
    public RetirementStatus Status { get; set; }
    public int RetirementAge { get; set; }
    
    // Confidence level (0-100)
    public int ConfidenceLevel { get; set; }
    
    // Savings projections
    public decimal ConservativeSavings { get; set; }
    public decimal MostLikelySavings { get; set; }
    public decimal OptimisticSavings { get; set; }
    
    // Monthly income projections
    public decimal ConservativeMonthlyIncome { get; set; }
    public decimal MostLikelyMonthlyIncome { get; set; }
    public decimal OptimisticMonthlyIncome { get; set; }
    
    // Longevity
    public int ConservativeMoneyLastsUntil { get; set; }
    public int MostLikelyMoneyLastsUntil { get; set; }
    public int OptimisticMoneyLastsUntil { get; set; }
    
    // Safety cushion
    public decimal MinSafetyCushion { get; set; }
    public decimal MaxSafetyCushion { get; set; }
    
    // Estimated values
    public decimal EstimatedMonthlyIncome { get; set; }
    public decimal EstimatedMonthlyExpenses { get; set; }
    public decimal EstimatedSocialSecurity { get; set; }
    
    // Action items (for when not on track)
    public decimal AdditionalMonthlySavingsNeeded { get; set; }
    public decimal TargetSavings { get; set; }
    public decimal ProjectedMonthlyExpenses { get; set; }
    public decimal ReducedMonthlyExpenses { get; set; }
}
