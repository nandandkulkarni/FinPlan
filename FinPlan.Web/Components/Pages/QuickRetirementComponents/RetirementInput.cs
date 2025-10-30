namespace FinPlan.Web.Components.Pages.QuickRetirementComponents;

public class RetirementInput
{
    // ═══════════════════════════════════════════════════════════
    // TIER 1: Basic (4 questions) → 40% Confidence
    // ═══════════════════════════════════════════════════════════
    public int CurrentAge { get; set; }
    public decimal CurrentSavings { get; set; }
    public decimal MonthlySavings { get; set; }
    public bool HasPartner { get; set; }
    
    // ═══════════════════════════════════════════════════════════
    // TIER 2: Refined (+ 3 questions) → 80% Confidence
    // ═══════════════════════════════════════════════════════════
    public int? DesiredRetirementAge { get; set; }
    public decimal? ActualMonthlyIncome { get; set; }
    public decimal? ActualMonthlyExpenses { get; set; }
    
    // ═══════════════════════════════════════════════════════════
    // TIER 3: Advanced (+ 3 questions) → 95% Confidence
    // ═══════════════════════════════════════════════════════════
    public decimal? Employer401kMatchPercent { get; set; }
    public decimal? PreTaxSavingsPercentage { get; set; }
    public decimal? OtherRetirementIncome { get; set; }
    
    // ═══════════════════════════════════════════════════════════
    // TIER 4: Expert (+ 3 questions) → 99% Confidence
    // ═══════════════════════════════════════════════════════════
    public decimal? ExpectedSocialSecurity { get; set; }
    public RiskTolerance? RiskTolerance { get; set; }
    public decimal? MonthlyHealthcareCost { get; set; }
}
