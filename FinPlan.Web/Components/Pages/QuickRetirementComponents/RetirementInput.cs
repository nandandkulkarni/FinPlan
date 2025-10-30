namespace FinPlan.Web.Components.Pages.QuickRetirementComponents;

public class RetirementInput
{
    // Basic inputs (required)
    public int CurrentAge { get; set; }
    public decimal CurrentSavings { get; set; }
    public decimal MonthlySavings { get; set; }
    public bool HasPartner { get; set; }
    
    // Refined inputs (optional - for more accuracy)
    public int? DesiredRetirementAge { get; set; }
    public decimal? ActualMonthlyIncome { get; set; }
    public decimal? ActualMonthlyExpenses { get; set; }
}
