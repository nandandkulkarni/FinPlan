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
            return false;
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
        public int RetirementYearYou { get; set; } = 0; //= DateTime.Now.Year + 5;
        public int RetirementYearPartner { get; set; } = 0; //<|cursor|>= DateTime.Now.Year + 8;

        // Auto-calc toggle (persisted with model)
        public bool AutoCalculate { get; set; } = false;

        public int SSStartYearYou { get; set; } = 0;// DateTime.Now.Year + 9;
        public int SSStartYearPartner { get; set; } = 0;// DateTime.Now.Year + 12;
        // New: allow user to enter SS start ages; years are computed from current ages
        public int SSStartAgeYou { get; set; } = 0;//67; // Keep reasonable defaults for SS ages
        public int SSStartAgePartner { get; set; } = 0; // Keep reasonable defaults for SS ages

        // New: expected Social Security benefit (monthly) for each person
        public decimal SocialSecurityMonthlyYou { get; set; } = 0m;
        public decimal SocialSecurityMonthlyPartner { get; set; } = 0m;

        public int LifeExpectancyYou { get; set; } = 0; // Changed from 2090 to 0 for empty state
        public int LifeExpectancyPartner { get; set; } = 0; // Changed from 2095 to 0 for empty state

        // Keep SimulationStartYear for compatibility with UI/tests but projections will start at earliest retirement year
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
        
        // Alternative property names for compatibility with UI
        public decimal WithdrawalOne 
        { 
            get => AnnualWithdrawalOne; 
            set => AnnualWithdrawalOne = value; 
        }
        public decimal WithdrawalBoth 
        { 
            get => AnnualWithdrawalBoth; 
            set => AnnualWithdrawalBoth = value; 
        }
        
        // Social Security Benefits - Alternative property names
        public decimal SSYou 
        { 
            get => SocialSecurityMonthlyYou * 12; // Convert monthly to annual
            set => SocialSecurityMonthlyYou = value / 12; // Store as monthly
        }
        public decimal SSPartner 
        { 
            get => SocialSecurityMonthlyPartner * 12; // Convert monthly to annual
            set => SocialSecurityMonthlyPartner = value / 12; // Store as monthly
        }
        
        public int ReverseMortgageStartYear { get; set; }
        public decimal ReverseMortgageMonthly { get; set; }

        // New: age at which primary person would start reverse mortgage (optional)
        public int ReverseMortgageStartAge { get; set; } = 0;

        public int PartialRetirementStart { get; set; }
        public int PartialRetirementEnd { get; set; }
        public decimal PartTimeIncome { get; set; }

        public decimal TaxableTaxRate { get; set; } = 15;

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

                // If AutoCalculate is enabled, previously SimulationStartYear was set; we no longer use SimulationStartYear

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


        private decimal CalculateSS(int currentAge, int ssStartAge, decimal monthlySSAtSSStartYear, decimal ssInflation)
        {
            if (currentAge < ssStartAge)        //Age of SS Withdrawal not reached
                return 0;

            int yearsSinceSSStart = currentAge - ssStartAge;

            decimal yearlySocialSecurity = monthlySSAtSSStartYear * 12;
            //Increase SS by inflation
            for (int yearOfSSWithdraw = 0; yearOfSSWithdraw < yearsSinceSSStart; yearOfSSWithdraw++)
            {
                yearlySocialSecurity += yearlySocialSecurity * ssInflation / 100;
            }

            return yearlySocialSecurity;
        }
        private decimal CalculateAmountNeededForCostOfLiving(
             int yourCurrentAge, int partnerCurrentAge,
            int yourRetirementAge, int partnerRetirementAge, decimal yearlyAmountNeededOneRetired, decimal yearlyAmountNeededBothRetired, decimal costOfLivingInflation)
        {

            bool youRetired = yourCurrentAge >= yourRetirementAge;
            bool partnerRetired = partnerCurrentAge >= partnerRetirementAge;

            if (!youRetired && !partnerRetired) //Both Not Retired
                return 0;

            if (youRetired && partnerRetired) //Both Retired
            {
                int yearsSinceBothRetired = Math.Min(yourCurrentAge - yourRetirementAge, partnerCurrentAge - partnerRetirementAge);
                //Increase cost of living by inflation
                int yearsSinceCOLStart = Math.Max(yourCurrentAge - yourRetirementAge, partnerCurrentAge - partnerRetirementAge);
                decimal amountNeededForCostOfLiving1 = yearlyAmountNeededBothRetired;
                for (int yearOfCOLWithdraw = 0; yearOfCOLWithdraw < yearsSinceBothRetired; yearOfCOLWithdraw++)
                {
                    amountNeededForCostOfLiving1 += amountNeededForCostOfLiving1 * costOfLivingInflation / 100;
                }
                return amountNeededForCostOfLiving1;
            }

            //One Retired
            int yearsSince = youRetired ? yourCurrentAge - yourRetirementAge : partnerCurrentAge - partnerRetirementAge;
            int yearsSinceFirstRetirement = Math.Max(0, yearsSince);
            decimal amountNeededForCostOfLiving2 = yearlyAmountNeededOneRetired;
            for (int yearOfCOLWithdraw = 0; yearOfCOLWithdraw < yearsSinceFirstRetirement; yearOfCOLWithdraw++)
            {
                amountNeededForCostOfLiving2 += amountNeededForCostOfLiving2 * costOfLivingInflation / 100;
            }
            return amountNeededForCostOfLiving2;

        }

        // Add inside the CalendarSpendingModel class (near other helpers)
        private decimal ComputeTaxOnTaxableGrowthSplit(decimal taxableGrowthAmount)
        {
            // Tax rates
            var ordinaryIncomeTaxRate = Math.Max(0m, TraditionalTaxRate / 100m); // reuse your ordinary rate
            const decimal longTermCapitalGainsTaxRate = 0.15m;                   // LT cap gains / qualified dividends
            const decimal annualRealizationFraction = 0.25m;                     // fraction of LT/ST gains realized each year

            // Income mix shares (mirrors Savings MixedInvestment)
            const decimal qualifiedDividendShare = 0.25m;
            const decimal nonQualifiedIncomeShare = 0.25m;
            const decimal longTermGainsShare = 0.40m;
            const decimal shortTermGainsShare = 0.10m;

            // Allocate yearly taxable growth into buckets
            var qualifiedDividendAmount = taxableGrowthAmount * qualifiedDividendShare;
            var nonQualifiedIncomeAmount = taxableGrowthAmount * nonQualifiedIncomeShare;
            var accruedLongTermGains = taxableGrowthAmount * longTermGainsShare;
            var accruedShortTermGains = taxableGrowthAmount * shortTermGainsShare;

            // Only a portion of gains are realized (and taxed) each year
            var realizedLongTermGainsThisYear = accruedLongTermGains * annualRealizationFraction;
            var realizedShortTermGainsThisYear = accruedShortTermGains * annualRealizationFraction;

            // Compute taxes for each bucket
            var taxOnQualifiedDividends = qualifiedDividendAmount * longTermCapitalGainsTaxRate;
            var taxOnNonQualifiedIncome = nonQualifiedIncomeAmount * ordinaryIncomeTaxRate;
            var taxOnRealizedLongTermGains = realizedLongTermGainsThisYear * longTermCapitalGainsTaxRate;
            var taxOnRealizedShortTermGains = realizedShortTermGainsThisYear * ordinaryIncomeTaxRate;

            return taxOnQualifiedDividends
                 + taxOnNonQualifiedIncome
                 + taxOnRealizedLongTermGains
                 + taxOnRealizedShortTermGains;
        }


        public void Calculate()
        {
            SyncRetirementYearsFromAges();

            var currentYear = DateTime.Now.Year;
            var lifeExpectancyYearYou = currentYear + (LifeExpectancyYou - CurrentAgeYou);
            var lifeExpectancyYearPartner = currentYear + (LifeExpectancyPartner - CurrentAgePartner);
            var simulationEndYear = Math.Max(lifeExpectancyYearYou, lifeExpectancyYearPartner);


            YearRows.Clear();

            var lastYearTaxableBalance = TaxableBalance;
            var lastYearTraditionalBalance = TraditionalBalance;
            var lastYearRothBalance = RothBalance;

            int yearSinceRetired = -1;

            // Start at earliest retirement year (person who retires earliest)
            int startYear;
            if (RetirementYearPartner > 0)
                startYear = Math.Min(RetirementYearYou, RetirementYearPartner);
            else
                startYear = RetirementYearYou;

            for (int year = startYear; year <= simulationEndYear; year++)
            {
                //DECIDED HOW MUCH TO WITHDRAW FOR LIVING EXPENSES
                var isYouRetired = year >= RetirementYearYou;
                var isPartnerRetired = year >= RetirementYearPartner;

                if (isYouRetired || isPartnerRetired)
                {
                    yearSinceRetired++;
                }

                var calendarYearRow = new CalendarYearRow { Year = year };

                // ages
                calendarYearRow.AgeYou = CurrentAgeYou + (year - DateTime.Now.Year);
                calendarYearRow.AgePartner = CurrentAgePartner + (year - DateTime.Now.Year);

                calendarYearRow.Milestone = GetMilestoneText(year);

                calendarYearRow.SSYou = CalculateSS(calendarYearRow.AgeYou, SSStartAgeYou, SocialSecurityMonthlyYou, InflationRate);
                calendarYearRow.SSPartner = CalculateSS(calendarYearRow.AgePartner, SSStartAgePartner, SocialSecurityMonthlyPartner, InflationRate);


                calendarYearRow.ReverseMortgage = GetReverseMortgageForYear(year);
                calendarYearRow.OtherTaxableIncome = OtherTaxableIncomeForYear(year);

                //CALCULATE INCOME
                calendarYearRow.TotalNonSSNonGrowthTaxableIncome = calendarYearRow.OtherTaxableIncome;

                //CALCULATE GROWTH
                calendarYearRow.GrowthOfTaxableBalance = CalculateNetGrowth(lastYearTaxableBalance, 12);
                calendarYearRow.GrowthOfTradionalBalance = CalculateNetGrowth(lastYearTraditionalBalance, 12);
                calendarYearRow.GrowthOfRothBalance = CalculateNetGrowth(lastYearRothBalance, 12);

                calendarYearRow.GrowthBeforeTaxes = calendarYearRow.GrowthOfTaxableBalance + calendarYearRow.GrowthOfTradionalBalance + calendarYearRow.GrowthOfRothBalance;

                calendarYearRow.TaxOnTaxableNonSSNonGrowthIncome = CalculateTaxOnTaxableNonSSIncome(calendarYearRow.TotalNonSSNonGrowthTaxableIncome);
                calendarYearRow.TaxOnSSIncome = (calendarYearRow.SSYou + calendarYearRow.SSPartner) * .15m;

                calendarYearRow.TaxOnTaxableInterestAndDividendGrowth = CalculateTaxOnTaxableInterestOrDividendGrowth(calendarYearRow.GrowthOfTaxableBalance);

                //calendarYearRow.TaxOnTaxableInterestAndDividendGrowth = ComputeTaxOnTaxableGrowthSplit(calendarYearRow.GrowthOfTaxableBalance);

                //WITHDRAWAL
                calendarYearRow.TaxesDueOnAllTaxableGrowthAndIncome =
                        calendarYearRow.TaxOnTaxableNonSSNonGrowthIncome +
                        calendarYearRow.TaxOnSSIncome +
                        calendarYearRow.TaxOnTaxableInterestAndDividendGrowth;

                decimal taxableBalanceSoFar = lastYearTaxableBalance + calendarYearRow.SSYou + calendarYearRow.SSPartner + calendarYearRow.ReverseMortgage + calendarYearRow.OtherTaxableIncome + calendarYearRow.GrowthOfTaxableBalance;
                decimal traditionalBalanceSoFar = lastYearTraditionalBalance + calendarYearRow.GrowthOfTradionalBalance;
                decimal rothBalanceSoFar = lastYearRothBalance + calendarYearRow.GrowthOfRothBalance;

                ///CALCULATE AMOUNT NEEDED FOR COST OF LIVING
                calendarYearRow.AmountNeededForCostOfLiving =
                    CalculateAmountNeededForCostOfLiving(
                        calendarYearRow.AgeYou,
                        calendarYearRow.AgePartner,
                        RetirementAgeYou,
                        RetirementAgePartner,
                        AnnualWithdrawalOne,
                        AnnualWithdrawalBoth,
                        InflationRate);

                {

                    calendarYearRow.TaxableWithdrawnForCostOfLivingIfAtAll = 0m;
                    calendarYearRow.TradWithdrawnForCostOfLivingIfAtAll = 0m;
                    calendarYearRow.RothWithdrawnForCostOfLivingIfAtAll = 0m;


                    //FIRST SPLIT THE WITHDRAWAL FOR COST OF LIVING

                    (
                        calendarYearRow.TaxableWithdrawnForCostOfLivingIfAtAll,
                        calendarYearRow.TradWithdrawnForCostOfLivingIfAtAll,
                        calendarYearRow.RothWithdrawnForCostOfLivingIfAtAll,
                        calendarYearRow.TotalWithdrawForCostOfLivingExcludingTaxes
                        ) = CalculateWithdrawalSplit(
                        taxableBalanceSoFar,
                        traditionalBalanceSoFar,
                        rothBalanceSoFar,
                        calendarYearRow.AmountNeededForCostOfLiving);


                    //SUBTRACT THE AMOUNT WITHDRAWN FROM BALANCE
                    taxableBalanceSoFar -= calendarYearRow.TaxableWithdrawnForCostOfLivingIfAtAll;
                    traditionalBalanceSoFar -= calendarYearRow.TradWithdrawnForCostOfLivingIfAtAll;
                    rothBalanceSoFar -= calendarYearRow.RothWithdrawnForCostOfLivingIfAtAll;
                }

                {
                    (
                    calendarYearRow.TaxableWithdrawForInitialAndProbablyOnlyTaxPaymenOnTaxableIncome,
                    calendarYearRow.TraditionalWithdrawForInitialTaxPaymentOnTaxableIncome,
                    calendarYearRow.RothWithdrawForInitialTaxPaymentOnTaxableIncome,
                    calendarYearRow.TotalWithdrawForInitialTaxPaymentOnTaxableIncome
                        ) = CalculateWithdrawalSplit(
                        taxableBalanceSoFar,
                        traditionalBalanceSoFar,
                        rothBalanceSoFar,
                        calendarYearRow.TaxesDueOnAllTaxableGrowthAndIncome);


                    taxableBalanceSoFar -= calendarYearRow.TaxableWithdrawForInitialAndProbablyOnlyTaxPaymenOnTaxableIncome;
                    traditionalBalanceSoFar -= calendarYearRow.TraditionalWithdrawForInitialTaxPaymentOnTaxableIncome;
                    rothBalanceSoFar -= calendarYearRow.RothWithdrawForInitialTaxPaymentOnTaxableIncome;
                }

                {
                    if (calendarYearRow.TraditionalWithdrawForInitialTaxPaymentOnTaxableIncome > 0)
                    {
                        decimal amountToTax = calendarYearRow.TraditionalWithdrawForInitialTaxPaymentOnTaxableIncome;
                        decimal totalWithdrawnFromTraditionalForTax = 0m;
                        decimal totalWithdrawnFromRothForTax = 0m;
                        decimal taxThreshold = 1m;
                        decimal totalTaxDue = 0m;
                        while (true)
                        {
                            decimal taxDue = CalculateTaxOnTraditional(amountToTax);

                            if (taxDue <= taxThreshold)
                                break;

                            totalTaxDue += taxDue;

                            decimal withdrawFromTraditional = Math.Min(traditionalBalanceSoFar, taxDue);
                            traditionalBalanceSoFar -= withdrawFromTraditional;
                            totalWithdrawnFromTraditionalForTax += withdrawFromTraditional;

                            if (withdrawFromTraditional < taxDue)
                            {
                                decimal withdrawFromRoth = taxDue - withdrawFromTraditional;
                                rothBalanceSoFar -= withdrawFromRoth;
                                totalWithdrawnFromRothForTax += withdrawFromRoth;
                                break;
                            }

                            amountToTax = withdrawFromTraditional;
                        }

                        calendarYearRow.TraditionalWithdrawnForTaxOnTraditional = totalWithdrawnFromTraditionalForTax;
                        calendarYearRow.RothWithdrawnForTaxOnTraditional = totalWithdrawnFromRothForTax;
                        calendarYearRow.TaxDueDueToTraditionalWithdrawnForTaxOnTraditional = totalTaxDue;
                    }


                }

                calendarYearRow.EndingTaxable = Math.Max(0, taxableBalanceSoFar);
                calendarYearRow.EndingTraditional = Math.Max(0, traditionalBalanceSoFar);
                calendarYearRow.EndingRoth = Math.Max(0, rothBalanceSoFar);

                lastYearTaxableBalance = (int)calendarYearRow.EndingTaxable;
                lastYearTraditionalBalance = (int)calendarYearRow.EndingTraditional;
                lastYearRothBalance = (int)calendarYearRow.EndingRoth;

                if (calendarYearRow.AmountNeededForCostOfLiving > 0m && calendarYearRow.TotalWithdrawForCostOfLivingExcludingTaxes < calendarYearRow.AmountNeededForCostOfLiving)
                {
                    if (!string.IsNullOrWhiteSpace(calendarYearRow.Milestone))
                        calendarYearRow.Milestone += ", ";
                    calendarYearRow.Milestone += "Depleted";
                }

                YearRows.Add(calendarYearRow);
            }

        }
        internal (decimal taxableWithdraw, decimal tradWithdraw, decimal rothWithdraw, decimal totalAmountWithdrawn) CalculateWithdrawalSplit(decimal taxableBalance, decimal tradBal, decimal rothBalance, decimal netNeededFromAccounts)
        {
            decimal taxableWithdrawn = Math.Min(taxableBalance, netNeededFromAccounts);

            decimal stillNeeded = netNeededFromAccounts - taxableWithdrawn;

            decimal tradWithdrawn = Math.Min(tradBal, stillNeeded);

            stillNeeded = stillNeeded - tradWithdrawn;

            decimal rothWithdrawn = Math.Min(rothBalance, stillNeeded);

            decimal totalAmountWithdrawn = taxableWithdrawn + tradWithdrawn + rothWithdrawn;

            return (taxableWithdrawn, tradWithdrawn, rothWithdrawn, totalAmountWithdrawn);
        }

        internal decimal CalculateTaxOnTaxableInterestOrDividendGrowth(decimal balanceEarningInterestAndDividend)
        {
            return balanceEarningInterestAndDividend * (TaxableTaxRate / 100m);
        }

        internal decimal CalculateTaxOnTaxableNonSSIncome(decimal nonSSIncome)
        {
            return nonSSIncome * (TaxableTaxRate / 100m);
        }

        internal decimal CalculateNetGrowth(decimal balance, int compoundingCycles)
        {
            return balance * (InvestmentReturn / 100m); //Simple interest for now
        }

     

        internal decimal OtherTaxableIncomeForYear(int year)
        {
            // Placeholder for other taxable income sources if needed
            return 0;
        }

        internal decimal GetReverseMortgageForYear(int year)
        {
            if (ReverseMortgageStartYear > 0 && year >= ReverseMortgageStartYear)
            {
                return ReverseMortgageMonthly * 12m;
            }
            return 0m;
        }

        internal decimal GetSocialSecurityForYear(int year, bool isPartner, int yearForInflationAdjustment)
        {
            var ssStartYear = isPartner ? SSStartYearPartner : SSStartYearYou;
            var monthlyBenefit = isPartner ? SocialSecurityMonthlyPartner : SocialSecurityMonthlyYou;

            if (year < ssStartYear) return 0m;
            var yearsSince = year - ssStartYear;
            var factor = (decimal)Math.Pow((double)(1 + (double)(InflationRate / 100m)), yearsSince);
            return monthlyBenefit * 12m * factor;
        }

        internal string GetMilestoneText(int year)
        {

            // milestones
            var milestones = new List<string>();
            if (year == RetirementYearYou) milestones.Add("You retire");
            if (year == RetirementYearPartner) milestones.Add("Partner retires");
            if (year == SSStartYearYou) milestones.Add("SS You");
            if (year == SSStartYearPartner) milestones.Add("SS Partner");
            return string.Join(", ", milestones);
        }

        /// <summary>
        /// Simulates annual withdrawals and returns the year each account is depleted
        /// </summary>
        public (int? taxableDepletedYear, int? traditionalDepletedYear, int? rothDepletedYear) DepleteAccounts()
        {
            SyncRetirementYearsFromAges();
            int start;
            if (RetirementYearPartner > 0)
                start = Math.Min(RetirementYearYou, RetirementYearPartner);
            else
                start = RetirementYearYou;

            var currentYear = DateTime.Now.Year;
            var lifeExpectancyYearYou = currentYear + (LifeExpectancyYou - CurrentAgeYou);
            var lifeExpectancyYearPartner = currentYear + (LifeExpectancyPartner - CurrentAgePartner);
            var end = Math.Max(lifeExpectancyYearYou, lifeExpectancyYearPartner);

            decimal taxBal = TaxableBalance;
            decimal tradBal = TraditionalBalance;
            decimal rothBal = RothBalance;
            int? taxableDepletedYear = null;
            int? traditionalDepletedYear = null;
            int? rothDepletedYear = null;

            for (int y = start; y <= end; y++)
            {
                // Determine amountNeededForCostOfLiving amount for this year
                var isYouRetired = y >= RetirementYearYou;
                var isPartnerRetired = y >= RetirementYearPartner;
                decimal withdrawal = 0m;
                if (isYouRetired && isPartnerRetired) withdrawal = AnnualWithdrawalBoth;
                else if (isYouRetired || isPartnerRetired) withdrawal = AnnualWithdrawalOne;

                // Withdraw from taxable first, then traditional, then roth
                decimal taxableWithdraw = Math.Min(taxBal, withdrawal);
                taxBal -= taxableWithdraw;
                if (taxBal <= 0 && taxableDepletedYear == null) taxableDepletedYear = y;

                decimal remaining = withdrawal - taxableWithdraw;
                decimal tradWithdraw = Math.Min(tradBal, remaining);
                tradBal -= tradWithdraw;
                if (tradBal <= 0 && traditionalDepletedYear == null) traditionalDepletedYear = y;

                remaining -= tradWithdraw;
                decimal rothWithdraw = Math.Min(rothBal, remaining);
                rothBal -= rothWithdraw;
                if (rothBal <= 0 && rothDepletedYear == null) rothDepletedYear = y;

                // If all buckets depleted, break
                if (taxBal <= 0 && tradBal <= 0 && rothBal <= 0) break;
            }
            return (taxableDepletedYear, traditionalDepletedYear, rothDepletedYear);
        }

        /// <summary>
        /// Estimates the taxable portion of Social Security benefits using IRS rules (simplified for married filing jointly)
        /// </summary>
        /// <param name="socialSecurityBenefits">Total annual Social Security benefits</param>
        /// <param name="otherIncome">Other taxable income (withdrawals, interest, etc.)</param>
        /// <param name="marriedFilingJointly">True if married filing jointly, false for single</param>
        /// <returns>Estimated taxable Social Security benefits</returns>
        public static decimal EstimateTaxableSocialSecurity(decimal socialSecurityBenefits, decimal otherIncome, bool marriedFilingJointly = true)
        {
            // IRS thresholds for 2024
            decimal baseAmount = marriedFilingJointly ? 32000m : 25000m;
            decimal maxAmount = marriedFilingJointly ? 44000m : 34000m;

            decimal combinedIncome = otherIncome + 0.5m * socialSecurityBenefits;

            if (combinedIncome <= baseAmount)
                return 0m;
            else if (combinedIncome <= maxAmount)
                return 0.5m * (combinedIncome - baseAmount);
            else
            {
                // Up to 85% of benefits may be taxable
                decimal taxable = 0.85m * socialSecurityBenefits;
                // IRS formula: 0.85 * (combinedIncome - maxAmount) + lesser of (baseAmount, 0.5 * socialSecurityBenefits)
                decimal excess = combinedIncome - maxAmount;
                decimal lesser = Math.Min(0.5m * socialSecurityBenefits, baseAmount);
                decimal result = 0.85m * excess + lesser;
                return Math.Min(result, taxable);
            }
        }

        // Calculation helpers for testability
        internal decimal CalculateNetNeededFromAccounts(decimal withdrawal, decimal availableIncome)
        {
            return Math.Max(0m, withdrawal - availableIncome);
        }



        internal decimal CalculateEstimatedTaxableSS(decimal ssTotal, decimal otherIncome)
        {
            return EstimateTaxableSocialSecurity(ssTotal, otherIncome, true);
        }

        internal decimal CalculateTaxOnTraditional(decimal tradWithdraw)
        {
            return tradWithdraw * (TraditionalTaxRate / 100m);
        }

        internal decimal CalculateTaxOnSS(decimal estimatedTaxableSS)
        {
            return estimatedTaxableSS * (TraditionalTaxRate / 100m);
        }

        internal decimal CalculateTaxOnTaxableGrowth(decimal taxableBalance)
        {
            var taxableGrowth = taxableBalance * (InvestmentReturn / 100m);
            return taxableGrowth * (TraditionalTaxRate / 100m);
        }

        internal decimal CalculateGrowth(decimal balance)
        {
            return balance * (InvestmentReturn / 100m);
        }

        internal decimal CalculateTotalGrowth(decimal taxableBalance, decimal tradBalance, decimal rothBalance)
        {
            return CalculateGrowth(taxableBalance) + CalculateGrowth(tradBalance) + CalculateGrowth(rothBalance);
        }

        internal decimal CalculateTaxesPaid(decimal tradWithdraw, decimal estimatedTaxableSS, decimal taxOnTaxableGrowth)
        {
            return CalculateTaxOnTraditional(tradWithdraw) + CalculateTaxOnSS(estimatedTaxableSS) + taxOnTaxableGrowth;
        }

        internal (decimal endingTaxable, decimal endingTraditional, decimal endingRoth) CalculateEndingBalances(
            decimal taxBal, decimal tradBal, decimal rothBal,
            decimal taxableGrowth, decimal traditionalGrowth, decimal rothGrowth)
        {
            return (taxBal + taxableGrowth, tradBal + traditionalGrowth, rothBal + rothGrowth);
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

        public decimal OtherTaxableIncome { get; set; }
        public decimal TaxesDueOnAllTaxableGrowthAndIncome { get; set; }
        public decimal Growth { get; set; }
        public decimal EndingTaxable { get; set; }
        public decimal EndingTraditional { get; set; }
        public decimal EndingRoth { get; set; }
        public string Notes { get; set; } = string.Empty;
        public decimal AmountNeededForCostOfLiving { get; set; }
        public decimal TotalNonSSNonGrowthTaxableIncome { get; internal set; }
        public decimal GrowthOfTaxableBalance { get; internal set; }
        public decimal EstimatedTaxableSocialSecurity { get; internal set; }
        public decimal TaxOnTaxableNonSSNonGrowthIncome { get; internal set; }
        public decimal TaxOnSSIncome { get; internal set; }
        public decimal TaxOnTaxableInterestAndDividendGrowth { get; internal set; }
        public decimal GrowthOfTradionalBalance { get; internal set; }
        public decimal GrowthOfRothBalance { get; internal set; }
        public decimal GrowthBeforeTaxes { get; internal set; }
        public decimal TaxOnTraditionalWithdrawalDoneForCostOfLiving { get; internal set; }
        public decimal TaxableWithdrawForInitialAndProbablyOnlyTaxPaymenOnTaxableIncome { get; internal set; }
        public decimal TraditionalWithdrawForInitialTaxPaymentOnTaxableIncome { get; internal set; }
        public decimal RothWithdrawForInitialTaxPaymentOnTaxableIncome { get; internal set; }
        public decimal TotalWithdrawForInitialTaxPaymentOnTaxableIncome { get; internal set; }
        public decimal TotalWithdrawForCostOfLivingExcludingTaxes { get; internal set; }
        public decimal TaxableWithdrawnForCostOfLivingIfAtAll { get; internal set; }
        public decimal TradWithdrawnForCostOfLivingIfAtAll { get; internal set; }
        public decimal RothWithdrawnForCostOfLivingIfAtAll { get; internal set; }
        public decimal TraditionalWithdrawnForTaxOnTraditional { get; set; }
        public decimal RothWithdrawnForTaxOnTraditional { get; set; }
        public decimal TaxDueDueToTraditionalWithdrawnForTaxOnTraditional { get; internal set; }

        public decimal TotalTaxableWithdrawn
        {
            get
            {
                return

                    TaxableWithdrawnForCostOfLivingIfAtAll +
                    TaxableWithdrawForInitialAndProbablyOnlyTaxPaymenOnTaxableIncome;
            }
        }

        public decimal TotalTraditionalWithdrawn
        {
            get
            {
                return
                    TradWithdrawnForCostOfLivingIfAtAll +
                    TraditionalWithdrawForInitialTaxPaymentOnTaxableIncome +
                    TraditionalWithdrawnForTaxOnTraditional;
            }
        }

        public decimal TotalRothWithdrawn
        {
            get
            {
                return
                    RothWithdrawnForCostOfLivingIfAtAll +
                    RothWithdrawForInitialTaxPaymentOnTaxableIncome +
                    RothWithdrawnForTaxOnTraditional;
            }
        }
    }
}
