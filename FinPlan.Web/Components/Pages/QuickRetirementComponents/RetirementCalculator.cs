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
            RetirementAge = input.DesiredRetirementAge ?? DEFAULT_RETIREMENT_AGE
        };
        
        // Calculate derived values
        var multiplier = input.HasPartner ? PARTNER_INCOME_MULTIPLIER : 1.0m;
        var expenseMultiplier = input.HasPartner ? PARTNER_EXPENSE_MULTIPLIER : 1.0m;
        
        // Use actual income if provided, otherwise estimate from savings
        if (input.ActualMonthlyIncome.HasValue && input.ActualMonthlyIncome.Value > 0)
        {
            result.EstimatedMonthlyIncome = input.ActualMonthlyIncome.Value;
        }
        else
        {
            result.EstimatedMonthlyIncome = input.MonthlySavings / SAVINGS_RATE_ASSUMPTION * multiplier;
        }
        
        // Use actual expenses if provided, otherwise estimate from income
        if (input.ActualMonthlyExpenses.HasValue && input.ActualMonthlyExpenses.Value > 0)
        {
            result.EstimatedMonthlyExpenses = input.ActualMonthlyExpenses.Value;
        }
        else
        {
            result.EstimatedMonthlyExpenses = (result.EstimatedMonthlyIncome - input.MonthlySavings * multiplier) / expenseMultiplier * expenseMultiplier;
        }
        
        // Add healthcare costs if provided (Tier 4)
        if (input.MonthlyHealthcareCost.HasValue && input.MonthlyHealthcareCost.Value > 0)
        {
            result.EstimatedMonthlyExpenses += input.MonthlyHealthcareCost.Value;
        }
        
        // Use explicit Social Security if provided (Tier 4), otherwise estimate
        if (input.ExpectedSocialSecurity.HasValue && input.ExpectedSocialSecurity.Value > 0)
        {
            result.EstimatedSocialSecurity = input.ExpectedSocialSecurity.Value;
        }
        else
        {
            result.EstimatedSocialSecurity = GetSocialSecurityEstimate(input.CurrentAge, result.EstimatedMonthlyIncome) * (input.HasPartner ? PARTNER_SS_MULTIPLIER : 1.0m);
        }
        
        // Add other retirement income if provided (Tier 3)
        var otherMonthlyIncome = input.OtherRetirementIncome ?? 0m;
        
        var yearsUntilRetirement = result.RetirementAge - input.CurrentAge;
        var monthsUntilRetirement = yearsUntilRetirement * 12;
        
        // Calculate effective monthly savings including 401k match (Tier 3)
        var effectiveMonthlySavings = input.MonthlySavings;
        if (input.Employer401kMatchPercent.HasValue && input.Employer401kMatchPercent.Value > 0)
        {
            // Assume employer matches up to the savings amount
            var matchAmount = input.MonthlySavings * (input.Employer401kMatchPercent.Value / 100m);
            effectiveMonthlySavings += matchAmount;
        }
        
        // Determine return rates based on risk tolerance (Tier 4)
        double conservativeReturn = 0.05;
        double mostLikelyReturn = 0.07;
        double optimisticReturn = 0.09;
        
        if (input.RiskTolerance.HasValue)
        {
            switch (input.RiskTolerance.Value)
            {
                case QuickRetirementComponents.RiskTolerance.Conservative:
                    conservativeReturn = 0.04;
                    mostLikelyReturn = 0.05;
                    optimisticReturn = 0.06;
                    break;
                case QuickRetirementComponents.RiskTolerance.Moderate:
                    conservativeReturn = 0.05;
                    mostLikelyReturn = 0.07;
                    optimisticReturn = 0.09;
                    break;
                case QuickRetirementComponents.RiskTolerance.Aggressive:
                    conservativeReturn = 0.06;
                    mostLikelyReturn = 0.09;
                    optimisticReturn = 0.12;
                    break;
            }
        }
        
        // Conservative scenario (lower return, higher inflation, 90% savings)
        result.ConservativeSavings = CalculateFutureValue(
            input.CurrentSavings * multiplier,
            effectiveMonthlySavings * multiplier * 0.9m,
            conservativeReturn,
            yearsUntilRetirement
        );
        
        // Most likely scenario (median return, median inflation, 100% savings)
        result.MostLikelySavings = CalculateFutureValue(
            input.CurrentSavings * multiplier,
            effectiveMonthlySavings * multiplier,
            mostLikelyReturn,
            yearsUntilRetirement
        );
        
        // Optimistic scenario (higher return, lower inflation, 110% savings)
        result.OptimisticSavings = CalculateFutureValue(
            input.CurrentSavings * multiplier,
            effectiveMonthlySavings * multiplier * 1.1m,
            optimisticReturn,
            yearsUntilRetirement
        );
        
        // Calculate retirement expenses (adjusted for inflation)
        var currentAnnualExpenses = result.EstimatedMonthlyExpenses * 12;
        var conservativeRetirementExpenses = currentAnnualExpenses * (decimal)Math.Pow(1.04, yearsUntilRetirement) * RETIREMENT_EXPENSE_RATIO;
        var mostLikelyRetirementExpenses = currentAnnualExpenses * (decimal)Math.Pow(1.03, yearsUntilRetirement) * RETIREMENT_EXPENSE_RATIO;
        var optimisticRetirementExpenses = currentAnnualExpenses * (decimal)Math.Pow(1.02, yearsUntilRetirement) * RETIREMENT_EXPENSE_RATIO;
        
        // Calculate monthly income from savings (including other retirement income from Tier 3)
        result.ConservativeMonthlyIncome = CalculateMonthlyIncome(result.ConservativeSavings, result.EstimatedSocialSecurity + otherMonthlyIncome, conservativeRetirementExpenses / 12);
        result.MostLikelyMonthlyIncome = CalculateMonthlyIncome(result.MostLikelySavings, result.EstimatedSocialSecurity + otherMonthlyIncome, mostLikelyRetirementExpenses / 12);
        result.OptimisticMonthlyIncome = CalculateMonthlyIncome(result.OptimisticSavings, result.EstimatedSocialSecurity + otherMonthlyIncome, optimisticRetirementExpenses / 12);
        
        // Calculate how long money lasts (including other retirement income)
        var totalAnnualOtherIncome = (result.EstimatedSocialSecurity + otherMonthlyIncome) * 12;
        
        result.ConservativeMoneyLastsUntil = CalculateMoneyLastsUntil(
            result.ConservativeSavings,
            conservativeRetirementExpenses,
            totalAnnualOtherIncome,
            DEFAULT_RETIREMENT_AGE,
            0.04,
            conservativeReturn
        );
        
        result.MostLikelyMoneyLastsUntil = CalculateMoneyLastsUntil(
            result.MostLikelySavings,
            mostLikelyRetirementExpenses,
            totalAnnualOtherIncome,
            DEFAULT_RETIREMENT_AGE,
            0.03,
            mostLikelyReturn
        );
        
        result.OptimisticMoneyLastsUntil = CalculateMoneyLastsUntil(
            result.OptimisticSavings,
            optimisticRetirementExpenses,
            totalAnnualOtherIncome,
            DEFAULT_RETIREMENT_AGE,
            0.02,
            optimisticReturn
        );
        
        // Calculate minimum retirement needs
        var minimumNeeded = CalculateMinimumRetirementSavings(
            mostLikelyRetirementExpenses,
            totalAnnualOtherIncome,
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
        
        // Calculate confidence level based on data quality
        result.ConfidenceLevel = CalculateConfidence(input);
        
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
    
    private decimal GetSocialSecurityEstimate(int currentAge, decimal monthlyIncome)
    {
        // Base estimate on age
        decimal baseEstimate;
        if (currentAge < 40)
            baseEstimate = 1500m; // Conservative for younger workers
        else if (currentAge < 50)
            baseEstimate = 1700m;
        else if (currentAge < 60)
            baseEstimate = 1800m;
        else
            baseEstimate = 1900m;
        
        // Adjust based on income (SS is progressive but income-based)
        // Average earner (~$4K/month) gets base estimate
        // Higher earners get more, but capped
        var incomeAdjustment = 1.0m;
        if (monthlyIncome > 6000)
            incomeAdjustment = 1.3m; // Higher earner, but SS caps benefits
        else if (monthlyIncome > 4000)
            incomeAdjustment = 1.15m;
        else if (monthlyIncome < 2500)
            incomeAdjustment = 0.75m; // Lower earner
        
        return baseEstimate * incomeAdjustment;
    }
    
    private int CalculateConfidence(RetirementInput input)
    {
        // Tier 1: Basic (4 questions) → 40% Confidence
        // Always have these: CurrentAge, CurrentSavings, MonthlySavings, HasPartner
        var tier = 1;
        var confidence = 40;
        
        // Tier 2: Refined (+ 3 questions) → 80% Confidence
        if (input.DesiredRetirementAge.HasValue &&
            input.ActualMonthlyIncome.HasValue && input.ActualMonthlyIncome.Value > 0 &&
            input.ActualMonthlyExpenses.HasValue && input.ActualMonthlyExpenses.Value > 0)
        {
            tier = 2;
            confidence = 80;
        }
        
        // Tier 3: Advanced (+ 3 questions) → 95% Confidence
        if (tier >= 2 &&
            input.Employer401kMatchPercent.HasValue &&
            input.PreTaxSavingsPercentage.HasValue &&
            input.OtherRetirementIncome.HasValue)
        {
            tier = 3;
            confidence = 95;
        }
        
        // Tier 4: Expert (+ 3 questions) → 99% Confidence
        if (tier >= 3 &&
            input.ExpectedSocialSecurity.HasValue &&
            input.RiskTolerance.HasValue &&
            input.MonthlyHealthcareCost.HasValue)
        {
            tier = 4;
            confidence = 99;
        }
        
        return confidence;
    }
}
