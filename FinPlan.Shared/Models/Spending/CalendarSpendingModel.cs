using System;
using System.Collections.Generic;

namespace FinPlan.Shared.Models.Spending
{
    public class CalendarSpendingModel
    {
        // Inputs — mirror UI fields
        public int CurrentAgeYou { get; set; } = 63;
        public int CurrentAgePartner { get; set; } = 60;

        // New: retirement ages (user-friendly) — stored so model persists both age and computed year
        public int RetirementAgeYou { get; set; } = 65;
        public int RetirementAgePartner { get; set; } = 68;

        // Stored retirement years (kept in sync with ages)
        public int RetirementYearYou { get; set; } = DateTime.Now.Year + 5;
        public int RetirementYearPartner { get; set; } = DateTime.Now.Year + 8;

        // Auto-calc toggle (persisted with model)
        public bool AutoCalculate { get; set; } = false;

        public int SSStartYearYou { get; set; } = DateTime.Now.Year + 9;
        public int SSStartYearPartner { get; set; } = DateTime.Now.Year + 12;

        public int LifeExpectancyYou { get; set; } = 2090;
        public int LifeExpectancyPartner { get; set; } = 2095;

        public int SimulationStartYear { get; set; } = DateTime.Now.Year;

        // Money
        public decimal TaxableBalance { get; set; } = 250_000m;
        public decimal TraditionalBalance { get; set; } = 500_000m;
        public decimal RothBalance { get; set; } = 250_000m;
        public decimal TraditionalTaxRate { get; set; } = 22.0m;
        public decimal InvestmentReturn { get; set; } = 5.0m;
        public decimal InflationRate { get; set; } = 2.5m;

        // Withdrawals
        public decimal AnnualWithdrawalOne { get; set; } = 80_000m;
        public decimal AnnualWithdrawalBoth { get; set; } = 100_000m;
        public int ReverseMortgageStartYear { get; set; }
        public decimal ReverseMortgageMonthly { get; set; }

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

                // simple SS/withdrawal/growth model (placeholder)
                row.SSYou = (y >= SSStartYearYou) ? 15000m : 0m;
                row.SSPartner = (y >= SSStartYearPartner) ? 12000m : 0m;

                // withdrawal policy
                var isYouRetired = y >= RetirementYearYou;
                var isPartnerRetired = y >= RetirementYearPartner;
                decimal withdrawal = 0m;
                if (isYouRetired && isPartnerRetired) withdrawal = AnnualWithdrawalBoth;
                else if (isYouRetired || isPartnerRetired) withdrawal = AnnualWithdrawalOne;

                // reverse mortgage
                row.ReverseMortgage = (ReverseMortgageStartYear > 0 && y >= ReverseMortgageStartYear) ? ReverseMortgageMonthly * 12m : 0m;

                // Withdraw from taxable first, then traditional, then roth
                var taxableWithdraw = Math.Min(taxBal, withdrawal);
                taxBal -= taxableWithdraw;
                var remaining = withdrawal - taxableWithdraw;
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

                // growth
                row.Growth = (taxBal + tradBal + rothBal) * (InvestmentReturn / 100m);

                // ending balances (distribute growth)
                taxBal += row.Growth * 0.4m;
                tradBal += row.Growth * 0.4m;
                rothBal += row.Growth * 0.2m;

                row.EndingTaxable = taxBal;
                row.EndingTraditional = tradBal;
                row.EndingRoth = rothBal;

                if (taxBal + tradBal + rothBal <= 0) row.Notes = "Money depleted";

                YearRows.Add(row);
            }
        }
    }

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
    }
}
