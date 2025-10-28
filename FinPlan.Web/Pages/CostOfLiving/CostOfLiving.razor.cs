using FinPlan.Shared.Models.LivingCosts;
using FinPlan.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Timers;

namespace FinPlan.Web.Pages.CostOfLiving;

public partial class CostOfLiving : ComponentBase, IDisposable
{
    [Inject]
    private IHttpClientFactory HttpClientFactory { get; set; } = default!;

    [Inject]
    private IConfiguration Configuration { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private UserGuidService UserGuidService { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private ApiUrlProvider ApiUrlProvider { get; set; } = default!;

    //public void Dispose()
    //{
    //    if (autosaveTimer != null)
    //    {
    //        try { autosaveTimer.Dispose(); }
    //        catch { }
    //        autosaveTimer = null;
    //    }

    //    if (undoTimer != null)
    //    {
    //        try { undoTimer.Dispose(); }
    //        catch { }
    //        undoTimer = null;
    //    }
    //}

    // Simple Mode Toggle
    private bool isSimpleMode = true; // Start in beginner mode by default
    private bool showProfileSelector = false;

    private void ToggleSimpleMode()
    {
        isSimpleMode = !isSimpleMode;
        StateHasChanged();
    }

    private void ToggleProfileSelector()
    {
        showProfileSelector = !showProfileSelector;
        StateHasChanged();
    }



    private async Task ClearDataAsync()
    {
        try
        {
            // Confirm with user before deleting
            var confirm = await JSRuntime.InvokeAsync<bool>("confirm",
                "Are you sure you want to clear all saved cost of living data? This action cannot be undone.");

            if (!confirm) return;

            var apiBaseUrl = this.ApiUrlProvider.GetApiBaseUrl();

            // Create request body for POST method instead of query parameters
            var deleteRequest = new
            {
                UserGuid = userGuid,
                CalculatorType = calculatorType
            };

            var json = System.Text.Json.JsonSerializer.Serialize(deleteRequest);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var client = HttpCustomClientService.CreateRetryClient(HttpClientFactory);
            // Use POST to clear endpoint instead of DELETE with query parameters
            var response = await client.PostAsync($"{apiBaseUrl}/api/FinPlan/remove", content);

            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Clear the "don't show intro again" localStorage setting
                await JSRuntime.InvokeVoidAsync("localStorage.removeItem", "finplan-cost-of-living-income-planner-hide-intro");

                Log("Plan data cleared successfully.");

                // Reload the page to reflect the cleared state
                Navigation.NavigateTo(Navigation.Uri, forceLoad: true);
            }
            else
            {
                LogError($"Failed to clear data: {response.StatusCode}");
                await JSRuntime.InvokeVoidAsync("alert", $"Failed to clear data: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            LogError($"Error clearing data: {ex.Message}");
            await JSRuntime.InvokeVoidAsync("alert", $"Error clearing data: {ex.Message}");
        }
    }

    // Ghost tab creation modal state

    // Returns true if the Add Category button should be disabled
    private bool IsAddCategoryDisabled => false;//string.IsNullOrWhiteSpace(NewCategoryName?.Trim());

    private bool showCreateTabModal = false;
    private string? pendingGhostTab = null;

    private void PromptCreateTab(string ghostTab)
    {
        pendingGhostTab = ghostTab;
        showCreateTabModal = true;
    }

    private async Task ConfirmCreateTab()
    {
        showCreateTabModal = false;
        pendingGhostTab = null;
        await AddNewTab();
    }

    private void CancelCreateTab()
    {
        showCreateTabModal = false;
        pendingGhostTab = null;
    }

    // Edit Item Modal state
    // Details Modal State
    private bool showDetailsModal = false;
    private bool detailsEditMode = false;
    private int detailsItemIndex = -1;
    private string modalSubcategory = string.Empty;
    private string modalFrequency = "Monthly";
    private decimal modalCurrentValue = 0m;
    private string modalAdjustOption = "Same";
    private decimal modalCustomPercentage = 0m;
    private decimal? modalManualRetirementValue = null;
    private string modalPerItemInflationSource = "UseGlobal";
    private decimal modalPerItemInflationPercent = 0m;
    private bool modalIncludeInRetirement = true;

    private void OpenViewModal(int idx)
    {
        if (idx >= 0 && idx < Items.Count)
        {
            detailsItemIndex = idx;
            // Copy values to modal fields
            modalSubcategory = Items[idx].Subcategory ?? string.Empty;
            modalFrequency = Items[idx].Frequency.ToString();
            modalCurrentValue = Items[idx].CurrentValue;
            modalAdjustOption = Items[idx].AdjustOption.ToString();
            modalCustomPercentage = Items[idx].CustomPercentage;
            modalManualRetirementValue = Items[idx].ManualRetirementValue;
            modalPerItemInflationSource = Items[idx].PerItemInflationSource.ToString();
            modalPerItemInflationPercent = Items[idx].PerItemInflationPercent ?? 0;
            modalIncludeInRetirement = Items[idx].IncludeInRetirement;
            showDetailsModal = true;
            detailsEditMode = true; // Always open in edit mode
        }
    }

    private async Task HandleIntroModal()
    {
        showIntroModal = false;

        // Only show intro modal if model is empty and user hasn't opted out
        if (Items == null || !Items.Any())
        {
            try
            {
                var hideIntro = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "finplan-cost-of-living-income-planner-hide-intro");
                showIntroModal = string.IsNullOrEmpty(hideIntro) || !hideIntro.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                // Ignore errors and show modal
                showIntroModal = false;
            }
        }
    }

    private async Task OnDontShowIntroChanged(ChangeEventArgs e)
    {
        bool checkedState = e?.Value is bool b && b;
        string key = "finplan-cost-of-living-income-planner-hide-intro";

        if (checkedState)
        {
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", key, "true");
        }
        else
        {
            await JSRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
    }

    // Add these new methods
    private void CloseIntroModal()
    {
        showIntroModal = false;
        StateHasChanged();
    }

    // Onboarding Methods for Simple Mode
    private async Task StartWithCommonExpenses()
    {
        // Load preset common expense categories
        Items = GetCommonExpensePreset();
        RecalculateAll(save: true);
        await InvokeAsync(StateHasChanged);
    }

    private async Task StartFromScratch()
    {
        // Clear any existing items and let user build from scratch
        Items = new List<CostItem>();
        showIntroModal = false; // Add this line to close the modal

        RecalculateAll(save: true);
        await InvokeAsync(StateHasChanged);
    }

    private List<CostItem> GetCommonExpensePreset()
    {
        return new List<CostItem>
        {
            // Housing
            new CostItem { Category = "Housing", Subcategory = "Rent/Mortgage", Frequency = FinPlan.Shared.Models.LivingCosts.Frequency.Monthly, CurrentValue = 2000, IncludeInRetirement = true, AdjustOption = RetirementAdjustOption.Same },
            new CostItem { Category = "Housing", Subcategory = "Utilities", Frequency = FinPlan.Shared.Models.LivingCosts.Frequency.Monthly, CurrentValue = 200, IncludeInRetirement = true, AdjustOption = RetirementAdjustOption.Same },

            // Food
            new CostItem { Category = "Food", Subcategory = "Groceries", Frequency = FinPlan.Shared.Models.LivingCosts.Frequency.Monthly, CurrentValue = 600, IncludeInRetirement = true, AdjustOption = RetirementAdjustOption.Same },
            new CostItem { Category = "Food", Subcategory = "Dining Out", Frequency = FinPlan.Shared.Models.LivingCosts.Frequency.Monthly, CurrentValue = 300, IncludeInRetirement = true, AdjustOption = RetirementAdjustOption.CustomPercentage, CustomPercentage = 75 },

            // Transportation
            new CostItem { Category = "Transportation", Subcategory = "Car Payment", Frequency = FinPlan.Shared.Models.LivingCosts.Frequency.Monthly, CurrentValue = 400, IncludeInRetirement = false },
            new CostItem { Category = "Transportation", Subcategory = "Gas & Maintenance", Frequency = FinPlan.Shared.Models.LivingCosts.Frequency.Monthly, CurrentValue = 200, IncludeInRetirement = true, AdjustOption = RetirementAdjustOption.CustomPercentage, CustomPercentage = 50 },

            // Healthcare
            new CostItem { Category = "Healthcare", Subcategory = "Insurance Premiums", Frequency = FinPlan.Shared.Models.LivingCosts.Frequency.Monthly, CurrentValue = 300, IncludeInRetirement = true, AdjustOption = RetirementAdjustOption.Inflation, PerItemInflationSource = InflationSource.Custom, PerItemInflationPercent = 4 },

            // Personal
            new CostItem { Category = "Personal", Subcategory = "Entertainment", Frequency = FinPlan.Shared.Models.LivingCosts.Frequency.Monthly, CurrentValue = 200, IncludeInRetirement = true, AdjustOption = RetirementAdjustOption.Same },
            new CostItem { Category = "Personal", Subcategory = "Clothing", Frequency = FinPlan.Shared.Models.LivingCosts.Frequency.Monthly, CurrentValue = 100, IncludeInRetirement = true, AdjustOption = RetirementAdjustOption.CustomPercentage, CustomPercentage = 75 }
        };
    }


    private void CloseDetailsModal()
    {
        showDetailsModal = false;
        detailsEditMode = false;
        detailsItemIndex = -1;
    }

    private void EnableDetailsEdit()
    {
        detailsEditMode = true;
    }

    private void SaveDetailsModal()
    {
        if (detailsItemIndex >= 0 && detailsItemIndex < Items.Count)
        {
            var item = Items[detailsItemIndex];
            item.Subcategory = modalSubcategory;
            if (Enum.TryParse<FinPlan.Shared.Models.LivingCosts.Frequency>(modalFrequency, out var freq))
                item.Frequency = freq;
            item.CurrentValue = modalCurrentValue;
            if (Enum.TryParse<RetirementAdjustOption>(modalAdjustOption, out var adj))
                item.AdjustOption = adj;
            item.CustomPercentage = modalCustomPercentage;
            item.ManualRetirementValue = modalManualRetirementValue;
            if (Enum.TryParse<InflationSource>(modalPerItemInflationSource, out var infl))
                item.PerItemInflationSource = infl;
            item.PerItemInflationPercent = modalPerItemInflationPercent;
            item.IncludeInRetirement = modalIncludeInRetirement;
            StateHasChanged();
            ScheduleAutosave();
        }
        CloseDetailsModal();
    }
    private List<CostItem> Items { get; set; } = new List<CostItem>(); // = StandardCostCategories.GetDefaults();

    // City Template properties
    private List<CityTemplate>? availableCities;
    private CityTemplate? selectedCity;
    private List<DemographicProfile>? cityProfiles;
    private DemographicProfile? matchedProfile;
    private bool showCitySelector = false;
    private UserDemographics? userDemographics;

    private int YearsToRetirement { get; set; } = 20;
    // Backing field and property so changes trigger recalculation and autosave
    private decimal _inflationRate = 2.5m;
    private decimal InflationRate
    {
        get => _inflationRate;
        set
        {
            if (_inflationRate != value)
            {
                _inflationRate = value;
                RecalculateAll(save: true);
            }
        }
    }

    private bool isLoading = false;

    // Saving state for UI
    private bool isSaving = false;
    private string lastSaveMessage = string.Empty;
    private string saveErrorMessage = string.Empty;

    // Only dynamic tabs; start with a single default plan (plan-a)
    private string activeTab = "Plan-A-City-A"; // Track active tab for UI highlighting

    private List<string> dynamicTabs = new List<string>();
    // Always include the built-in "plan-a" tab first so UI shows a default tab when no dynamic tabs exist.
    private IEnumerable<string> Tabs
    {
        get
        {
            // Ensure TabHeaders has default for built-in tab
            if (!TabHeaders.ContainsKey("Plan-A-City-A")) TabHeaders["Plan-A-City-A"] = "Plan - A";

            // Return plan-a first, then any dynamic tabs (excluding duplicates)
            var result = new List<string> { "Plan-A-City-A" };
            foreach (var t in dynamicTabs)
            {
                if (!string.Equals(t, "Plan-A-City-A", StringComparison.OrdinalIgnoreCase) && !result.Contains(t)) result.Add(t);
            }
            return result;
        }
    }

    private Dictionary<string, string> TabHeaders = new Dictionary<string, string>();

    // Lightweight fpDebug helpers - non-blocking
    private void Log(string message)
    {
        try { _ = JSRuntime.InvokeVoidAsync("fpDebug.log", $"CostOfLiving: {message}"); } catch { }
    }
    private void LogError(string message)
    {
        try { _ = JSRuntime.InvokeVoidAsync("fpDebug.error", $"CostOfLiving ERROR: {message}"); } catch { }
    }

    private string GetTabLabel(string tab)
    {
        if (TabHeaders.ContainsKey(tab) && !string.IsNullOrWhiteSpace(TabHeaders[tab])) return TabHeaders[tab];
        // fallback: convert id to nicer label
        if (string.Equals(tab, "Plan-A-City-A", StringComparison.OrdinalIgnoreCase)) return "Plan-A-City-A";
        return tab;
    }

    private HashSet<string> editingTabs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    private bool IsEditing(string tab) => editingTabs.Contains(tab);
    private void ToggleEdit(string tab)
    {
        if (editingTabs.Contains(tab)) editingTabs.Remove(tab);
        else
        {
            if (!TabHeaders.ContainsKey(tab)) TabHeaders[tab] = GetTabLabel(tab);
            editingTabs.Add(tab);
        }
    }

    private void CancelEdit(string tab)
    {
        if (TabHeaders.ContainsKey(tab) == false)
            TabHeaders[tab] = GetTabLabel(tab);
        editingTabs.Remove(tab);
    }

    private void HandleHeaderKey(KeyboardEventArgs e, string tab)
    {
        if (e.Key == "Enter")
        {
            _ = SaveTabHeader(tab);
        }
        else if (e.Key == "Escape")
        {
            CancelEdit(tab);
        }
    }

    private void HandleTabKey(KeyboardEventArgs e, string tab)
    {
        if (e == null) return;

        if (e.Key == "ArrowLeft" || e.Key == "ArrowRight")
        {
            var list = Tabs.ToList();
            if (list.Count <= 1) return;
            var idx = list.IndexOf(tab);
            if (idx < 0) idx = 0;
            idx = e.Key == "ArrowLeft" ? (idx - 1 + list.Count) % list.Count : (idx + 1) % list.Count;
            var next = list[idx];
            // navigate visually and load data
            _ = OnTabClick(next);
        }
        else if (e.Key == "Enter" || e.Key == " ")
        {
            _ = OnTabClick(tab);
        }
    }

    private string MapTabToCalculatorType(string tab)
    {
        // All tabs map to CostOfLiving-{tabId}
        return $"CostOfLiving-{tab}";
    }

    private async Task SaveTabHeader(string tab)
    {
        Log($"SaveTabHeader start: {tab}");
        // Load existing data so we don't overwrite items when updating header
        try
        {
            var calcType = MapTabToCalculatorType(tab);
            var apiBaseUrl = this.ApiUrlProvider.GetApiBaseUrl();
            var client = HttpCustomClientService.CreateRetryClient(HttpClientFactory);
            var loadUrl = $"{apiBaseUrl}/api/CostOfLiving/load?userGuid={Uri.EscapeDataString(userGuid)}&calculatorType={Uri.EscapeDataString(calcType)}";
            CostOfLivingData? data = null;

            try
            {
                var resp = await client.GetAsync(loadUrl);
                Log($"SaveTabHeader: load call returned {(int)resp.StatusCode}");
                if (resp.IsSuccessStatusCode)
                {
                    var j = await resp.Content.ReadAsStringAsync();
                    data = System.Text.Json.JsonSerializer.Deserialize<CostOfLivingData>(j, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }
            catch (Exception exLoad)
            {
                LogError($"SaveTabHeader load error: {exLoad.Message}");
            }

            if (data == null)
            {
                // No existing saved data - use defaults
                data = new CostOfLivingData
                {
                    Items = StandardCostCategories.GetDefaults(),
                    YearsToRetirement = YearsToRetirement,
                    InflationRate = InflationRate,
                    CollapsedCategories = collapsed.ToList()
                };
            }

            data.Header = TabHeaders.ContainsKey(tab) ? TabHeaders[tab] : GetTabLabel(tab);

            var dto = new PersistCostOfLivingRequest
            {
                UserGuid = userGuid,
                CalculatorType = calcType,
                Data = data
            };

            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var saveUrl = $"{apiBaseUrl}/api/CostOfLiving/save";
            var saveResp = await client.PostAsync(saveUrl, content);
            Log($"SaveTabHeader: save returned {(int)saveResp.StatusCode}");
            if (saveResp.IsSuccessStatusCode)
            {
                editingTabs.Remove(tab);
                await LoadTabsAsync();
                StateHasChanged();
                Log($"SaveTabHeader success: {tab}");
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("console.error", $"Save header failed: {saveResp.StatusCode}");
                LogError($"SaveTabHeader failed status: {saveResp.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            await JSRuntime.InvokeVoidAsync("console.error", $"Error saving header: {ex.Message}");
            LogError($"SaveTabHeader exception: {ex.Message}");
        }
    }

    private bool isAddingTab = false;
    private bool isDeletingPlans = false;

    // Confirm deletion of all saved dynamic plans for the user
    private async Task ConfirmDeleteAll()
    {
        try
        {
            var ok = await JSRuntime.InvokeAsync<bool>("confirm", "Delete ALL saved plans for this user? This cannot be undone.");
            if (!ok) return;
            await DeleteAllPlans();
        }
        catch (Exception ex)
        {
            LogError($"ConfirmDeleteAll exception: {ex.Message}");
        }
    }

    // Delete each dynamic tab for the current user by calling the existing DeleteTab API endpoint
    private async Task DeleteAllPlans()
    {
        Log("DeleteAllPlans start");
        if (isDeletingPlans) return;
        isDeletingPlans = true;
        isSaving = true; // show saving indicator to user while deleting
        await InvokeAsync(StateHasChanged);

        try
        {
            var apiBaseUrl = this.ApiUrlProvider.GetApiBaseUrl();
            var client = HttpCustomClientService.CreateRetryClient(HttpClientFactory);

            // Make a copy of dynamicTabs to avoid mutation during enumeration
            var tabsToDelete = dynamicTabs.ToList();
            foreach (var tab in tabsToDelete)
            {
                try
                {
                    var calcType = MapTabToCalculatorType(tab);
                    var url = $"{apiBaseUrl}/api/CostOfLiving/tabs?userGuid={Uri.EscapeDataString(userGuid)}&calculatorType={Uri.EscapeDataString(calcType)}";
                    var resp = await client.DeleteAsync(url);
                    Log($"DeleteAllPlans: delete {tab} returned {(int)resp.StatusCode}");
                }
                catch (Exception ex)
                {
                    LogError($"DeleteAllPlans delete error for {tab}: {ex.Message}");
                }
            }

            // Clear local state for dynamic tabs and headers
            foreach (var t in tabsToDelete)
            {
                dynamicTabs.Remove(t);
                if (TabHeaders.ContainsKey(t)) TabHeaders.Remove(t);
            }

            // Reload tabs and select default
            await LoadTabsAsync();
            activeTab = Tabs.FirstOrDefault() ?? "Plan-A-City-A";
            calculatorType = MapTabToCalculatorType(activeTab);
            await LoadFromApi();

            Log("DeleteAllPlans completed");
        }
        catch (Exception ex)
        {
            LogError($"DeleteAllPlans exception: {ex.Message}");
        }
        finally
        {
            isDeletingPlans = false;
            isSaving = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private static string NumberToLetters(int number)
    {
        // Convert 1 -> A, 26 -> Z, 27 -> AA, etc.
        var sb = new System.Text.StringBuilder();
        while (number > 0)
        {
            number--; // 1-based
            int rem = number % 26;
            sb.Insert(0, (char)('A' + rem));
            number = number / 26;
        }
        return sb.ToString();
    }

    private async Task AddNewTab()
    {
        Log("AddNewTab start");
        if (isAddingTab) return; // guard re-entrancy
        isAddingTab = true;
        try
        {
            int totalTabs = Tabs.Count();
            int candidateIndex = totalTabs + 1;
            if (!TabHeaders.ContainsKey("Plan-A-City-A")) TabHeaders["Plan-A-City-A"] = "Plan-A-City-A";
            var existingNames = new HashSet<string>(TabHeaders.Values, StringComparer.OrdinalIgnoreCase);

            string headerName;
            do
            {
                var letters = NumberToLetters(candidateIndex);
                headerName = selectedCity != null
                    ? $"Plan-{letters}-{selectedCity.CityName}"
                    : $"Plan-{letters}-City-{letters}";
                candidateIndex++;
            } while (existingNames.Contains(headerName));

            var newTab = $"tab-{DateTime.Now:yyMMddHHmmss}";

            // Use city template if selected and matchedProfile exists
            List<CostItem> initialItems;
            if (selectedCity != null && matchedProfile != null && matchedProfile.SampleExpenses != null && matchedProfile.SampleExpenses.Any())
            {
                initialItems = matchedProfile.SampleExpenses.Select(e => new CostItem
                {
                    Category = e.Category,
                    Subcategory = e.Subcategory,
                    CurrentValue = e.CurrentValue,
                    Frequency = e.Frequency,
                    AdjustOption = e.AdjustOption,
                    PerItemInflationPercent = e.PerItemInflationPercent,
                    PerItemInflationSource = e.PerItemInflationSource,
                    CustomPercentage = e.CustomPercentage,
                    ManualRetirementValue = e.ManualRetirementValue,
                    IncludeInRetirement = e.IncludeInRetirement,
                    RetirementExclusionReason = e.RetirementExclusionReason
                }).ToList();
            }
            else
            {
                initialItems = StandardCostCategories.GetDefaults();
            }

            var initialData = new CostOfLivingData
            {
                Header = headerName,
                Items = initialItems,
                YearsToRetirement = YearsToRetirement,
                InflationRate = InflationRate,
                CollapsedCategories = collapsed.ToList()
            };

            var dto = new PersistCostOfLivingRequest
            {
                UserGuid = userGuid,
                CalculatorType = $"CostOfLiving-{newTab}",
                Data = initialData
            };

            var apiBaseUrl = this.ApiUrlProvider.GetApiBaseUrl();
            var client = HttpCustomClientService.CreateRetryClient(HttpClientFactory);
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var saveUrl = $"{apiBaseUrl}/api/CostOfLiving/save";
            var resp = await client.PostAsync(saveUrl, content);
            Log($"AddNewTab: save returned {(int)resp.StatusCode}");
            if (resp.IsSuccessStatusCode)
            {
                await LoadTabsAsync();
                TabHeaders[newTab] = headerName;
                if (!dynamicTabs.Contains(newTab)) dynamicTabs.Add(newTab);
                await OnTabClick(newTab);
                StateHasChanged();
                Log($"AddNewTab success: {newTab} / {headerName}");
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("console.error", $"Create tab failed: {resp.StatusCode}");
                LogError($"AddNewTab failed status: {resp.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            await JSRuntime.InvokeVoidAsync("console.error", $"Error creating tab: {ex.Message}");
            LogError($"AddNewTab exception: {ex.Message}");
        }
        finally
        {
            isAddingTab = false;
        }
    }

    private async Task ConfirmDeleteTab(string tab)
    {
        try
        {
            var header = TabHeaders.ContainsKey(tab) ? TabHeaders[tab] : tab;
            var ok = await JSRuntime.InvokeAsync<bool>("confirm", $"Delete tab '{header}'? This will remove saved data.");
            if (ok) await DeleteTab(tab);
        }
        catch (Exception ex)
        {
            LogError($"ConfirmDeleteTab exception: {ex.Message}");
        }
    }

    private async Task DeleteTab(string tab)
    {
        Log($"DeleteTab start: {tab}");
        try
        {
            var calcType = MapTabToCalculatorType(tab);
            var apiBaseUrl = this.ApiUrlProvider.GetApiBaseUrl();
            var url = $"{apiBaseUrl}/api/CostOfLiving/tabs?userGuid={Uri.EscapeDataString(userGuid)}&calculatorType={Uri.EscapeDataString(calcType)}";
            var client = HttpCustomClientService.CreateRetryClient(HttpClientFactory);
            var response = await client.DeleteAsync(url);
            Log($"DeleteTab: delete returned {(int)response.StatusCode}");
            if (response.IsSuccessStatusCode)
            {
                // remove locally
                dynamicTabs.Remove(tab);
                if (TabHeaders.ContainsKey(tab)) TabHeaders.Remove(tab);

                // reload tabs
                await LoadTabsAsync();

                // if deleted tab was active, switch to first available
                if (activeTab == tab)
                {
                    activeTab = Tabs.FirstOrDefault() ?? "Plan-A-City-A";
                    calculatorType = MapTabToCalculatorType(activeTab);
                    await LoadFromApi();
                }

                StateHasChanged();
                Log($"DeleteTab success: {tab}");
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("console.error", $"Delete failed: {response.StatusCode}");
                LogError($"DeleteTab failed status: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            await JSRuntime.InvokeVoidAsync("console.error", $"Error deleting tab: {ex.Message}");
            LogError($"DeleteTab exception: {ex.Message}");
        }
    }

    private async Task LoadTabsAsync()
    {
        Log("LoadTabsAsync start");
        try
        {
            var apiBaseUrl = this.ApiUrlProvider.GetApiBaseUrl();
            var url = $"{apiBaseUrl}/api/CostOfLiving/tabs?userGuid={Uri.EscapeDataString(userGuid)}";
            var client = HttpCustomClientService.CreateRetryClient(HttpClientFactory);
            // reduce timeout for this non-critical call so UI can proceed if API is slow
            try { client.Timeout = TimeSpan.FromSeconds(10); } catch { }
            var response = await client.GetAsync(url);
            Log($"LoadTabsAsync: tabs call returned {(int)response.StatusCode}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var list = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (list != null)
                {
                    dynamicTabs = list.Select(s => s.StartsWith("CostOfLiving-") ? s.Substring("CostOfLiving-".Length) : s).ToList();

                    // ensure headers dictionary has defaults for built-in tabs
                    if (!TabHeaders.ContainsKey("Plan-A-City-A")) TabHeaders["Plan-A-City-A"] = "Plan-A-City-A";

                    // load headers for each dynamic tab
                    foreach (var t in dynamicTabs)
                    {
                        var dataUrl = $"{apiBaseUrl}/api/CostOfLiving/load?userGuid={Uri.EscapeDataString(userGuid)}&calculatorType={Uri.EscapeDataString($"CostOfLiving-{t}")}";
                        var resp = await client.GetAsync(dataUrl);
                        Log($"LoadTabsAsync: load header for {t} returned {(int)resp.StatusCode}");
                        if (resp.IsSuccessStatusCode)
                        {
                            var j = await resp.Content.ReadAsStringAsync();
                            try
                            {
                                var d = System.Text.Json.JsonSerializer.Deserialize<CostOfLivingData>(j, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                                if (d != null) TabHeaders[t] = d.Header ?? t;
                            }
                            catch (Exception ex)
                            {
                                TabHeaders[t] = t;
                                LogError($"LoadTabsAsync header parse failed for {t}: {ex.Message}");
                            }
                        }
                        else
                        {
                            TabHeaders[t] = t;
                        }
                    }

                    Log($"LoadTabsAsync success: {dynamicTabs.Count} tabs");
                }
            }
        }
        catch (OperationCanceledException oce)
        {
            // Timeout or cancellation - continue with defaults
            LogError($"LoadTabsAsync timeout/cancelled: {oce.Message}");
            dynamicTabs = new List<string>();
            if (!TabHeaders.ContainsKey("Plan-A-City-A")) TabHeaders["Plan-A-City-A"] = "Plan-A-City-A";
            // Show modal to allow retry
            ShowTimeout(OperationType.LoadTabs, "Loading tabs timed out. You can retry.");
        }
        catch (Exception ex)
        {
            LogError($"LoadTabsAsync exception: {ex.Message}");
            // ensure we have at least the default tab so UI remains usable
            dynamicTabs = new List<string>();
            if (!TabHeaders.ContainsKey("Plan-A-City-A")) TabHeaders["Plan-A-City-A"] = "Plan-A-City-A";
            ShowTimeout(OperationType.LoadTabs, $"Loading tabs failed: {ex.Message}");
        }
    }

    // when loading tab data, pick up header if present
    private async Task LoadFromApi()
    {
        Log($"LoadFromApi start: {calculatorType}");
        var apiBaseUrl = this.ApiUrlProvider.GetApiBaseUrl();
        var url = $"{apiBaseUrl}/api/CostOfLiving/load?userGuid={Uri.EscapeDataString(userGuid)}&calculatorType={Uri.EscapeDataString(calculatorType)}";

        try
        {
            var client = HttpCustomClientService.CreateRetryClient(HttpClientFactory);
            // shorter timeout to avoid long blocking during initial render
            try { client.Timeout = TimeSpan.FromSeconds(12); } catch { }
            var response = await client.GetAsync(url);
            Log($"LoadFromApi: call returned {(int)response.StatusCode}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                try
                {
                    var data = System.Text.Json.JsonSerializer.Deserialize<CostOfLivingData>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (data != null)
                    {
                        // restore top-level settings
                        YearsToRetirement = data.YearsToRetirement;
                        InflationRate = data.InflationRate;
                        collapsed = new HashSet<string>(data.CollapsedCategories ?? new List<string>());

                        Items = data.Items ?? new List<CostItem>();

                        // set header mapping for current activeTab
                        if (!string.IsNullOrWhiteSpace(activeTab))
                        {
                            TabHeaders[activeTab] = data.Header ?? GetTabLabel(activeTab);
                        }

                        StateHasChanged();
                        Log($"LoadFromApi success: items={Items.Count}");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    LogError($"LoadFromApi parse error: {ex.Message}");
                }

                // Fallback: try deserialize into raw list of items
                var loaded = System.Text.Json.JsonSerializer.Deserialize<List<CostItem>>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (loaded != null)
                {
                    Items = loaded;
                    StateHasChanged();
                    Log($"LoadFromApi fallback list loaded: items={Items.Count}");
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // no saved data, load defaults
                //Items = StandardCostCategories.GetDefaults();
                //StateHasChanged();
                //Log("LoadFromApi: not found - using defaults");
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("console.error", $"Load failed: {response.StatusCode}");
                LogError($"LoadFromApi failed status: {response.StatusCode}");
            }
        }
        catch (OperationCanceledException oce)
        {
            LogError($"LoadFromApi timeout/cancelled: {oce.Message}");
            // fallback to defaults so UI remains usable
            Items = StandardCostCategories.GetDefaults();
            StateHasChanged();
            ShowTimeout(OperationType.LoadFromApi, "Loading calculator data timed out. You can retry.");
        }
        catch (Exception ex)
        {
            await JSRuntime.InvokeVoidAsync("console.error", $"Error loading: {ex.Message}");
            LogError($"LoadFromApi exception: {ex.Message}");
            ShowTimeout(OperationType.LoadFromApi, $"Loading data failed: {ex.Message}");
        }
    }

    private void UpdateGroupCategory(string oldCategory, string newCategory)
    {
        if (oldCategory == newCategory) return;

        foreach (var it in Items.Where(i => (i.Category ?? string.Empty) == (oldCategory ?? string.Empty)))
        {
            it.Category = newCategory;
        }
    }

    private HashSet<string> collapsed = new HashSet<string>();

    private string GenerateNewItemName(string category)
    {
        var catKey = string.IsNullOrWhiteSpace(category) ? "Uncategorized" : category;
        var existingCount = Items.Count(i => string.Equals(i.Category ?? string.Empty, category ?? string.Empty, StringComparison.OrdinalIgnoreCase));
        return $"{catKey} Item {existingCount + 1}";
    }

    private void AddItemToCategory(string category)
    {
        Log($"AddItemToCategory: {category}");
        var cat = category ?? string.Empty;
        var newItem = new CostItem
        {
            Category = cat,
            Subcategory = GenerateNewItemName(cat),
            CurrentValue = 0m,
            AdjustOption = RetirementAdjustOption.Inflation,
            IncludeInRetirement = true
        };

        // insert at end of that category's block (just add; rendering sorts by Subcategory)
        Items.Add(newItem);

        // Open the same view/edit modal used for existing items so the user can edit the new item
        var idx = Items.IndexOf(newItem);
        if (idx >= 0)
        {
            OpenViewModal(idx);
        }

        StateHasChanged();
        ScheduleAutosave();
    }

    private void ToggleCollapse(string category)
    {
        var key = category ?? string.Empty;
        if (collapsed.Contains(key)) collapsed.Remove(key);
        else collapsed.Add(key);
    }

    private bool IsCollapsed(string category)
    {
        return collapsed.Contains(category ?? string.Empty);
    }

    private decimal TotalCurrent => Math.Round(Items.Sum(i => i.CurrentValue), 2);

    private decimal TotalCurrentPerMonth => Math.Round(Items.Sum(i => i.GetMonthlyEquivalent), 2);

    private decimal TotalRetirement => Math.Round(Items.Sum(i => i.GetRetirementValue(YearsToRetirement, InflationRate)), 2);

    private static string FormatCurrency(decimal value)
    {
        return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:C0}", value);
    }

    private string FormatDecimal(decimal value)
    {
        return value.ToString("0.0", System.Globalization.CultureInfo.CurrentCulture);
    }

    // Missing fields and helper methods restored
    private string NewCategorySentinel { get; } = "__new__";
    private string SelectedAddCategory { get; set; } = string.Empty;
    private string NewCategoryName { get; set; } = string.Empty;
    private bool showAddCategoryModal = false;

    private List<string> Categories => Items.Select(i => i.Category ?? string.Empty).Distinct().OrderBy(c => c).ToList();

    private void RefreshCategories()
    {
        InvokeAsync(StateHasChanged);
    }

    private void OpenAddCategoryModal()
    {
        NewCategoryName = string.Empty;
        showAddCategoryModal = true;
    }

    private void CloseAddCategoryModal()
    {
        showAddCategoryModal = false;
        NewCategoryName = string.Empty;
    }

    private void AddCategoryModal()
    {
        var newCat = (NewCategoryName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(newCat)) return;
        if (Categories.Contains(newCat))
        {
            SelectedAddCategory = newCat;
            CloseAddCategoryModal();
            return;
        }

        var newItem = new CostItem
        {
            Category = newCat,
            Subcategory = GenerateNewItemName(newCat),
            CurrentValue = 0m,
            AdjustOption = RetirementAdjustOption.Inflation,
            IncludeInRetirement = true
        };

        Items.Add(newItem);
        RefreshCategories();
        SelectedAddCategory = newCat;
        StateHasChanged();
        ScheduleAutosave();
        CloseAddCategoryModal();
    }

    private void HandleAddCategoryKeyModal(KeyboardEventArgs e)
    {
        if (e.Key == "Enter") AddCategoryModal();
    }

    private void AddRow()
    {
        var category = SelectedAddCategory == NewCategorySentinel ? (string.IsNullOrWhiteSpace(NewCategoryName) ? "Uncategorized" : NewCategoryName) : SelectedAddCategory;
        var newItem = new CostItem { Category = category, Subcategory = GenerateNewItemName(category), CurrentValue = 0m, AdjustOption = RetirementAdjustOption.Inflation, IncludeInRetirement = true };
        Items.Add(newItem);
        if (SelectedAddCategory == NewCategorySentinel && !string.IsNullOrWhiteSpace(NewCategoryName))
        {
            RefreshCategories();
            SelectedAddCategory = NewCategoryName;
            NewCategoryName = string.Empty;
        }
        ScheduleAutosave();
    }

    // remove with undo support
    private CostItem? lastRemovedItem = null;
    private int lastRemovedIndex = -1;
    private System.Timers.Timer? undoTimer;
    private bool showUndoToast = false;

    private void RemoveItem(int index)
    {
        if (index >= 0 && index < Items.Count)
        {
            lastRemovedIndex = index;
            lastRemovedItem = Items[index];
            Items.RemoveAt(index);

            showUndoToast = true;
            StateHasChanged();

            // start undo timer; if expires, finalize by saving
            if (undoTimer == null)
            {
                undoTimer = new System.Timers.Timer(5000);
                undoTimer.AutoReset = false;
                undoTimer.Elapsed += async (_, __) =>
                {
                    try
                    {
                        showUndoToast = false;
                        await InvokeAsync(async () => await SaveToApi());
                        lastRemovedItem = null;
                        lastRemovedIndex = -1;
                        StateHasChanged();
                    }
                    catch (Exception ex)
                    {
                        LogError($"undoTimer elapsed exception: {ex.Message}");
                    }
                };
            }
            else
            {
                undoTimer.Stop();
                undoTimer.Interval = 5000;
            }
            undoTimer.Start();
        }
    }

    private void UndoRemove()
    {
        if (lastRemovedItem != null)
        {
            var insertIndex = Math.Min(Math.Max(0, lastRemovedIndex), Items.Count);
            Items.Insert(insertIndex, lastRemovedItem);
            lastRemovedItem = null;
            lastRemovedIndex = -1;
            showUndoToast = false;
            if (undoTimer != null)
            {
                try { undoTimer.Stop(); undoTimer.Dispose(); } catch { }
                undoTimer = null;
            }
        }
    }

    private void RemoveCategory(string category)
    {
        if (string.IsNullOrEmpty(category)) return;
        Items.RemoveAll(i => (i.Category ?? string.Empty) == category);
        ScheduleAutosave();
    }

    // user & calculatorType fields and API helpers
    private string userGuid = Guid.NewGuid().ToString();
    private string calculatorType = "CostOfLiving_Your";



    private async Task SaveToApi()
    {
        Log("SaveToApi start");
        isSaving = true;
        saveErrorMessage = string.Empty;
        lastSaveMessage = string.Empty;
        await InvokeAsync(StateHasChanged);

        var apiBaseUrl = this.ApiUrlProvider.GetApiBaseUrl();
        var url = $"{apiBaseUrl}/api/CostOfLiving/save";

        try
        {
            string headerToSave = string.Empty;
            const string prefix = "CostOfLiving-";
            if (!string.IsNullOrEmpty(calculatorType) && calculatorType.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var tabId = calculatorType.Substring(prefix.Length);
                if (TabHeaders != null && TabHeaders.ContainsKey(tabId)) headerToSave = TabHeaders[tabId];
            }

            var dto = new PersistCostOfLivingRequest
            {
                UserGuid = userGuid,
                CalculatorType = calculatorType,
                Data = new CostOfLivingData
                {
                    Header = headerToSave,
                    Items = Items.Select(i => new CostItem
                    {
                        Category = i.Category,
                        Subcategory = i.Subcategory,
                        CurrentValue = i.CurrentValue,
                        Frequency = i.Frequency,
                        AdjustOption = i.AdjustOption,
                        PerItemInflationPercent = i.PerItemInflationPercent,
                        PerItemInflationSource = i.PerItemInflationSource,
                        CustomPercentage = i.CustomPercentage,
                        ManualRetirementValue = i.ManualRetirementValue,
                        IncludeInRetirement = i.IncludeInRetirement
                    }).ToList(),
                    CollapsedCategories = collapsed.ToList(),
                    YearsToRetirement = YearsToRetirement,
                    InflationRate = InflationRate
                }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var client = HttpCustomClientService.CreateRetryClient(HttpClientFactory);
            var response = await client.PostAsync(url, content);
            Log($"SaveToApi: save returned {(int)response.StatusCode}");
            if (response.IsSuccessStatusCode)
            {
                await JSRuntime.InvokeVoidAsync("console.log", "Saved Cost of Living");
                Log("SaveToApi success");
                //lastSaveMessage = $"Saved {DateTime.Now:T}";
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("console.error", $"Save failed: {response.StatusCode}");
                LogError($"SaveToApi failed status: {response.StatusCode}");
                saveErrorMessage = $"{response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            await JSRuntime.InvokeVoidAsync("console.error", $"Error saving: {ex.Message}");
            LogError($"SaveToApi exception: {ex.Message}");
            saveErrorMessage = ex.Message;
        }
        finally
        {
            isSaving = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        SelectedAddCategory = Categories.FirstOrDefault() ?? NewCategorySentinel;
        Log("OnInitialized");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Log("OnAfterRenderAsync firstRender start");

            // Check if user has opted out of seeing the intro modal
            try
            {
                var hideIntro = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "finplan-cost-of-living-hide-intro");
                if (hideIntro == "true")
                {
                    showIntroModal = false;
                }

                // New: update survey visibility (mobile: only after ~5 loads; desktop: always)
                await UpdateSurveyVisibilityAsync();
            }
            catch (Exception ex)
            {
                LogError($"Error checking intro modal preference: {ex.Message}");
            }

            // show loading indicator while we fetch user data and tabs
            isLoading = true;
            StateHasChanged();
            await HandleIntroModal();

            try
            {
                try
                {
                    userGuid = await UserGuidService.GetOrCreateUserGuidAsync();

                    _ = TrackPageViewAsync();

                    Log($"OnAfterRenderAsync got userGuid: {userGuid}");
                }
                catch (Exception exGet) { LogError($"UserGuidService.GetOrCreateUserGuidAsync failed: {exGet.Message}"); }

                await LoadTabsAsync();

                if (!Tabs.Contains(activeTab))
                {
                    // If there are no dynamic tabs returned, keep the existing activeTab (default "plan-a").
                    // Otherwise select the first available tab safely.
                    activeTab = Tabs.FirstOrDefault() ?? activeTab ?? "Plan-A-City-A";
                }

                calculatorType = MapTabToCalculatorType(activeTab);

                await LoadFromApi();

                // Load city templates and user demographics
                await LoadAvailableCities();
                await LoadUserDemographics();
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
                Log("OnAfterRenderAsync firstRender end");
            }
        }
    }

    private System.Timers.Timer? autosaveTimer;
    private readonly object autosaveLock = new object();
    private int autosaveDelayMs = 800; // debounce

    // Recalculate derived UI values and optionally schedule autosave.
    private void RecalculateAll(bool save = false)
    {
        try
        {
            // Totals are computed properties (TotalCurrent/TotalRetirement) so just refresh UI.
            InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            LogError($"RecalculateAll error: {ex.Message}");
        }

        if (save)
        {
            ScheduleAutosave();
        }
    }

    private void ScheduleAutosave()
    {
        Log("ScheduleAutosave called");

        lock (autosaveLock)
        {
            if (autosaveTimer == null)
            {
                autosaveTimer = new System.Timers.Timer(autosaveDelayMs);
                autosaveTimer.AutoReset = false;
                autosaveTimer.Elapsed += async (_, __) =>
                {
                    try
                    {
                        await InvokeAsync(async () => await SaveToApi());
                    }
                    catch (Exception ex)
                    {
                        LogError($"autosave timer handler exception: {ex.Message}");
                    }
                };
            }
            else
            {
                autosaveTimer.Stop();
                autosaveTimer.Interval = autosaveDelayMs;
            }

            autosaveTimer.Start();
        }
    }

    public void Dispose()
    {
        if (autosaveTimer != null)
        {
            try { autosaveTimer.Dispose(); } catch { }
            autosaveTimer = null;
        }

        if (undoTimer != null)
        {
            try { undoTimer.Dispose(); } catch { }
            undoTimer = null;
        }
    }

    private void HandleAddCategoryKey(KeyboardEventArgs e)
    {
        if (e.Key == "Enter") OpenAddCategoryModal();
    }

    private void HandleNumericKey(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            ScheduleAutosave();
        }
    }

    private async Task OnTabClick(string tab)
    {
        if (activeTab == tab) return;
        Log($"OnTabClick: switching to {tab}");
        activeTab = tab;
        calculatorType = MapTabToCalculatorType(tab);
        await LoadFromApi();
    }

    private enum OperationType { None, LoadTabs, LoadFromApi }
    private bool showTimeoutModal = false;
    private string timeoutMessage = string.Empty;
    private OperationType failedOperation = OperationType.None;

    private void ShowTimeout(OperationType op, string message)
    {
        failedOperation = op;
        timeoutMessage = message;
        showTimeoutModal = true;
        InvokeAsync(StateHasChanged);
    }

    private void DismissTimeoutModal()
    {
        showTimeoutModal = false;
        failedOperation = OperationType.None;
        timeoutMessage = string.Empty;
        InvokeAsync(StateHasChanged);
    }

    private async Task RetryOperation()
    {
        showTimeoutModal = false;
        var op = failedOperation;
        failedOperation = OperationType.None;
        timeoutMessage = string.Empty;
        // Refresh UI
        StateHasChanged();

        try
        {
            if (op == OperationType.LoadTabs)
            {
                Log("User triggered retry: LoadTabsAsync");
                await LoadTabsAsync();
                // if tabs loaded and activeTab missing, ensure selection
                if (!Tabs.Contains(activeTab)) activeTab = Tabs.FirstOrDefault() ?? "Plan-A-City-A";
                calculatorType = MapTabToCalculatorType(activeTab);
                // After reloading tabs, try loading data for the active tab
                await LoadFromApi();
            }
            else if (op == OperationType.LoadFromApi)
            {
                Log("User triggered retry: LoadFromApi");
                await LoadFromApi();
            }
        }
        catch (Exception ex)
        {
            LogError($"RetryOperation failed: {ex.Message}");
            // Show modal again with updated message
            ShowTimeout(op, $"Retry failed: {ex.Message}");
        }
    }

    private async Task ConfirmRemoveItem(int idx)
    {
        var ok = await JSRuntime.InvokeAsync<bool>("confirm", $"Remove this item? This action cannot be undone.");
        if (ok) RemoveItem(idx);
    }

    private async Task ConfirmRemoveCategory(string category)
    {
        var ok = await JSRuntime.InvokeAsync<bool>("confirm", $"Remove this category and all its items? This action cannot be undone.");
        if (ok) RemoveCategory(category);
    }

    private void CollapseAllCategories()
    {
        collapsed = new HashSet<string>(Items.Select(i => i.Category).Where(c => !string.IsNullOrEmpty(c)).Distinct());
    }

    private void ToggleCollapseAllCategories()
    {
        if (AnyCategoryExpanded())
        {
            // Collapse all
            collapsed = new HashSet<string>(Items.Select(i => i.Category).Where(c => !string.IsNullOrEmpty(c)).Distinct());
        }
        else
        {
            // Expand all
            collapsed.Clear();
        }
    }
    private bool AnyCategoryExpanded()
    {
        var allCategories = Items.Select(i => i.Category).Where(c => !string.IsNullOrEmpty(c)).Distinct();
        return allCategories.Any(c => !collapsed.Contains(c));
    }

    // Add inside the existing @code { ... } block of CostOfLivingPlannerNew.razor

    // Modal visibility
    private bool showAssumptionsModal = false;

    // Called when user saves from the modal. Receives (Years, Inflation).
    private async Task OnAssumptionsSaved((int Years, decimal Inflation) values)
    {
        // apply values
        YearsToRetirement = values.Years;
        InflationRate = values.Inflation;

        // recalc and persist
        RecalculateAll(save: true);
        ScheduleAutosave();

        // close modal and update UI
        showAssumptionsModal = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task CloseAssumptions()
    {
        showAssumptionsModal = false;
        await InvokeAsync(StateHasChanged);
    }

    // Open the modal from UI
    private void OpenWizard(int section)
    {
        // reuse same modal for quick assumptions edits
        // copy current values into modal via component parameters (component reads YearsToRetirement/InflationRate)
        showAssumptionsModal = true;
    }

    // New state variables for multi-step flow
    private bool showPlanningTypeModal = true; // Start with this instead of overlay
    private bool showAssumptionsSetupModal = false;
    private bool useSampleData = false;

    private void SelectPlanningType(bool useSample)
    {
        useSampleData = useSample;
        showPlanningTypeModal = false;
        showAssumptionsSetupModal = true;
    }

    private void GoBackToPlanningType()
    {
        showAssumptionsSetupModal = false;
        showPlanningTypeModal = true;
    }

    private Task StartPlanning()
    {
        showAssumptionsSetupModal = false;

        if (useSampleData)
        {
            // Load sample data with the user's assumptions
            Items = StandardCostCategories.GetDefaults();
        }
        else
        {
            // Start with empty list
            Items = new List<CostItem>();
        }

        // Save the assumptions
        ScheduleAutosave();
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task ShowIntroWithReset()
    {
        // Remove the localStorage setting that hides the intro
        await JSRuntime.InvokeVoidAsync("localStorage.removeItem", "finplan-cost-of-living-income-planner-hide-intro");

        // Show the intro modal
        showIntroModal = true;

        // Update the UI
        StateHasChanged();
    }

    private void StartWithDefaults()
    {
        Items = StandardCostCategories.GetDefaults();
        showIntroModal = false; // Add this line to close the modal

        ScheduleAutosave();
        StateHasChanged();
    }

    private bool showIntroModal = true; // Show by default unless user has opted out
    private bool dontShowIntroAgain = false;





    private bool showToolTipYearsInfo = false;
    private bool showToolTipInflationInfo = false;

    // Add Item modal state
    private bool showAddItemModal = false;
    private string addItemCategory = string.Empty;

    // New item fields
    private string NewItemName = string.Empty;
    private string NewItemFrequency = "Monthly"; // parsed to enum on save
    private decimal NewItemAmount = 0m;
    private bool NewItemIncludeInRetirement = true;
    private string NewItemExclusionReason = "NotNeeded"; // NotNeeded | PaidOff | Other

    // Retirement fields
    private string NewItemAdjustOption = "Inflation"; // Same | CustomPercentage | Manual | Inflation
    private string NewItemPerItemInflationSource = "UseGlobal"; // UseGlobal | Custom
    private decimal NewItemPerItemInflationPercent = 0m;
    private decimal NewItemCustomPercentage = 0m;

    // Manual entry as string to allow empty -> null parsing
    private string? NewItemManualRetirementValueString = null;

    // Validation
    private bool IsAddItemDisabled
        => string.IsNullOrWhiteSpace(NewItemName?.Trim());

    // Open/close
    private void OpenAddItemModal(string category)
    {
        isEditItemMode = false;
        editItemIndex = -1;

        addItemCategory = category ?? string.Empty;

        NewItemName = string.Empty;
        NewItemFrequency = "Monthly";
        NewItemAmount = 0m;
        NewItemIncludeInRetirement = true;
        NewItemExclusionReason = "NotNeeded";

        NewItemAdjustOption = "Inflation";
        NewItemPerItemInflationSource = "UseGlobal";
        NewItemPerItemInflationPercent = 0m;
        NewItemCustomPercentage = 0m;
        NewItemManualRetirementValueString = null;

        showAddItemModal = true;
    }

    private void CloseAddItemModal()
    {
        showAddItemModal = false;
        isEditItemMode = false;
        editItemIndex = -1;
        NewItemName = string.Empty;
    }

    // Save
    private void AddItemModalSave()
    {
        if (IsAddItemDisabled) return;

        // Set defaults for simple mode
        if (isSimpleMode)
        {
            // Use sensible defaults for beginners
            if (string.IsNullOrEmpty(NewItemFrequency))
            {
                NewItemFrequency = "Monthly";
            }
            if (string.IsNullOrEmpty(NewItemAdjustOption))
            {
                NewItemAdjustOption = "Inflation";
            }
            if (string.IsNullOrEmpty(NewItemPerItemInflationSource))
            {
                NewItemPerItemInflationSource = "UseGlobal";
            }
        }

        if (!Enum.TryParse<Frequency>(NewItemFrequency, out var freq))
            freq = Frequency.Monthly;

        if (!Enum.TryParse<RetirementAdjustOption>(NewItemAdjustOption, out var adjust))
            adjust = RetirementAdjustOption.Inflation;

        if (!Enum.TryParse<InflationSource>(NewItemPerItemInflationSource, out var inflSrc))
            inflSrc = InflationSource.UseGlobal;

        // Parse exclusion reason
        ExclusionReason? exclusionReason = null;
        if (!NewItemIncludeInRetirement && Enum.TryParse<ExclusionReason>(NewItemExclusionReason, out var exReason))
        {
            exclusionReason = exReason;
        }

        decimal? manualValue = null;
        if (!string.IsNullOrWhiteSpace(NewItemManualRetirementValueString) &&
            decimal.TryParse(NewItemManualRetirementValueString, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.CurrentCulture, out var mv) &&
            mv >= 0)
        {
            manualValue = mv;
        }

        if (isEditItemMode && editItemIndex >= 0 && editItemIndex < Items.Count)
        {
            // Update existing item
            var item = Items[editItemIndex];
            item.Subcategory = NewItemName.Trim();
            item.Frequency = freq;
            item.CurrentValue = NewItemAmount;
            item.IncludeInRetirement = NewItemIncludeInRetirement;
            item.RetirementExclusionReason = exclusionReason;
            item.AdjustOption = adjust;
            item.PerItemInflationSource = inflSrc;
            item.PerItemInflationPercent = inflSrc == InflationSource.Custom ? NewItemPerItemInflationPercent : null;
            item.CustomPercentage = NewItemAdjustOption == "CustomPercentage" ? NewItemCustomPercentage : 0m;
            item.ManualRetirementValue = NewItemAdjustOption == "Manual" ? manualValue : null;
        }
        else
        {
            // Add new item
            var newItem = new CostItem
            {
                Category = addItemCategory,
                Subcategory = NewItemName.Trim(),
                CurrentValue = NewItemAmount,
                Frequency = freq,
                IncludeInRetirement = NewItemIncludeInRetirement,
                RetirementExclusionReason = exclusionReason,
                AdjustOption = adjust,
                PerItemInflationSource = inflSrc,
                PerItemInflationPercent = inflSrc == InflationSource.Custom ? NewItemPerItemInflationPercent : null,
                CustomPercentage = NewItemAdjustOption == "CustomPercentage" ? NewItemCustomPercentage : 0m,
                ManualRetirementValue = NewItemAdjustOption == "Manual" ? manualValue : null
            };
            Items.Add(newItem);
            collapsed.Remove(addItemCategory ?? string.Empty); // ensure visible
        }

        // Close and persist
        showAddItemModal = false;
        isEditItemMode = false;
        editItemIndex = -1;

        StateHasChanged();
        ScheduleAutosave();
    }

    // Keyboard: allow Enter to submit from key fields
    private void HandleAddItemKeyModal(KeyboardEventArgs e)
    {
        if (e?.Key == "Enter")
            AddItemModalSave();
    }

    // Unified Item modal mode state
    private bool isEditItemMode = false;
    private int editItemIndex = -1;

    private void OpenEditItemModal(int idx)
    {
        if (idx < 0 || idx >= Items.Count) return;

        isEditItemMode = true;
        editItemIndex = idx;

        var it = Items[idx];
        addItemCategory = it.Category ?? string.Empty;
        NewItemName = it.Subcategory ?? string.Empty;
        NewItemFrequency = it.Frequency.ToString();
        NewItemAmount = it.CurrentValue;
        NewItemIncludeInRetirement = it.IncludeInRetirement;
        NewItemExclusionReason = it.RetirementExclusionReason?.ToString() ?? "NotNeeded";

        NewItemAdjustOption = it.AdjustOption.ToString(); // "Same" | "Inflation" | "CustomPercentage" | "Manual"
        NewItemPerItemInflationSource = it.PerItemInflationSource.ToString(); // "UseGlobal" | "Custom"
        NewItemPerItemInflationPercent = it.PerItemInflationPercent ?? 0m;
        NewItemCustomPercentage = it.CustomPercentage;
        NewItemManualRetirementValueString = it.ManualRetirementValue?.ToString(System.Globalization.CultureInfo.CurrentCulture) ?? null;

        showAddItemModal = true;
    }

    // Add/Edit Item modal inline tooltip flags
    private bool showTipItemName = false;
    private bool showTipFrequency = false;
    private bool showTipAmount = false;
    private bool showTipInclude = false;
    private bool showTipExclusionReason = false;
    private bool showTipAdjust = false;
    private bool showTipCustomPercent = false;
    private bool showTipManualValue = false;
    private bool showTipInflationSource = false;
    private bool showTipCustomInflation = false;

    // Info link state (top-level)
    private bool showTipCurrentTotal = false;
    private bool showTipRetirementTotal = false;
    private bool showTipAddCategory = false;
    private bool showTipCollapseAll = false;

    // Per-category info states
    private HashSet<string> catInfoOpen = new(StringComparer.OrdinalIgnoreCase);
    private HashSet<string> addItemInfoOpen = new(StringComparer.OrdinalIgnoreCase);

    private bool IsCatInfoOpen(string? key) => !string.IsNullOrWhiteSpace(key) && catInfoOpen.Contains(key);
    private void ToggleCatInfo(string? key)
    {
        var k = key ?? string.Empty;
        if (catInfoOpen.Contains(k)) catInfoOpen.Remove(k); else catInfoOpen.Add(k);
    }

    private bool IsAddItemInfoOpen(string? key) => !string.IsNullOrWhiteSpace(key) && addItemInfoOpen.Contains(key);
    private void ToggleAddItemInfo(string? key)
    {
        var k = key ?? string.Empty;
        if (addItemInfoOpen.Contains(k)) addItemInfoOpen.Remove(k); else addItemInfoOpen.Add(k);
    }
    // Per-row View/Edit info state
    private HashSet<int> viewEditInfoOpen = new();

    private bool IsViewEditInfoOpen(int idx) => viewEditInfoOpen.Contains(idx);

    private void ToggleViewEditInfo(int idx)
    {
        if (viewEditInfoOpen.Contains(idx)) viewEditInfoOpen.Remove(idx);
        else viewEditInfoOpen.Add(idx);
    }

    // Plan‑A tab info tip
    private bool showTipPlanATab = false;
    private void TogglePlanATabInfo() => showTipPlanATab = !showTipPlanATab;

    // ============================================
    // City Template Methods
    // ============================================

    private async Task LoadAvailableCities()
    {
        try
        {
            var client = HttpCustomClientService.CreateRetryClient(HttpClientFactory);
            var apiBaseUrl = this.ApiUrlProvider.GetApiBaseUrl();
            availableCities = await client.GetFromJsonAsync<List<CityTemplate>>($"{apiBaseUrl}/api/citytemplate");
            Log($"Loaded {availableCities?.Count ?? 0} city templates");
        }
        catch (Exception ex)
        {
            LogError($"Error loading city templates: {ex.Message}");
        }
    }

    private async Task LoadUserDemographics()
    {
        try
        {
            var client = HttpCustomClientService.CreateRetryClient(HttpClientFactory);
            var apiBaseUrl = this.ApiUrlProvider.GetApiBaseUrl();

            try
            {
                userDemographics = await client.GetFromJsonAsync<UserDemographics>($"{apiBaseUrl}/api/userdemographics/{userGuid}");
                Log($"Loaded user demographics for {userGuid}");
            }
            catch (HttpRequestException)
            {
                // User demographics not found - this is expected for new users
                Log($"No demographics found for user {userGuid}");
            }
        }
        catch (Exception ex)
        {
            LogError($"Error loading user demographics: {ex.Message}");
        }
    }

    // update SelectCity to open the profile modal after loading profiles
    private async Task SelectCity(CityTemplate city)
    {
        try
        {
            selectedCity = city;

            // Load profiles for this city
            var client = HttpCustomClientService.CreateRetryClient(HttpClientFactory);
            var apiBaseUrl = this.ApiUrlProvider.GetApiBaseUrl();
            cityProfiles = await client.GetFromJsonAsync<List<DemographicProfile>>($"{apiBaseUrl}/api/citytemplate/{city.CityId}/profiles");

            // Try to match a profile if we have user demographics
            if (userDemographics != null && cityProfiles?.Any() == true)
            {
                await MatchBestProfile(city.CityId);
            }

            // Show the profile selector modal (only if profiles exist)
            showProfileSelector = cityProfiles != null && cityProfiles.Any();

            Log($"Selected city: {city.CityName}, loaded {cityProfiles?.Count ?? 0} profiles");
            StateHasChanged();
        }
        catch (Exception ex)
        {
            LogError($"Error selecting city: {ex.Message}");
        }
    }

    private async Task MatchBestProfile(string cityId)
    {
        try
        {
            if (userDemographics == null) return;

            var client = HttpCustomClientService.CreateRetryClient(HttpClientFactory);
            var apiBaseUrl = this.ApiUrlProvider.GetApiBaseUrl();

            var url = $"{apiBaseUrl}/api/citytemplate/match?" +
                      $"cityId={Uri.EscapeDataString(cityId)}" +
                      $"&age={userDemographics.Age}" +
                      $"&maritalStatus={userDemographics.MaritalStatus}" +
                      $"&childrenCount={userDemographics.ChildrenAges?.Count ?? 0}";

            matchedProfile = await client.GetFromJsonAsync<DemographicProfile>(url);

            if (matchedProfile != null)
            {
                Log($"Matched profile: {matchedProfile.ProfileName}");
            }
        }
        catch (Exception ex)
        {
            LogError($"Error matching profile: {ex.Message}");
        }
    }

    private async Task LoadExpensesFromProfile(DemographicProfile profile)
    {
        try
        {
            if (profile.SampleExpenses != null && profile.SampleExpenses.Any())
            {
                // Ask for confirmation before replacing expenses
                var confirmed = await JSRuntime.InvokeAsync<bool>("confirm",
                    $"Load sample expenses from '{profile.ProfileName}'? This will replace your current expenses.");

                if (!confirmed) return;

                // Clear existing items
                Items.Clear();

                // Add expenses from profile
                foreach (var expense in profile.SampleExpenses)
                {
                    Items.Add(new CostItem
                    {
                        Category = expense.Category,
                        Subcategory = expense.Subcategory,
                        CurrentValue = expense.CurrentValue,
                        Frequency = expense.Frequency,
                        AdjustOption = RetirementAdjustOption.Inflation,
                        PerItemInflationSource = InflationSource.UseGlobal,
                        //PerItemInflationPercent = expense.PerItemInflationPercent,
                        //CustomPercentage = expense.CustomPercentage,
                        //ManualRetirementValue = expense.ManualRetirementValue,
                        IncludeInRetirement = true,
                        //RetirementExclusionReason = expense.RetirementExclusionReason
                    });
                }

                RecalculateAll(save: true);
                StateHasChanged();

                Log($"Loaded {profile.SampleExpenses.Count} expenses from profile: {profile.ProfileName}");
            }
        }
        catch (Exception ex)
        {
            LogError($"Error loading expenses from profile: {ex.Message}");
        }
    }

    private async Task SaveUserDemographics(int age, MaritalStatus maritalStatus, List<int> childrenAges)
    {
        try
        {
            var client = HttpCustomClientService.CreateRetryClient(HttpClientFactory);
            var apiBaseUrl = this.ApiUrlProvider.GetApiBaseUrl();

            var demographics = new UserDemographics
            {
                UserGuid = userGuid,
                Age = age,
                MaritalStatus = maritalStatus,
                ChildrenAges = childrenAges,
                PreferredCurrency = selectedCity?.Currency ?? "USD",
                SelectedCityId = selectedCity?.CityId ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            HttpResponseMessage response;
            if (userDemographics == null)
            {
                // Create new
                response = await client.PostAsJsonAsync($"{apiBaseUrl}/api/userdemographics", demographics);
            }
            else
            {
                // Update existing
                response = await client.PutAsJsonAsync($"{apiBaseUrl}/api/userdemographics/{userGuid}", demographics);
            }

            response.EnsureSuccessStatusCode();
            userDemographics = demographics;

            Log("User demographics saved successfully");
        }
        catch (Exception ex)
        {
            LogError($"Error saving user demographics: {ex.Message}");
        }
    }

    private void ToggleCitySelector()
    {
        showCitySelector = !showCitySelector;
        StateHasChanged();
    }

    private bool showSurvey = false;


    private async Task UpdateSurveyVisibilityAsync()
    {
        try
        {
            // Detect mobile via matchMedia (<= 767px)
            var isMobile = await JSRuntime.InvokeAsync<bool>("eval", "window.matchMedia && window.matchMedia('(max-width: 767px)').matches");

            if (!isMobile)
            {
                // Desktop -> always show
                showSurvey = true;
                return;
            }

            // Mobile -> show only after ~5 page loads (per device/localStorage)
            string key = "finplan-cost-of-living-mobile-loads";

            var raw = await JSRuntime.InvokeAsync<string>("localStorage.getItem", key);
            int count = 0;
            if (!string.IsNullOrEmpty(raw) && int.TryParse(raw, out var parsed)) count = parsed;

            count++;
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", key, count.ToString());

            showSurvey = count >= 5;
        }
        catch (Exception ex)
        {
            //DebugService.AddMessage($"Survey visibility check failed: {ex.Message}");
            // fallback: hide on error to avoid surprising mobile UX
            showSurvey = false;
        }
        finally
        {
            StateHasChanged();
        }
    }

    // create a new tab using the selected profile (keeps existing AddNewTab behavior)
    private async Task CreateTabFromProfile(DemographicProfile profile)
    {
        if (profile == null) return;

        try
        {
            // Set as matched so AddNewTab will use the profile's sample expenses
            matchedProfile = profile;

            try { matchedProfile.DeserializeFromDatabase(); } catch { }

            foreach (var item in matchedProfile.SampleExpenses)
            {
                item.PerItemInflationSource = InflationSource.UseGlobal;
                item.AdjustOption = RetirementAdjustOption.Inflation;
                item.IncludeInRetirement = true;
            }

            // Close pickers/modals
            showProfileSelector = false;
            showCitySelector = false;

            // Create a new plan/tab using the existing AddNewTab implementation
            await AddNewTab();

            selectedCity = null;
        }
        catch (Exception ex)
        {
            LogError($"CreateTabFromProfile exception: {ex.Message}");
        }
    }
    private async Task HandleItemSave((CostItem Item, bool IsEdit, int EditIndex) data)
    {
        if (data.IsEdit && data.EditIndex >= 0 && data.EditIndex < Items.Count)
        {
            // Update existing item
            Items[data.EditIndex] = data.Item;
        }
        else
        {
            // Add new item
            Items.Add(data.Item);
            collapsed.Remove(data.Item.Category ?? string.Empty); // ensure visible
        }

        StateHasChanged();
        ScheduleAutosave();
    }

    private async Task TrackPageViewAsync()
    {
        try
        {
            var apiBaseUrl = this.ApiUrlProvider.GetApiBaseUrl();
            var client = HttpCustomClientService.CreateRetryClient(HttpClientFactory);
            var route = Navigation.ToBaseRelativePath(Navigation.Uri);
            string? ua = null;
            try { ua = await JSRuntime.InvokeAsync<string>("eval", "navigator.userAgent"); } catch { }
            var dto = new { Page = "CostOfLiving", Route = route, UserGuid = userGuid, UserAgent = ua };
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            await client.PostAsync($"{apiBaseUrl}/api/Tracking/pageview", content);
        }
        catch { }
    }

    // Ad HTML for left column — paste your Adsterra long banner snippet here or store in config and load on init.
    private string AdHtmlLeft = "<!-- Adsterra long banner snippet goes here -->";
}
