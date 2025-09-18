using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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
    public partial class Retire : IDisposable
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

        // track last save time to avoid rapid duplicate saves
        private DateTime _lastSave = DateTime.MinValue;
        private readonly TimeSpan _minSaveInterval = TimeSpan.FromMilliseconds(300);
        private bool _isDataAvaiableForTheUser= false;

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

        // Save now returns success flag so callers can display result
        public async Task<bool> Save()
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
                    _isDataAvaiableForTheUser = true;
                    return true;
                }
                else
                {
                    DebugService.AddMessage($"Calendar save failed: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                DebugService.AddMessage($"Calendar save error: {ex.Message}");
                return false;
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
                    _isDataAvaiableForTheUser= true;
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                DebugService.AddMessage($"load error: {ex.Message}");
            }
        }

        // New: Clear server-side saved plan and reset local model, then open wizard for re-entry
        // Returns true if overall operation succeeded (delete + save), false otherwise
        public async Task<bool> ClearAllAndOpenWizardAsync()
        {
            var deleteSuccess = false;
            try
            {
                var userGuid = await UserGuidService.GetOrCreateUserGuidAsync();
                var apiBaseUrl = GetApiBaseUrl();
                var url = $"{apiBaseUrl}/api/Retirement/delete?userGuid={userGuid}&calculatorType=CalendarWireframe";

                var client = HttpCustomClientService.CreateRetryClient(HttpClientFactory);
                var response = await client.DeleteAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    DebugService.AddMessage("Calendar deleted on server.");
                    deleteSuccess = true;
                }
                else
                {
                    DebugService.AddMessage($"Calendar delete returned: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                DebugService.AddMessage($"delete error: {ex.Message}");
            }

            // Reset local model to a clean instance and open the wizard for quick re-entry
            Model = new CalendarSpendingModel();
            Model.Calculate();
            _isDataAvaiableForTheUser = true;
            StateHasChanged();

            // Also save the cleared state so server and client are in sync
            var saveSuccess = false;
            try { saveSuccess = await Save(); } catch { saveSuccess = false; }

            // Consider operation successful only if delete and save both succeeded
            return deleteSuccess && saveSuccess;
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

        // Called when focus leaves the panel (bubbling from inputs)
        private async Task HandleFocusOut(FocusEventArgs e)
        {
            // debounce quick focus changes
            if (DateTime.UtcNow - _lastSave < _minSaveInterval) return;
            _lastSave = DateTime.UtcNow;
            await Save();
        }

        // Called when key pressed inside panel; save on Enter
        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e == null) return;
            if (e.Key == "Enter")
            {
                // prevent multiple saves if already recently saved
                if (DateTime.UtcNow - _lastSave < _minSaveInterval) return;
                _lastSave = DateTime.UtcNow;
                await Save();
            }
        }

        // Handler invoked when the RetirementInputWizard finishes
        public async Task HandleWizardFinished((int AgeYou, int AgePartner) ages)
        {
            try
            {
                // Apply ages to model and sync years
                Model.CurrentAgeYou = ages.AgeYou > 0 ? ages.AgeYou : Model.CurrentAgeYou;
                Model.CurrentAgePartner = ages.AgePartner > 0 ? ages.AgePartner : Model.CurrentAgePartner;
                Model.SyncRetirementYearsFromAges();

                // hide the modal flag
                _isDataAvaiableForTheUser = false;

                // Recalculate and save
                Model.Calculate();
                await Save();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                try { DebugService.AddMessage($"Wizard finish error: {ex.Message}"); } catch { }
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
