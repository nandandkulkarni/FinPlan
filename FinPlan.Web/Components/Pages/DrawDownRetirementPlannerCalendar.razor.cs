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
    public partial class DrawDownRetirementPlannerCalendar : IDisposable
    {
        // inject services
        [Inject] public DebugMessageService DebugService { get; set; } = default!;
        [Inject] public UserGuidService UserGuidService { get; set; } = default!;
        [Inject] public IHttpClientFactory HttpClientFactory { get; set; } = default!;
        [Inject] public IConfiguration Configuration { get; set; } = default!;

        // model instance used for both save/load and calculation
        public CalendarSpendingModel Model { get; set; } = new();

        // UI toggle
        public bool ShowRightDebug { get; set; } = false;
        public void ToggleRightDebug() => ShowRightDebug = !ShowRightDebug;

        // Results proxy for the grid binds to Model.YearRows
        public List<CalendarYearRow> YearRows => Model.YearRows;

        // track first render
        private bool hasInitialized = false;

        // debounce timer for auto-calc
        private System.Timers.Timer? debounceTimer;
        private const double DebounceMs = 300; // debounce period agreed

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !hasInitialized)
            {
                hasInitialized = true;
                try
                {
                    DebugService.AddMessage("Calendar load started");
                }
                catch { }

                await Load();
                StateHasChanged();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        protected override Task OnInitializedAsync()
        {
            // initialize debounce timer
            debounceTimer = new System.Timers.Timer(DebounceMs) { AutoReset = false };
            debounceTimer.Elapsed += async (_, __) =>
            {
                // run calculate on UI thread if auto-calc enabled
                await InvokeAsync(() =>
                {
                    if (Model.AutoCalculate)
                    {
                        Model.Calculate();
                    }
                    StateHasChanged();
                });
            };

            // initialize model defaults from existing defaults
            Model.SimulationStartYear = Math.Min(Model.RetirementYearYou, Model.RetirementYearPartner);
            Model.Calculate();
            return base.OnInitializedAsync();
        }

        public async Task Save()
        {
            var apiBaseUrl = GetApiBaseUrl();
            var url = $"{apiBaseUrl}/api/Retirement/save";

            try
            {
                var saveRequest = new PersistCalendarSpendingRequest
                {
                    UserGuid = await UserGuidService.GetOrCreateUserGuidAsync(),
                    CalculatorType = "CalendarWireframe",
                    Data = Model
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
            Model.Calculate();
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
                    DebugService.AddMessage("load failed");
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                // The API now stores CalendarSpendingModel directly — deserialize
                var stored = System.Text.Json.JsonSerializer.Deserialize<CalendarSpendingModel>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (stored != null)
                {
                    Model = stored;
                    Model.Calculate();
                    DebugService.AddMessage("loaded");
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                DebugService.AddMessage($"load error: {ex.Message}");
            }
        }

        // Called on many input changes; debounces Calculate when AutoCalculate is enabled
        private void OnInputChanged()
        {
            // always keep retirement years in sync for immediate display
            Model.SyncRetirementYearsFromAges();
            StateHasChanged();

            // if auto-calc is enabled, debounce a Calculate call
            if (Model.AutoCalculate && debounceTimer != null)
            {
                debounceTimer.Stop();
                debounceTimer.Start();
            }
        }

        private string GetApiBaseUrl()
        {
#if DEBUG
            return Configuration["FinPlanSettings:ApiBaseUrlLocal"] ?? "https://localhost:7330";
#else
            return Configuration["FinPlanSettings:ApiBaseUrlCloud"] ?? "api-money-amperespark-bnbva5h5g6gme6fm.eastus2-01.azurewebsites.net";
#endif
        }

        public void Dispose()
        {
            try { debounceTimer?.Dispose(); } catch { }
        }
    }
}
