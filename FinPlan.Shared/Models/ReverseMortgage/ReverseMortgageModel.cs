namespace FinPlan.Shared.Models.ReverseMortgage;

public class ReverseMortgageModel
{
    public int ApplicationYear { get; set; } = System.DateTime.Now.Year;
    public int CurrentAge { get; set; } = 65;
    public int? SpouseAge { get; set; } // Added to persist spouse age
    public decimal CurrentHomeValue { get; set; } = 400000;
    public decimal HomeAppreciationRate { get; set; } = 3.5m;
    public decimal CurrentMortgageBalance { get; set; } = 50000;
    public decimal MonthlyPayment { get; set; } = 1000;
    public decimal MortgageInterestRate { get; set; } = 4.5m;
    public int LoanTermYears { get; set; } = 30;
    public int MortgageStartYear { get; set; } = System.DateTime.Now.Year - 10;
    public string PropertyType { get; set; } = "SingleFamily";
    public bool IsPrimaryResidence { get; set; } = true;
    public decimal MonthlyPrincipalPaid { get; set; } = 0;
}
