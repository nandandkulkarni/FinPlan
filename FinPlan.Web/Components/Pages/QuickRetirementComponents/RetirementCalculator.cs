using FinPlan.Web.Components.Pages.QuickRetirementComponents;

namespace FinPlan.Web.Components.Pages.QuickRetirementComponents;

public class RetirementCalculator
{
    private const int DEFAULT_RETIREMENT_AGE = 65;
    private const int DEFAULT_LIFE_EXPECTANCY = 95;
    private const decimal SAVINGS_RATE_ASSUMPTION = 0.15m; // 15%
    private const decimal RETIREMENT_EXPENSE_RATIO = 0.80m; // 80% of pre-retirement
    private const decimal PARTNER_INCOME_MULTIPLIER = 1.85m;
    private const decimal PARTNER_EXPENSE_MULTIPLIER = 1.5m;
    private const decimal PARTNER_SS_MULTIPLIER = 1.85m;
    
    public RetirementCalculationResult Calculate(RetirementInput input)
    {
        var result = new RetirementCalculationResult
        {
            RetirementAge = DEFAULT_RETIREMENT_AGE
        };
        
        // Calculate derived values
        var multiplier = input.HasPartner ? PARTNER_INCOME_MULTIPLIER : 1.0m;
        var expenseMultiplier = input.HasPartner ? PARTNER_EXPENSE_MULTIPLIER : 1.0m;
        
        result.EstimatedMonthlyIncome = input.MonthlySavings / SAVINGS_RATE_ASSUMPTION * multiplier;
        result.EstimatedMonthlyExpenses = (result.EstimatedMonthlyIncome - input.MonthlySavings * multiplier) / expenseMultiplier * expenseMultiplier;
        result.EstimatedSocialSecurity = GetSocialSecurityEstimate(input.CurrentAge) * (input.HasPartner ? PARTNER_SS_MULTIPLIER : 1.0m);
        
        var yearsUntilRetirement = DEFAULT_RETIREMENT_AGE - input.CurrentAge;
        var monthsUntilRetirement = yearsUntilRetirement * 12;
        
        // Conservative scenario (5% return, 4% inflation, 90% savings)
        result.ConservativeSavings = CalculateFutureValue(
            input.CurrentSavings * multiplier,
            input.MonthlySavings * multiplier * 0.9m,
            0.05,
            yearsUntilRetirement
        );
        
        // Most likely scenario (7% return, 3% inflation, 100% savings)
        result.MostLikelySavings = CalculateFutureValue(
            input.CurrentSavings * multiplier,
            input.MonthlySavings * multiplier,
            0.07,
            yearsUntilRetirement
        );
        
        // Optimistic scenario (9% return, 2% inflation, 110% savings)
        result.OptimisticSavings = CalculateFutureValue(
            input.CurrentSavings * multiplier,
            input.MonthlySavings * multiplier * 1.1m,
            0.09,
            yearsUntilRetirement
        );
        
        // Calculate retirement expenses (adjusted for inflation)
        var currentAnnualExpenses = result.EstimatedMonthlyExpenses * 12;
        var conservativeRetirementExpenses = currentAnnualExpenses * (decimal)Math.Pow(1.04, yearsUntilRetirement) * RETIREMENT_EXPENSE_RATIO;
        var mostLikelyRetirementExpenses = currentAnnualExpenses * (decimal)Math.Pow(1.03, yearsUntilRetirement) * RETIREMENT_EXPENSE_RATIO;
        var optimisticRetirementExpenses = currentAnnualExpenses * (decimal)Math.Pow(1.02, yearsUntilRetirement) * RETIREMENT_EXPENSE_RATIO;
        
        // Calculate monthly income from savings
        result.ConservativeMonthlyIncome = CalculateMonthlyIncome(result.ConservativeSavings, result.EstimatedSocialSecurity, conservativeRetirementExpenses / 12);
        result.MostLikelyMonthlyIncome = CalculateMonthlyIncome(result.MostLikelySavings, result.EstimatedSocialSecurity, mostLikelyRetirementExpenses / 12);
        result.OptimisticMonthlyIncome = CalculateMonthlyIncome(result.OptimisticSavings, result.EstimatedSocialSecurity, optimisticRetirementExpenses / 12);
        
        // Calculate how long money lasts
        result.ConservativeMoneyLastsUntil = CalculateMoneyLastsUntil(
            result.ConservativeSavings,
            conservativeRetirementExpenses,
            result.EstimatedSocialSecurity * 12,
            DEFAULT_RETIREMENT_AGE,
            0.04,
            0.05
        );
        
        result.MostLikelyMoneyLastsUntil = CalculateMoneyLastsUntil(
            result.MostLikelySavings,
            mostLikelyRetirementExpenses,
            result.EstimatedSocialSecurity * 12,
            DEFAULT_RETIREMENT_AGE,
            0.03,
            0.05
        );
        
        result.OptimisticMoneyLastsUntil = CalculateMoneyLastsUntil(
            result.OptimisticSavings,
            optimisticRetirementExpenses,
            result.EstimatedSocialSecurity * 12,
            DEFAULT_RETIREMENT_AGE,
            0.02,
            0.05
        );
        
        // Calculate minimum retirement needs
        var minimumNeeded = CalculateMinimumRetirementSavings(
            mostLikelyRetirementExpenses,
            result.EstimatedSocialSecurity * 12,
            DEFAULT_LIFE_EXPECTANCY - DEFAULT_RETIREMENT_AGE
        );
        
        result.MinSafetyCushion = result.ConservativeSavings - minimumNeeded;
        result.MaxSafetyCushion = result.OptimisticSavings - minimumNeeded;
        
        // Determine status
        if (result.MostLikelyMoneyLastsUntil >= DEFAULT_LIFE_EXPECTANCY)
        {
            result.Status = RetirementStatus.OnTrack;
        }
        else if (result.MostLikelyMoneyLastsUntil >= DEFAULT_LIFE_EXPECTANCY - 5)
        {
            result.Status = RetirementStatus.Close;
        }
        else
        {
            result.Status = RetirementStatus.NeedsWork;
        }
        
        // Calculate action items if not on track
        if (result.Status != RetirementStatus.OnTrack)
        {
            result.TargetSavings = minimumNeeded * 1.1m; // 10% cushion
            var shortfall = result.TargetSavings - result.MostLikelySavings;
            result.AdditionalMonthlySavingsNeeded = CalculateAdditionalMonthlySavings(shortfall, yearsUntilRetirement, 0.07);
            result.ProjectedMonthlyExpenses = mostLikelyRetirementExpenses / 12;
            result.ReducedMonthlyExpenses = result.ProjectedMonthlyExpenses * 0.85m; // 15% reduction
        }
        
        return result;
    }
    
    private decimal CalculateFutureValue(decimal currentSavings, decimal monthlySavings, double annualReturn, int years)
    {
        var monthlyRate = annualReturn / 12;
        var months = years * 12;
        
        // Future value of current savings
        var fvSavings = currentSavings * (decimal)Math.Pow(1 + annualReturn, years);
        
        // Future value of monthly contributions
        var fvContributions = monthlySavings * (decimal)(((Math.Pow(1 + monthlyRate, months) - 1) / monthlyRate));
        
        return fvSavings + fvContributions;
    }
    
    private decimal CalculateMonthlyIncome(decimal totalSavings, decimal socialSecurity, decimal monthlyExpenses)
    {
        // Assume 4% withdrawal rate from savings
        var withdrawalRate = 0.04m;
        var monthlyFromSavings = (totalSavings * withdrawalRate) / 12;
        return monthlyFromSavings + socialSecurity;
    }
    
    private int CalculateMoneyLastsUntil(decimal savingsBalance, decimal annualExpenses, decimal annualSocialSecurity, int startAge, double inflation, double returnRate)
    {
        var currentBalance = savingsBalance;
        var currentExpenses = annualExpenses;
        var age = startAge;
        
        while (currentBalance > 0 && age < 120)
        {
            // Net withdrawal needed (after social security)
            var netWithdrawal = Math.Max(0, currentExpenses - annualSocialSecurity);
            
            // Deduct withdrawal
            currentBalance -= netWithdrawal;
            
            if (currentBalance <= 0)
                break;
            
            // Apply investment returns to remaining balance
            currentBalance *= (decimal)(1 + returnRate);
            
            // Increase expenses with inflation
            currentExpenses *= (decimal)(1 + inflation);
            
            age++;
        }
        
        return age;
    }
    
    private decimal CalculateMinimumRetirementSavings(decimal annualExpenses, decimal annualSocialSecurity, int retirementYears)
    {
        // Simple calculation: total expenses minus social security, discounted
        var netExpenses = annualExpenses - annualSocialSecurity;
        if (netExpenses <= 0)
            return 0;
        
        // Present value of annuity with 5% return
        var discountRate = 0.05;
        var pvFactor = (1 - Math.Pow(1 + discountRate, -retirementYears)) / discountRate;
        return netExpenses * (decimal)pvFactor;
    }
    
    private decimal CalculateAdditionalMonthlySavings(decimal shortfall, int years, double returnRate)
    {
        var monthlyRate = returnRate / 12;
        var months = years * 12;
        
        // PMT formula: shortfall = PMT * ((1 + r)^n - 1) / r
        var denominator = ((Math.Pow(1 + monthlyRate, months) - 1) / monthlyRate);
        return shortfall / (decimal)denominator;
    }
    
    private decimal GetSocialSecurityEstimate(int currentAge)
    {
        // Simplified social security estimates based on age
        // Younger people may see lower benefits due to system changes
        if (currentAge < 40)
            return 1500m; // Conservative for younger workers
        else if (currentAge < 50)
            return 1700m;
        else if (currentAge < 60)
            return 1800m;
        else
            return 1900m;
    }
}
