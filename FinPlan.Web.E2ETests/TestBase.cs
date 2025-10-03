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
        
        [SetUp]
        public async Task TestSetup()
        {
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