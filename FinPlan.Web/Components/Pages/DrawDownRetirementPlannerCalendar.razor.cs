using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FinPlan.Shared.Models;
using FinPlan.Shared.Models.Spending;
using FinPlan.Web.Services;

namespace FinPlan.Web.Components.Pages
{
    public partial class DrawDownRetirementPlannerCalendar
    {
        // Inputs
        public int CurrentAgeYou { get; set; } = 63;
        public int CurrentAgePartner { get; set; } = 60;

        public int RetirementYearYou { get; set; } = DateTime.Now.Year + 5;
        public int RetirementYearPartner { get; set; } = DateTime.Now.Year + 8;

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
        public decimal Inflation { get; set; } = 2.5m;

        // Withdrawals
        public decimal AnnualWithdrawalOne { get; set; } = 80_000m;
        public decimal AnnualWithdrawalBoth { get; set; } = 100_000m;
        public int ReverseMortgageStartYear { get; set; }
        public decimal ReverseMortgageMonthly { get; set; }

        public int PartialRetirementStart { get; set; }
        public int PartialRetirementEnd { get; set; }
        public decimal PartTimeIncome { get; set; }

        // Results
        public List<YearRow> YearRows { get; set; } = new();

        protected override Task OnInitializedAsync()
        {
            // default simulation start year
            SimulationStartYear = Math.Min(RetirementYearYou, RetirementYearPartner);
            Calculate();
            return base.OnInitializedAsync();
        }

        public async Task Save()
        {
            // Build a minimal SpendingPlanModel from these inputs so it can be persisted via existing API
            var model = new SpendingPlanModel
            {
                RetirementAge = Math.Max(60, 65),
                LifeExpectancy = Math.Max(LifeExpectancyYou, LifeExpectancyPartner),
                TaxableBalance = TaxableBalance,
                TraditionalBalance = TraditionalBalance,
                RothBalance = RothBalance,
                TraditionalTaxRate = TraditionalTaxRate,
                AnnualWithdrawal = AnnualWithdrawalBoth,
                InflationRate = Inflation,
                InvestmentReturn = InvestmentReturn,
                WithdrawalPercentage = 4.0m
            };

            var apiBaseUrl = GetApiBaseUrl();
            var url = $"{apiBaseUrl}/api/Retirement/save";

            try
            {
                var saveRequest = new PersistSpendingRequest
                {
                    UserGuid = await UserGuidService.GetOrCreateUserGuidAsync(),
                    CalculatorType = "CalendarWireframe",
                    Data = model
                };

                var json = System.Text.Json.JsonSerializer.Serialize(saveRequest);
                using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var client = HttpCustomClientService.CreateRetryClient(HttpClientFactory);
                var response = await client.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    DebugService.AddMessage("Calendar saved");
                }
                else
                {
                    DebugService.AddMessage($"Calendar save failed: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                DebugService.AddMessage($"Calendar save error: {ex.Message}");
            }
        }

        public void Calculate()
        {
            YearRows.Clear();

            var start = SimulationStartYear;
            var end = Math.Max(LifeExpectancyYou, LifeExpectancyPartner);

            decimal taxBal = TaxableBalance;
            decimal tradBal = TraditionalBalance;
            decimal rothBal = RothBalance;

            for (int y = start; y <= end; y++)
            {
                var row = new YearRow { Year = y };
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

                // withdrawal policy: before both retired use One retired value, after both retired use Both
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

                // ending balances
                taxBal += row.Growth * 0.4m; // simplistic distribution of growth
                tradBal += row.Growth * 0.4m;
                rothBal += row.Growth * 0.2m;

                row.EndingTaxable = taxBal;
                row.EndingTraditional = tradBal;
                row.EndingRoth = rothBal;

                // Notes
                if (taxBal + tradBal + rothBal <= 0) row.Notes = "Money depleted";

                YearRows.Add(row);
            }
        }

        public async Task Load()
        {
            try
            {
                var userGuid = await UserGuidService.GetOrCreateUserGuidAsync();
                var apiBaseUrl = GetApiBaseUrl();
                var url = $"{apiBaseUrl}/api/Retirement/load?userGuid={userGuid}&calculatorType=CalendarWireframe";
                var client = HttpCustomClientService.CreateRetryClient(HttpClientFactory);
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    DebugService.AddMessage("Calendar load failed");
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                var loaded = System.Text.Json.JsonSerializer.Deserialize<SpendingPlanModel>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (loaded != null)
                {
                    TaxableBalance = loaded.TaxableBalance;
                    TraditionalBalance = loaded.TraditionalBalance;
                    RothBalance = loaded.RothBalance;
                    TraditionalTaxRate = loaded.TraditionalTaxRate;
                    InvestmentReturn = loaded.InvestmentReturn;
                    Inflation = loaded.InflationRate;
                    AnnualWithdrawalBoth = loaded.AnnualWithdrawal;
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                DebugService.AddMessage($"Calendar load error: {ex.Message}");
            }
        }

        public class YearRow
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

        private string GetApiBaseUrl()
        {
#if DEBUG
            return Configuration["FinPlanSettings:ApiBaseUrlLocal"] ?? "https://localhost:7330";
#else
            return Configuration["FinPlanSettings:ApiBaseUrlCloud"] ?? "api-money-amperespark-bnbva5h5g6gme6fm.eastus2-01.azurewebsites.net";
#endif
        }
    }
}
