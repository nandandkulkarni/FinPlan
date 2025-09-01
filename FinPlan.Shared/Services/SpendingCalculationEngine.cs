using FinPlan.Shared.Models;
using FinPlan.Shared.Models.Spending;

namespace FinPlan.Shared.Services
{
    public class SpendingCalculationEngine
    {
        public SpendingResults Calculate(SpendingPlanModel model)
        {
            decimal totalWithdrawals = 0;
            decimal totalGrowth = 0;
            decimal totalPartTimeIncome = 0;
            decimal totalTaxesPaid = 0;

            // Track individual account balances
            decimal taxableBalance = model.TaxableBalance;
            decimal traditionalBalance = model.TraditionalBalance;
            decimal rothBalance = model.RothBalance;

            decimal currentWithdrawal = model.AnnualWithdrawal;
            bool isSustainable = true;
            int moneyRunsOutAge = 0;

            for (int year = 1; year <= model.PlanYears; year++)
            {
                int currentAge = model.RetirementAge + year - 1;

                // Add part-time income if in partial retirement phase
                if (model.HasPartialRetirement && currentAge < model.PartialRetirementEndAge)
                {
                    // Apply part-time income to taxable account (most realistic)
                    taxableBalance += model.PartialRetirementIncome;
                    totalPartTimeIncome += model.PartialRetirementIncome;
                }

                // Calculate gross withdrawal needed based on strategy
                decimal totalBalance = taxableBalance + traditionalBalance + rothBalance;
                decimal grossWithdrawalNeeded = CalculateWithdrawal(currentWithdrawal, totalBalance, model);

                // Determine withdrawals from each account based on priority strategy
                (decimal taxableWithdrawal, decimal traditionalWithdrawal, decimal rothWithdrawal) =
                    CalculateAccountWithdrawals(grossWithdrawalNeeded, taxableBalance, traditionalBalance, rothBalance, model);

                // Calculate tax on traditional withdrawals
                decimal taxOnTraditional = traditionalWithdrawal * (model.TraditionalTaxRate / 100);
                totalTaxesPaid += taxOnTraditional;

                // Net withdrawal (what the user receives)
                decimal netWithdrawal = taxableWithdrawal + traditionalWithdrawal + rothWithdrawal;

                // Update total withdrawals
                totalWithdrawals += netWithdrawal;

                // Calculate investment returns for each account type
                decimal taxableGrowth = Math.Max(0, (taxableBalance - taxableWithdrawal) * (model.InvestmentReturn / 100));
                decimal traditionalGrowth = Math.Max(0, (traditionalBalance - traditionalWithdrawal) * (model.InvestmentReturn / 100));
                decimal rothGrowth = Math.Max(0, (rothBalance - rothWithdrawal) * (model.InvestmentReturn / 100));

                decimal totalYearlyGrowth = taxableGrowth + traditionalGrowth + rothGrowth;
                totalGrowth += totalYearlyGrowth;

                // Update balances
                taxableBalance = Math.Max(0, taxableBalance - taxableWithdrawal + taxableGrowth);
                traditionalBalance = Math.Max(0, traditionalBalance - traditionalWithdrawal + traditionalGrowth);
                rothBalance = Math.Max(0, rothBalance - rothWithdrawal + rothGrowth);

                decimal totalEndingBalance = taxableBalance + traditionalBalance + rothBalance;

                // For inflation-adjusted, increase withdrawal amount
                if (model.Strategy == SpendingPlanModel.WithdrawalStrategy.InflationAdjusted)
                {
                    currentWithdrawal *= 1 + model.InflationRate / 100;
                }

                // Check if funds depleted
                if (totalEndingBalance <= 0 && moneyRunsOutAge == 0)
                {
                    isSustainable = false;
                    moneyRunsOutAge = model.RetirementAge + year;
                    break;
                }
            }

            decimal finalBalance = taxableBalance + traditionalBalance + rothBalance;

            return new SpendingResults
            {
                FinalBalance = Math.Round(finalBalance, 2),
                TaxableBalance = Math.Round(taxableBalance, 2),
                TraditionalBalance = Math.Round(traditionalBalance, 2),
                RothBalance = Math.Round(rothBalance, 2),
                TotalWithdrawals = Math.Round(totalWithdrawals, 2),
                TotalGrowth = Math.Round(totalGrowth, 2),
                TotalPartTimeIncome = Math.Round(totalPartTimeIncome, 2),
                TotalTaxesPaid = Math.Round(totalTaxesPaid, 2),
                IsSustainable = isSustainable,
                MoneyRunsOutAge = moneyRunsOutAge
            };
        }

        public List<YearlySpendingBreakdown> GetYearlyBreakdown(SpendingPlanModel model)
        {
            var breakdown = new List<YearlySpendingBreakdown>();

            // Track individual account balances
            decimal taxableBalance = model.TaxableBalance;
            decimal traditionalBalance = model.TraditionalBalance;
            decimal rothBalance = model.RothBalance;

            decimal currentWithdrawal = model.AnnualWithdrawal;

            for (int year = 1; year <= model.PlanYears; year++)
            {
                int age = model.RetirementAge + year - 1;

                // Starting balances for this year
                decimal startingTaxableBalance = taxableBalance;
                decimal startingTraditionalBalance = traditionalBalance;
                decimal startingRothBalance = rothBalance;

                // Add part-time income if in partial retirement phase
                decimal partTimeIncome = 0;
                if (model.HasPartialRetirement && age < model.PartialRetirementEndAge)
                {
                    partTimeIncome = model.PartialRetirementIncome;
                    taxableBalance += partTimeIncome; // Apply to taxable account
                }

                // Calculate gross withdrawal needed based on strategy
                decimal totalBalance = taxableBalance + traditionalBalance + rothBalance;
                decimal grossWithdrawalNeeded = CalculateWithdrawal(currentWithdrawal, totalBalance, model);

                // Determine withdrawals from each account based on priority strategy
                (decimal taxableWithdrawal, decimal traditionalWithdrawal, decimal rothWithdrawal) =
                    CalculateAccountWithdrawals(grossWithdrawalNeeded, taxableBalance, traditionalBalance, rothBalance, model);

                // Calculate tax on traditional withdrawals
                decimal taxOnTraditional = traditionalWithdrawal * (model.TraditionalTaxRate / 100);

                // Calculate investment returns for each account type
                decimal taxableGrowth = Math.Max(0, (taxableBalance - taxableWithdrawal) * (model.InvestmentReturn / 100));
                decimal traditionalGrowth = Math.Max(0, (traditionalBalance - traditionalWithdrawal) * (model.InvestmentReturn / 100));
                decimal rothGrowth = Math.Max(0, (rothBalance - rothWithdrawal) * (model.InvestmentReturn / 100));

                // Update balances
                decimal endingTaxableBalance = Math.Max(0, taxableBalance - taxableWithdrawal + taxableGrowth);
                decimal endingTraditionalBalance = Math.Max(0, traditionalBalance - traditionalWithdrawal + traditionalGrowth);
                decimal endingRothBalance = Math.Max(0, rothBalance - rothWithdrawal + rothGrowth);

                // Add to results
                breakdown.Add(new YearlySpendingBreakdown
                {
                    Year = year,
                    Age = age,

                    // Starting balances
                    StartingTaxableBalance = Math.Round(startingTaxableBalance, 2),
                    StartingTraditionalBalance = Math.Round(startingTraditionalBalance, 2),
                    StartingRothBalance = Math.Round(startingRothBalance, 2),

                    // Withdrawals
                    TaxableWithdrawal = Math.Round(taxableWithdrawal, 2),
                    TraditionalWithdrawal = Math.Round(traditionalWithdrawal, 2),
                    RothWithdrawal = Math.Round(rothWithdrawal, 2),

                    // Tax
                    TaxPaid = Math.Round(taxOnTraditional, 2),

                    // Growth
                    TaxableGrowth = Math.Round(taxableGrowth, 2),
                    TraditionalGrowth = Math.Round(traditionalGrowth, 2),
                    RothGrowth = Math.Round(rothGrowth, 2),

                    // Ending balances
                    EndingTaxableBalance = Math.Round(endingTaxableBalance, 2),
                    EndingTraditionalBalance = Math.Round(endingTraditionalBalance, 2),
                    EndingRothBalance = Math.Round(endingRothBalance, 2),

                    // Other info
                    PartTimeIncome = Math.Round(partTimeIncome, 2),
                    IsPartialRetirement = partTimeIncome > 0
                });

                // Update for next year
                taxableBalance = endingTaxableBalance;
                traditionalBalance = endingTraditionalBalance;
                rothBalance = endingRothBalance;

                // For inflation-adjusted, increase withdrawal amount
                if (model.Strategy == SpendingPlanModel.WithdrawalStrategy.InflationAdjusted)
                {
                    currentWithdrawal *= 1 + model.InflationRate / 100;
                }

                // Break if funds depleted
                if (taxableBalance + traditionalBalance + rothBalance <= 0)
                {
                    break;
                }
            }

            return breakdown;
        }

        private decimal CalculateWithdrawal(decimal baseWithdrawal, decimal currentBalance, SpendingPlanModel model)
        {
            switch (model.Strategy)
            {
                case SpendingPlanModel.WithdrawalStrategy.FixedAmount:
                    return baseWithdrawal;

                case SpendingPlanModel.WithdrawalStrategy.FixedPercentage:
                    return currentBalance * (model.WithdrawalPercentage / 100);

                case SpendingPlanModel.WithdrawalStrategy.InflationAdjusted:
                    // Base withdrawal is already adjusted for inflation in the main loop
                    return baseWithdrawal;

                default:
                    return baseWithdrawal;
            }
        }

        // Helper method to determine withdrawals from each account type
        private (decimal taxable, decimal traditional, decimal roth) CalculateAccountWithdrawals(
            decimal totalNeeded, decimal taxableBalance, decimal traditionalBalance, decimal rothBalance, SpendingPlanModel model)
        {
            decimal taxable = 0;
            decimal traditional = 0;
            decimal roth = 0;
            decimal remaining = totalNeeded;

            // Calculate total available
            decimal totalAvailable = taxableBalance + traditionalBalance + rothBalance;

            // Handle case where withdrawal exceeds total balance
            if (totalNeeded > totalAvailable)
            {
                return (
                    Math.Min(taxableBalance, taxableBalance / Math.Max(0.0001m, totalAvailable) * totalNeeded),
                    Math.Min(traditionalBalance, traditionalBalance / Math.Max(0.0001m, totalAvailable) * totalNeeded),
                    Math.Min(rothBalance, rothBalance / Math.Max(0.0001m, totalAvailable) * totalNeeded)
                );
            }

            switch (model.PriorityStrategy)
            {
                case SpendingPlanModel.WithdrawalPriorityStrategy.TaxOptimized:
                    // Tax-optimized: Taxable first, then Traditional, then Roth
                    taxable = Math.Min(remaining, taxableBalance);
                    remaining -= taxable;

                    if (remaining > 0)
                    {
                        traditional = Math.Min(remaining, traditionalBalance);
                        remaining -= traditional;
                    }

                    if (remaining > 0)
                    {
                        roth = Math.Min(remaining, rothBalance);
                    }
                    break;

                case SpendingPlanModel.WithdrawalPriorityStrategy.ProportionalSplit:
                    // Withdraw proportionally from all accounts
                    if (totalAvailable > 0)
                    {
                        taxable = Math.Min(taxableBalance, totalNeeded * (taxableBalance / totalAvailable));
                        traditional = Math.Min(traditionalBalance, totalNeeded * (traditionalBalance / totalAvailable));
                        roth = Math.Min(rothBalance, totalNeeded * (rothBalance / totalAvailable));
                    }
                    break;

                case SpendingPlanModel.WithdrawalPriorityStrategy.CustomOrder:
                    // Use custom order specified by user
                    foreach (string accountType in model.WithdrawalOrder)
                    {
                        if (remaining <= 0) break;

                        if (accountType == "Taxable" && taxableBalance > 0)
                        {
                            taxable = Math.Min(remaining, taxableBalance);
                            remaining -= taxable;
                        }
                        else if (accountType == "Traditional" && traditionalBalance > 0)
                        {
                            traditional = Math.Min(remaining, traditionalBalance);
                            remaining -= traditional;
                        }
                        else if (accountType == "Roth" && rothBalance > 0)
                        {
                            roth = Math.Min(remaining, rothBalance);
                            remaining -= roth;
                        }
                    }
                    break;
            }

            return (taxable, traditional, roth);
        }
    }
}
