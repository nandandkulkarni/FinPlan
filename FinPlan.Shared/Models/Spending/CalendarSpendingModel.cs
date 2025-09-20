using System;
using System.Collections.Generic;
using System.Linq;

namespace FinPlan.Shared.Models.Spending
{
    public class MissingFieldInfo
    {
        public string FieldName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int WizardStep { get; set; }
        public bool IsRequired { get; set; } = true;
    }

    public class SetupSection
    {
        public int StepNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsComplete { get; set; }
        public List<MissingFieldInfo> MissingFields { get; set; } = new();
        public int CompletedFields { get; set; }
        public int TotalFields { get; set; }
    }

    public class CalendarSpendingModel
    {
        /// <summary>
        /// Gets a list of setup sections with completion status and missing fields
        /// </summary>
        public List<SetupSection> GetSetupSections()
        {
            var sections = new List<SetupSection>();

            // Step 1: Ages & Life Expectancy
            var step1Missing = new List<MissingFieldInfo>();
            if (CurrentAgeYou == 0) step1Missing.Add(new MissingFieldInfo { DisplayName = "Your current age", Description = "Required for retirement timeline calculations", WizardStep = 1 });
            if (CurrentAgePartner == 0) step1Missing.Add(new MissingFieldInfo { DisplayName = "Partner's current age", Description = "Required for joint retirement planning", WizardStep = 1 });
            if (RetirementAgeYou == 0) step1Missing.Add(new MissingFieldInfo { DisplayName = "Your retirement age", Description = "When do you plan to retire?", WizardStep = 1 });
            if (RetirementAgePartner == 0) step1Missing.Add(new MissingFieldInfo { DisplayName = "Partner's retirement age", Description = "When does your partner plan to retire?", WizardStep = 1 });
            if (LifeExpectancyYou == 0) step1Missing.Add(new MissingFieldInfo { DisplayName = "Your life expectancy", Description = "How long should your money last?", WizardStep = 1 });
            if (LifeExpectancyPartner == 0) step1Missing.Add(new MissingFieldInfo { DisplayName = "Partner's life expectancy", Description = "Planning horizon for joint finances", WizardStep = 1 });

            sections.Add(new SetupSection 
            { 
                StepNumber = 1, 
                Title = "Ages & Life Expectancy", 
                IsComplete = step1Missing.Count == 0,
                MissingFields = step1Missing,
                CompletedFields = 6 - step1Missing.Count,
                TotalFields = 6
            });

            // Step 2: Starting Balances
            var step2Missing = new List<MissingFieldInfo>();
            if (TaxableBalance == 0 && TraditionalBalance == 0 && RothBalance == 0) 
                step2Missing.Add(new MissingFieldInfo { DisplayName = "Starting account balances", Description = "At least one account balance is required", WizardStep = 2 });

            sections.Add(new SetupSection 
            { 
                StepNumber = 2, 
                Title = "Starting Balances & Returns", 
                IsComplete = step2Missing.Count == 0,
                MissingFields = step2Missing,
                CompletedFields = 1 - step2Missing.Count,
                TotalFields = 1
            });

            // Step 3: Social Security
            var step3Missing = new List<MissingFieldInfo>();
            if (SocialSecurityMonthlyYou == 0) step3Missing.Add(new MissingFieldInfo { DisplayName = "Your Social Security benefit", Description = "Expected monthly benefit amount", WizardStep = 3 });
            if (SocialSecurityMonthlyPartner == 0) step3Missing.Add(new MissingFieldInfo { DisplayName = "Partner's Social Security benefit", Description = "Expected monthly benefit amount", WizardStep = 3 });

            sections.Add(new SetupSection 
            { 
                StepNumber = 3, 
                Title = "Social Security", 
                IsComplete = step3Missing.Count == 0,
                MissingFields = step3Missing,
                CompletedFields = 2 - step3Missing.Count,
                TotalFields = 2
            });

            // Step 5: Withdrawal Strategy
            var step5Missing = new List<MissingFieldInfo>();
            if (AnnualWithdrawalOne == 0) step5Missing.Add(new MissingFieldInfo { DisplayName = "Withdrawal (one retired)", Description = "Annual spending when one person is retired", WizardStep = 5 });
            if (AnnualWithdrawalBoth == 0) step5Missing.Add(new MissingFieldInfo { DisplayName = "Withdrawal (both retired)", Description = "Annual spending when both are retired", WizardStep = 5 });

            sections.Add(new SetupSection 
            { 
                StepNumber = 5, 
                Title = "Withdrawal Strategy", 
                IsComplete = step5Missing.Count == 0,
                MissingFields = step5Missing,
                CompletedFields = 2 - step5Missing.Count,
                TotalFields = 2
            });

            return sections;
        }

        /// <summary>
        /// Gets the overall completion percentage across all sections
        /// </summary>
        public int GetOverallCompletionPercentage()
        {
            var sections = GetSetupSections();
            var totalFields = sections.Sum(s => s.TotalFields);
            var completedFields = sections.Sum(s => s.CompletedFields);
            return totalFields > 0 ? (int)((double)completedFields / totalFields * 100) : 0;
        }

        /// <summary>
        /// Gets the next incomplete step number for smart navigation
        /// </summary>
        public int GetNextIncompleteStep()
        {
            var sections = GetSetupSections();
            var nextIncomplete = sections.FirstOrDefault(s => !s.IsComplete);
            return nextIncomplete?.StepNumber ?? 1;
        }

        /// <summary>
        /// Determines if the model is completely empty (first-time user state)
        /// </summary>
        public bool IsModelEmpty()
        {
            return CurrentAgeYou == 0 || 
                   CurrentAgePartner == 0 ||
                   RetirementAgeYou == 0 || 
                   RetirementAgePartner == 0 ||
                   (TaxableBalance == 0 && TraditionalBalance == 0 && RothBalance == 0) ||
                   (AnnualWithdrawalOne == 0 && AnnualWithdrawalBoth == 0);
        }

        /// <summary>
        /// Determines if the model has some data but is not complete (partial state)
        /// </summary>
        public bool IsModelPartiallyComplete()
        {
            if (IsModelEmpty()) return false;
            if (IsModelComplete()) return false;
            
            // Has some basic data but missing essential components
            bool hasValidAges = CurrentAgeYou > 0 && CurrentAgePartner > 0 && 
                               RetirementAgeYou > 0 && RetirementAgePartner > 0;
            bool hasAnyMoney = TaxableBalance > 0 || TraditionalBalance > 0 || RothBalance > 0;
            bool hasWithdrawalStrategy = AnnualWithdrawalOne > 0 || AnnualWithdrawalBoth > 0;
            bool hasLifeExpectancy = LifeExpectancyYou > 0 && LifeExpectancyPartner > 0;
            
            // Partial if has some but not all essential elements
            return (hasValidAges && !hasAnyMoney) || 
                   (hasValidAges && !hasWithdrawalStrategy) ||
                   (hasAnyMoney && !hasValidAges) ||
                   (hasValidAges && hasAnyMoney && !hasLifeExpectancy);
        }

        /// <summary>
        /// Determines if the model has all essential data for meaningful retirement calculations
        /// </summary>
        public bool IsModelComplete()
        {
            // Essential requirements for a complete retirement model
            bool hasValidAges = CurrentAgeYou > 0 &&
                               CurrentAgePartner > 0 &&
                               RetirementAgeYou > 0 &&
                               RetirementAgePartner > 0 &&
                               CurrentAgeYou >= 18 &&
                               CurrentAgePartner >= 18 &&
                               CurrentAgeYou < RetirementAgeYou &&
                               CurrentAgePartner < RetirementAgePartner;

            
            bool hasRetirementMoney = TaxableBalance > 0 || 
                                     TraditionalBalance > 0 || 
                                     RothBalance > 0;
            
            bool hasWithdrawalStrategy = AnnualWithdrawalOne > 0 && 
                                        AnnualWithdrawalBoth > 0 &&
                                        AnnualWithdrawalBoth >= AnnualWithdrawalOne; // logical constraint

            bool hasReasonableRates = InvestmentReturn >= 0 &&
                                     TraditionalTaxRate >= 0;

            
            bool hasLifeExpectancy = LifeExpectancyYou > RetirementAgeYou && 
                                    LifeExpectancyPartner > RetirementAgePartner;
            
            return hasValidAges && hasRetirementMoney && hasWithdrawalStrategy && 
                   hasReasonableRates && hasLifeExpectancy;
        }

        // Inputs – mirror UI fields - Updated for empty state management
        public int CurrentAgeYou { get; set; } = 0; // Changed from 63 to 0 for empty state
        public int CurrentAgePartner { get; set; } = 0; // Changed from 60 to 0 for empty state

        // New: retirement ages (user-friendly) – stored so model persists both age and computed year
        public int RetirementAgeYou { get; set; } = 0; // Changed from 65 to 0 for empty state
        public int RetirementAgePartner { get; set; } = 0; // Changed from 68 to 0 for empty state

        // Stored retirement years (kept in sync with ages)
        public int RetirementYearYou { get; set; } = DateTime.Now.Year + 5;
        public int RetirementYearPartner { get; set; } = DateTime.Now.Year + 8;

        // Auto-calc toggle (persisted with model)
        public bool AutoCalculate { get; set; } = false;

        public int SSStartYearYou { get; set; } = DateTime.Now.Year + 9;
        public int SSStartYearPartner { get; set; } = DateTime.Now.Year + 12;
        // New: allow user to enter SS start ages; years are computed from current ages
        public int SSStartAgeYou { get; set; } = 67; // Keep reasonable defaults for SS ages
        public int SSStartAgePartner { get; set; } = 67; // Keep reasonable defaults for SS ages

        // New: expected Social Security benefit (monthly) for each person
        public decimal SocialSecurityMonthlyYou { get; set; } = 0m;
        public decimal SocialSecurityMonthlyPartner { get; set; } = 0m;

        public int LifeExpectancyYou { get; set; } = 0; // Changed from 2090 to 0 for empty state
        public int LifeExpectancyPartner { get; set; } = 0; // Changed from 2095 to 0 for empty state

        public int SimulationStartYear { get; set; } = DateTime.Now.Year;

        // Money - Updated for empty state management
        public decimal TaxableBalance { get; set; } = 0m; // Changed from 250_000m to 0 for empty state
        public decimal TraditionalBalance { get; set; } = 0m; // Changed from 500_000m to 0 for empty state
        public decimal RothBalance { get; set; } = 0m; // Changed from 250_000m to 0 for empty state
        public decimal TraditionalTaxRate { get; set; } = 22.0m; // Keep reasonable default for tax rate
        public decimal InvestmentReturn { get; set; } = 5.0m; // Keep reasonable default for investment return
        public decimal InflationRate { get; set; } = 2.5m; // Keep reasonable default for inflation

        // Withdrawals - Updated for empty state management
        public decimal AnnualWithdrawalOne { get; set; } = 0m; // Changed from 80_000m to 0 for empty state
        public decimal AnnualWithdrawalBoth { get; set; } = 0m; // Changed from 100_000m to 0 for empty state
        public int ReverseMortgageStartYear { get; set; }
        public decimal ReverseMortgageMonthly { get; set; }

        // New: age at which primary person would start reverse mortgage (optional)
        public int ReverseMortgageStartAge { get; set; } = 0;

        public int PartialRetirementStart { get; set; }
        public int PartialRetirementEnd { get; set; }
        public decimal PartTimeIncome { get; set; }

        // Results/grid
        public List<CalendarYearRow> YearRows { get; set; } = new();

        // Sync helper: compute retirement years from ages and current ages
        public void SyncRetirementYearsFromAges()
        {
            try
            {
                var nowYear = DateTime.Now.Year;
                // compute year = nowYear + (retirementAge - currentAge)
                RetirementYearYou = nowYear + (RetirementAgeYou - CurrentAgeYou);
                RetirementYearPartner = nowYear + (RetirementAgePartner - CurrentAgePartner);

                // basic bounds
                if (RetirementYearYou < nowYear) RetirementYearYou = nowYear;
                if (RetirementYearPartner < nowYear) RetirementYearPartner = nowYear;

                // compute SS start years from ages
                SSStartYearYou = nowYear + (SSStartAgeYou - CurrentAgeYou);
                SSStartYearPartner = nowYear + (SSStartAgePartner - CurrentAgePartner);
                if (SSStartYearYou < nowYear) SSStartYearYou = nowYear;
                if (SSStartYearPartner < nowYear) SSStartYearPartner = nowYear;

                // If AutoCalculate is enabled, set SimulationStartYear to two years before the earliest retirement year
                if (AutoCalculate)
                {
                    var earliestRetirement = Math.Min(RetirementYearYou, RetirementYearPartner);
                    var suggestedStart = earliestRetirement - 2;
                    // do not allow start before current year
                    if (suggestedStart < nowYear) suggestedStart = nowYear;
                    SimulationStartYear = suggestedStart;
                }

                // If ReverseMortgageStartAge is set, compute ReverseMortgageStartYear
                if (ReverseMortgageStartAge > 0)
                {
                    ReverseMortgageStartYear = nowYear + (ReverseMortgageStartAge - CurrentAgeYou);
                    if (ReverseMortgageStartYear < nowYear) ReverseMortgageStartYear = nowYear;
                }
            }
            catch
            {
                // ignore any unexpected errors in sync
            }
        }

        // Calculate fills YearRows based on current inputs
        public void Calculate()
        {
            // ensure retirement years are in sync before calculation
            SyncRetirementYearsFromAges();

            YearRows.Clear();

            var start = SimulationStartYear;
            var end = Math.Max(LifeExpectancyYou, LifeExpectancyPartner);

            decimal taxBal = TaxableBalance;
            decimal tradBal = TraditionalBalance;
            decimal rothBal = RothBalance;

            // State for AmountNeeded inflation tracking
            int currentPhase = 0; // 0=none,1=one-retired,2=both-retired
            decimal lastAmountNeeded = 0m;

            for (int y = start; y <= end; y++)
            {
                var row = new CalendarYearRow { Year = y };

                // ages
                row.AgeYou = CurrentAgeYou + (y - DateTime.Now.Year);
                row.AgePartner = CurrentAgePartner + (y - DateTime.Now.Year);

                // milestones
                var milestones = new List<string>();
                if (y == RetirementYearYou) milestones.Add("You retire");
                if (y == RetirementYearPartner) milestones.Add("Partner retires");
                if (y == SSStartYearYou) milestones.Add("SS You");
                if (y == SSStartYearPartner) milestones.Add("SS Partner");
                row.Milestone = string.Join(", ", milestones);

                // Social Security: use expected monthly benefits converted to annual when eligible, inflation-adjusted each year since start
                if (y >= SSStartYearYou)
                {
                    var yearsSince = y - SSStartYearYou;
                    var factor = (decimal)Math.Pow((double)(1 + (double)(InflationRate / 100m)), yearsSince);
                    row.SSYou = SocialSecurityMonthlyYou * 12m * factor;
                }
                else
                {
                    row.SSYou = 0m;
                }

                if (y >= SSStartYearPartner)
                {
                    var yearsSinceP = y - SSStartYearPartner;
                    var factorP = (decimal)Math.Pow((double)(1 + (double)(InflationRate / 100m)), yearsSinceP);
                    row.SSPartner = SocialSecurityMonthlyPartner * 12m * factorP;
                }
                else
                {
                    row.SSPartner = 0m;
                }

                // withdrawal policy
                var isYouRetired = y >= RetirementYearYou;
                var isPartnerRetired = y >= RetirementYearPartner;
                decimal withdrawal = 0m;
                if (isYouRetired && isPartnerRetired) withdrawal = AnnualWithdrawalBoth;
                else if (isYouRetired || isPartnerRetired) withdrawal = AnnualWithdrawalOne;

                // Determine phase for AmountNeeded computation
                int phase = 0;
                if (isYouRetired && isPartnerRetired) phase = 2;
                else if (isYouRetired || isPartnerRetired) phase = 1;

                // Compute AmountNeeded
                if (phase == 0)
                {
                    lastAmountNeeded = 0m;
                    row.AmountNeeded = 0m;
                    currentPhase = 0;
                }
                else
                {
                    decimal phaseBase = (phase == 1) ? AnnualWithdrawalOne : AnnualWithdrawalBoth;

                    if (phase != currentPhase)
                    {
                        // phase just started this year; set base
                        lastAmountNeeded = phaseBase;
                        row.AmountNeeded = Math.Round(lastAmountNeeded, 2);
                        currentPhase = phase;
                    }
                    else
                    {
                        // continue same phase – inflate previous amount by InflationRate
                        var factor = 1 + (InflationRate / 100m);
                        lastAmountNeeded = lastAmountNeeded * factor;
                        row.AmountNeeded = Math.Round(lastAmountNeeded, 2);
                    }
                }

                // reverse mortgage
                row.ReverseMortgage = (ReverseMortgageStartYear > 0 && y >= ReverseMortgageStartYear) ? ReverseMortgageMonthly * 12m : 0m;

                // --- Prefer Social Security then Reverse Mortgage, then accounts ---
                // available income for this year BEFORE touching account balances
                decimal availableIncome = row.SSYou + row.SSPartner + row.ReverseMortgage;

                // net needed from accounts after SS + reverse mortgage
                decimal netNeededFromAccounts = Math.Max(0m, withdrawal - availableIncome);

                // Withdraw from taxable first, then traditional, then roth to meet netNeededFromAccounts
                var taxableWithdraw = Math.Min(taxBal, netNeededFromAccounts);
                taxBal -= taxableWithdraw;
                var remaining = netNeededFromAccounts - taxableWithdraw;
                var tradWithdraw = Math.Min(tradBal, remaining);
                tradBal -= tradWithdraw;
                remaining -= tradWithdraw;
                var rothWithdraw = Math.Min(rothBal, remaining);
                rothBal -= rothWithdraw;

                row.TaxableWithdrawal = taxableWithdraw;
                row.TraditionalWithdrawal = tradWithdraw;
                row.RothWithdrawal = rothWithdraw;

                // taxes paid on traditional withdrawal
                row.TaxesPaid = tradWithdraw * (TraditionalTaxRate / 100m);

                // FIXED: Calculate growth for each account based on its actual balance
                decimal taxableGrowth = taxBal * (InvestmentReturn / 100m);
                decimal traditionalGrowth = tradBal * (InvestmentReturn / 100m);
                decimal rothGrowth = rothBal * (InvestmentReturn / 100m);

                // Apply growth to each account
                taxBal += taxableGrowth;
                tradBal += traditionalGrowth;
                rothBal += rothGrowth;

                // Total growth for display
                row.Growth = taxableGrowth + traditionalGrowth + rothGrowth;

                row.EndingTaxable = taxBal;
                row.EndingTraditional = tradBal;
                row.EndingRoth = rothBal;

                if (taxBal + tradBal + rothBal <= 0) row.Notes = "Money depleted";

                YearRows.Add(row);
            }
        }
    }

    // CalendarYearRow moved to top-level so other projects can reference the type directly
    public class CalendarYearRow
    {
        public int Year { get; set; }
        public int AgeYou { get; set; }
        public int AgePartner { get; set; }
        public string Milestone { get; set; } = string.Empty;
        public decimal SSYou { get; set; }
        public decimal SSPartner { get; set; }
        public decimal ReverseMortgage { get; set; }
        public decimal TaxableWithdrawal { get; set; }
        public decimal TraditionalWithdrawal { get; set; }
        public decimal RothWithdrawal { get; set; }
        public decimal TaxesPaid { get; set; }
        public decimal Growth { get; set; }
        public decimal EndingTaxable { get; set; }
        public decimal EndingTraditional { get; set; }
        public decimal EndingRoth { get; set; }
        public string Notes { get; set; } = string.Empty;
        // New: computed phase withdrawal target (inflation adjusted across years within same phase)
        public decimal AmountNeeded { get; set; }
    }
}
