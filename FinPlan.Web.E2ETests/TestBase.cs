using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace FinPlan.Web.E2ETests
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class TestBase : PageTest
    {
        protected string BaseUrl { get; set; } = "https://rewealthen.com"; // Cloud-hosted production URL
        
        // Override BrowserOptions to ensure browser is visible (headed mode)
        public override BrowserTypeLaunchOptions LaunchOptions()
        {
            return new BrowserTypeLaunchOptions
            {
                Headless = false,
                SlowMo = 1000, // Slow down by 1000ms to make actions visible
                Channel = "msedge" // Use Microsoft Edge
            };
        }
        
        // Override ContextOptions to set viewport and other browser context settings
        public override BrowserNewContextOptions ContextOptions()
        {
            return new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize { Width = 2560, Height = 1440 },
                IgnoreHTTPSErrors = true
            };
        }
        
        [SetUp]
        public async Task TestSetup()
        {
            // Set viewport size to be twice the default (default is typically 1280x720)
            await Page.SetViewportSizeAsync(2560, 1440);
            
            // Set default timeout for element interactions
            Page.SetDefaultTimeout(30000);
        }
        
        /// <summary>
        /// Helper method to extract numeric value from currency formatted strings
        /// </summary>
        protected static decimal ExtractNumericValue(string? currencyText)
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
        /// Wait for any loading spinners or animations to complete
        /// </summary>
        protected async Task WaitForPageToLoad()
        {
            // Wait for any loading indicators to disappear
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Optional: Wait for any loading spinners or overlays to disappear
            try
            {
                await Page.WaitForSelectorAsync(".loading", new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
            }
            catch
            {
                // Ignore if no loading indicator exists
            }
        }
    }
}