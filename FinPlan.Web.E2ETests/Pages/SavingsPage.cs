using Microsoft.Playwright;

namespace FinPlan.Web.E2ETests.Pages
{
    public class SavingsPage
    {
        private readonly IPage _page;
        private const string PagePath = "/savings-wealth-building";
        
        public SavingsPage(IPage page)
        {
            _page = page;
        }
        
        #region Locators
        
        // Modal and Overlays
        public ILocator IntroModal => _page.Locator(".start-planning-overlay");
        public ILocator StartPlanningButton => _page.Locator("button:has-text('Start Planning')");
        public ILocator TrySampleDataButton => _page.Locator("button:has-text('Try Sample Data')");
        public ILocator CloseModalButton => _page.Locator("button[aria-label='Close']");
        public ILocator DontShowAgainCheckbox => _page.Locator("#dontShowIntroAgainCheck");
        
        // Tab Navigation
        public ILocator YourSavingsTab => _page.Locator("button:has-text('Your Savings')");
        public ILocator PartnerSavingsTab => _page.Locator("button:has-text('Partner Savings')");
        public ILocator ActiveTab => _page.Locator("button[aria-selected='true']");
        
        // Action Buttons
        public ILocator UpdatePlanButton => _page.Locator("button:has-text('Update Plan')");
        public ILocator ClearPlanDataButton => _page.Locator("button:has-text('Clear Plan Data')");
        
        // Summary Cards
        public ILocator SummaryCardsRow => _page.Locator(".summary-cards-row");
        public ILocator FinalSavingsCard => _page.Locator(".savings-card-yellow .card-value");
        public ILocator TotalContributionsCard => _page.Locator(".savings-card-teal .card-value");
        public ILocator TotalGrowthCard => _page.Locator(".savings-card-purple .card-value");
        public ILocator TaxesPaidCard => _page.Locator(".savings-card-red .card-value");
        
        // Progress Message
        public ILocator ProgressMessage => _page.Locator(".alert");
        
        // Section Cards
        public ILocator MilestonesSection => _page.Locator(".section-card").First;
        public ILocator StartingBalancesSection => _page.Locator(".section-card").Nth(1);
        public ILocator MonthlyContributionsSection => _page.Locator(".section-card").Nth(2);
        
        // Empty State
        public ILocator EmptyState => _page.Locator("text=No Projection Available");
        public ILocator GetStartedButton => _page.Locator("button:has-text('Get Started')");
        
        // Table
        public ILocator YearlyProjectionTable => _page.Locator("table[aria-label='Yearly projection']");
        public ILocator TableRows => YearlyProjectionTable.Locator("tbody tr");
        public ILocator FirstTableRow => TableRows.First;
        
        // Toggle Switches
        public ILocator SimpleViewToggle => _page.Locator("#simpleViewToggle");
        public ILocator AdvancedSetupToggle => _page.Locator("#advancedSetupToggle");
        
        // Info Icons and Tooltips
        public ILocator InfoIcons => _page.Locator(".info-icon");
        public ILocator Tooltips => _page.Locator(".info-tooltip");
        public ILocator TooltipCloseButton => _page.Locator(".info-tooltip .float-end");
        
        // Wizard (if visible)
        public ILocator WizardModal => _page.Locator("text=Step");
        public ILocator WizardFinishButton => _page.Locator("button:has-text('Finish')");
        
        #endregion
        
        #region Actions
        
        public async Task NavigateAsync(string baseUrl)
        {
            // First go to home page
            await _page.GotoAsync(baseUrl);
            
            // Find and click the savings/wealth building link using the exact text we discovered
            var savingsLink = _page.Locator("text=Savings & Wealth-Building").Or(_page.Locator("text=Build Wealth"));
            await savingsLink.First.ClickAsync();
            
            // Wait for navigation to complete
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
        
        public async Task<List<string>> DiscoverLinksOnHomePageAsync(string baseUrl)
        {
            await _page.GotoAsync(baseUrl);
            
            // Get all links on the page
            var links = await _page.Locator("a").AllAsync();
            var linkTexts = new List<string>();
            
            foreach (var link in links)
            {
                var text = await link.TextContentAsync();
                var href = await link.GetAttributeAsync("href");
                if (!string.IsNullOrWhiteSpace(text))
                {
                    linkTexts.Add($"{text.Trim()} -> {href}");
                }
            }
            
            return linkTexts;
        }
        
        public async Task CloseIntroModalAsync()
        {
            await CloseModalButton.ClickAsync();
            await IntroModal.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
        }
        
        public async Task StartPlanningAsync()
        {
            await StartPlanningButton.ClickAsync();
            await WizardModal.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        }
        
        public async Task<bool> TrySampleDataAsync()
        {
            // Check if the Try Sample Data button exists
            if (await TrySampleDataButton.IsVisibleAsync())
            {
                await TrySampleDataButton.ClickAsync();
                
                // Wait for sample data to be loaded (summary cards should appear)
                // Use a try-catch to handle timeout gracefully
                try
                {
                    await SummaryCardsRow.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
                    return true;
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("Summary cards did not appear after clicking Try Sample Data button");
                    return false;
                }
            }
            return false;
        }
        
        public async Task SwitchToPartnerTabAsync()
        {
            await PartnerSavingsTab.ClickAsync();
            await _page.WaitForSelectorAsync("button[aria-selected='true']:has-text('Partner Savings')");
        }
        
        public async Task SwitchToYourTabAsync()
        {
            await YourSavingsTab.ClickAsync();
            await _page.WaitForSelectorAsync("button[aria-selected='true']:has-text('Your Savings')");
        }
        
        public async Task ClearPlanDataAsync()
        {
            // Set up dialog handler before clicking
            _page.Dialog += async (_, dialog) =>
            {
                await dialog.AcceptAsync();
            };
            
            await ClearPlanDataButton.ClickAsync();
            
            // Wait for page reload after clearing
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
        
        public async Task ToggleSimpleViewAsync()
        {
            await SimpleViewToggle.ClickAsync();
        }
        
        public async Task ToggleAdvancedSetupAsync()
        {
            await AdvancedSetupToggle.ClickAsync();
        }
        
        public async Task<string> GetFinalSavingsAmountAsync()
        {
            return await FinalSavingsCard.TextContentAsync() ?? "";
        }
        
        public async Task<string> GetTotalContributionsAsync()
        {
            return await TotalContributionsCard.TextContentAsync() ?? "";
        }
        
        public async Task<string> GetTotalGrowthAsync()
        {
            return await TotalGrowthCard.TextContentAsync() ?? "";
        }
        
        public async Task<string> GetTaxesPaidAsync()
        {
            return await TaxesPaidCard.TextContentAsync() ?? "";
        }
        
        public async Task<bool> HasSummaryCardsAsync()
        {
            return await SummaryCardsRow.IsVisibleAsync();
        }
        
        public async Task<bool> IsEmptyStateVisibleAsync()
        {
            return await EmptyState.IsVisibleAsync();
        }
        
        public async Task<int> GetTableRowCountAsync()
        {
            return await TableRows.CountAsync();
        }
        
        public async Task<string> GetTableCellTextAsync(int rowIndex, int columnIndex)
        {
            var row = TableRows.Nth(rowIndex);
            var cell = row.Locator("td").Nth(columnIndex);
            return await cell.TextContentAsync() ?? "";
        }
        
        public async Task ClickInfoIconAsync(int iconIndex = 0)
        {
            await InfoIcons.Nth(iconIndex).ClickAsync();
        }
        
        public async Task CloseTooltipAsync()
        {
            await TooltipCloseButton.ClickAsync();
        }
        
        public async Task<bool> IsTooltipVisibleAsync()
        {
            return await Tooltips.First.IsVisibleAsync();
        }
        
        public async Task CheckDontShowAgainAsync()
        {
            await DontShowAgainCheckbox.CheckAsync();
        }
        
        public async Task<bool> IsIntroModalVisibleAsync()
        {
            return await IntroModal.IsVisibleAsync();
        }
        
        public async Task<string> GetActiveTabTextAsync()
        {
            return await ActiveTab.TextContentAsync() ?? "";
        }
        
        #endregion
        
        #region Validation Helpers
        
        /// <summary>
        /// Extract numeric value from currency formatted strings like "$123,456"
        /// </summary>
        public static decimal ExtractCurrencyValue(string currencyText)
        {
            if (string.IsNullOrEmpty(currencyText))
                return 0;
                
            // Remove currency symbols, commas, and whitespace
            var numericString = currencyText.Replace("$", "").Replace(",", "").Trim();
            
            if (decimal.TryParse(numericString, out var result))
                return result;
                
            return 0;
        }
        
        /// <summary>
        /// Validate that a currency string is properly formatted
        /// </summary>
        public static bool IsCurrencyFormatted(string text)
        {
            return !string.IsNullOrEmpty(text) && 
                   text.StartsWith("$") && 
                   System.Text.RegularExpressions.Regex.IsMatch(text, @"\$[\d,]+");
        }
        
        #endregion
    }
}