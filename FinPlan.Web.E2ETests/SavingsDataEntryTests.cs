using NUnit.Framework;
using FinPlan.Web.E2ETests.Pages;
using Microsoft.Playwright;

namespace FinPlan.Web.E2ETests
{
    [TestFixture]
    public class SavingsDataEntryTests : TestBase
    {
        private SavingsPage _savingsPage = null!;
        
        [SetUp]
        public void Setup()
        {
            _savingsPage = new SavingsPage(Page);
        }

        [Test]
        public async Task NewUser_ShouldFillOutCompleteSavingsPlan()
        {
            // Navigate directly to the savings page with showIntro=false to skip the intro modal
            var savingsUrl = $"{BaseUrl}/savings-wealth-building?showIntro=false";
            Console.WriteLine($"Navigating to: {savingsUrl}");
            
            await Page.GotoAsync(savingsUrl);
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Page.WaitForTimeoutAsync(2000);
            
            Console.WriteLine("Starting to fill out savings plan sections...");
            
            // ===== SECTION 1: Milestones & Ages =====
            Console.WriteLine("Editing Section 1: Milestones & Ages");
            
            // Find and click the first "Update" button (Milestones section)
            var milestonesUpdateButton = Page.Locator(".section-card").First.Locator("button:has-text('Update')");
            await milestonesUpdateButton.ClickAsync();
            
            // Wait for wizard/modal to appear
            await Page.WaitForTimeoutAsync(2000);
            
            // Fill in Current Age - it's in a card with "Current Age" title and currency-input class
            // Both age fields have class="form-control currency-input" type="number"
            var currentAgeCard = Page.Locator(".account-card:has-text('Current Age')");
            var currentAgeInput = currentAgeCard.Locator("input.currency-input[type='number']");
            
            await currentAgeInput.ClickAsync(); // Click to focus
            await currentAgeInput.FillAsync("");  // Clear first
            await currentAgeInput.FillAsync("35");
            Console.WriteLine("  ✓ Current Age: 35");
            
            await Page.WaitForTimeoutAsync(1000);
            
            // Fill in Retirement Age - it's in a card with "Planned Retirement Age" title
            var retirementAgeCard = Page.Locator(".account-card:has-text('Planned Retirement Age')");
            var retirementAgeInput = retirementAgeCard.Locator("input.currency-input[type='number']");
            
            await retirementAgeInput.ClickAsync();
            await retirementAgeInput.FillAsync("");
            await retirementAgeInput.FillAsync("65");
            Console.WriteLine("  ✓ Retirement Age: 65");
            
            await Page.WaitForTimeoutAsync(1000);
            
            // Click Next button in wizard
            var nextButton = Page.Locator("button:has-text('Next')").First;
            await nextButton.ClickAsync();
            Console.WriteLine("  → Clicked Next");
            await Page.WaitForTimeoutAsync(2000);
            
            // ===== SECTION 2: Starting Balances =====
            Console.WriteLine("\nEditing Section 2: Starting Balances");
            
            // Look for currency input fields (they have class="form-control currency-input")
            var currencyInputs = Page.Locator("input.currency-input[type='number']");
            var inputCount = await currencyInputs.CountAsync();
            Console.WriteLine($"  Found {inputCount} currency input fields");
            
            if (inputCount >= 3)
            {
                // Taxable Account - first currency input
                await currencyInputs.Nth(0).ClickAsync();
                await currencyInputs.Nth(0).FillAsync("50000");
                Console.WriteLine("  ✓ Taxable Starting Balance: $50,000");
                await Page.WaitForTimeoutAsync(500);
                
                // Traditional Account - second currency input
                await currencyInputs.Nth(1).ClickAsync();
                await currencyInputs.Nth(1).FillAsync("75000");
                Console.WriteLine("  ✓ Traditional Starting Balance: $75,000");
                await Page.WaitForTimeoutAsync(500);
                
                // Roth Account - third currency input
                await currencyInputs.Nth(2).ClickAsync();
                await currencyInputs.Nth(2).FillAsync("25000");
                Console.WriteLine("  ✓ Roth Starting Balance: $25,000");
                await Page.WaitForTimeoutAsync(500);
            }
            
            // Click Next
            nextButton = Page.Locator("button:has-text('Next')").First;
            await nextButton.ClickAsync();
            Console.WriteLine("  → Clicked Next");
            await Page.WaitForTimeoutAsync(2000);
            
            // ===== SECTION 3: Monthly Contributions =====
            Console.WriteLine("\nEditing Section 3: Monthly Contributions");
            
            // Look for currency input fields again
            var monthlyInputs = Page.Locator("input.currency-input[type='number']");
            var monthlyCount = await monthlyInputs.CountAsync();
            Console.WriteLine($"  Found {monthlyCount} monthly contribution input fields");
            
            if (monthlyCount >= 3)
            {
                // Taxable Monthly Contribution - first
                await monthlyInputs.Nth(0).ClickAsync();
                await monthlyInputs.Nth(0).FillAsync("500");
                Console.WriteLine("  ✓ Taxable Monthly Contribution: $500");
                await Page.WaitForTimeoutAsync(500);
                
                // Traditional Monthly Contribution - second
                await monthlyInputs.Nth(1).ClickAsync();
                await monthlyInputs.Nth(1).FillAsync("1500");
                Console.WriteLine("  ✓ Traditional Monthly Contribution: $1,500");
                await Page.WaitForTimeoutAsync(500);
                
                // Roth Monthly Contribution - third
                await monthlyInputs.Nth(2).ClickAsync();
                await monthlyInputs.Nth(2).FillAsync("500");
                Console.WriteLine("  ✓ Roth Monthly Contribution: $500");
                await Page.WaitForTimeoutAsync(500);
            }
            
            // Click Next or Finish
            var continueButton = Page.Locator("button:has-text('Next'), button:has-text('Finish')").First;
            await continueButton.ClickAsync();
            Console.WriteLine("  → Clicked Next/Finish");
            await Page.WaitForTimeoutAsync(2000);
            
            // ===== SECTION 4: Growth Rates =====
            Console.WriteLine("\nEditing Section 4: Growth Rates");
            
            // Look for growth rate input fields using card-based selectors
            var taxableGrowthCard = Page.Locator(".account-card.taxable:has-text('Taxable Growth Rate')");
            var taxableGrowthInput = taxableGrowthCard.Locator("input.currency-input[type='number']");
            
            if (await taxableGrowthInput.IsVisibleAsync())
            {
                await taxableGrowthInput.ClickAsync();
                await taxableGrowthInput.FillAsync("6.5");
                Console.WriteLine("  ✓ Taxable Growth Rate: 6.5%");
                await Page.WaitForTimeoutAsync(500);
            }
            
            var traditionalGrowthCard = Page.Locator(".account-card.traditional:has-text('Traditional Growth Rate')");
            var traditionalGrowthInput = traditionalGrowthCard.Locator("input.currency-input[type='number']");
            
            if (await traditionalGrowthInput.IsVisibleAsync())
            {
                await traditionalGrowthInput.ClickAsync();
                await traditionalGrowthInput.FillAsync("7.5");
                Console.WriteLine("  ✓ Traditional Growth Rate: 7.5%");
                await Page.WaitForTimeoutAsync(500);
            }
            
            var rothGrowthCard = Page.Locator(".account-card.roth:has-text('Roth Growth Rate')");
            var rothGrowthInput = rothGrowthCard.Locator("input.currency-input[type='number']");
            
            if (await rothGrowthInput.IsVisibleAsync())
            {
                await rothGrowthInput.ClickAsync();
                await rothGrowthInput.FillAsync("7.5");
                Console.WriteLine("  ✓ Roth Growth Rate: 7.5%");
                await Page.WaitForTimeoutAsync(500);
            }
            
            // Click Next to go to tax treatment step
            continueButton = Page.Locator("button:has-text('Save & Next'), button:has-text('Next')").First;
            await continueButton.ClickAsync();
            Console.WriteLine("  → Clicked Save & Next");
            await Page.WaitForTimeoutAsync(2000);
            
            // ===== SECTION 5: Tax Treatment =====
            Console.WriteLine("\nEditing Section 5: Tax Treatment");
            
            // Select Medium tax bracket - find the radio button in the traditional card
            var mediumBracketOption = Page.Locator(".tax-bracket-option:has-text('Medium')");
            
            if (await mediumBracketOption.IsVisibleAsync())
            {
                var radioButton = mediumBracketOption.Locator("input[type='radio']");
                await radioButton.ClickAsync();
                Console.WriteLine("  ✓ Tax Bracket: Medium (22-24%)");
                await Page.WaitForTimeoutAsync(500);
            }
            else
            {
                Console.WriteLine("  ⚠️ Tax bracket selector not visible");
            }
            
            // Click Save & Finish to complete the wizard
            var finishButton = Page.Locator("button:has-text('Save & Finish'), button:has-text('Finish')").First;
            if (await finishButton.IsVisibleAsync())
            {
                await finishButton.ClickAsync();
                Console.WriteLine("  → Clicked Save & Finish");
                await Page.WaitForTimeoutAsync(3000);
            }
            else
            {
                Console.WriteLine("  ⚠️ Finish button not visible - trying alternative");
                var saveCloseBtn = Page.Locator("button:has-text('Save & Close')");
                if (await saveCloseBtn.IsVisibleAsync())
                {
                    await saveCloseBtn.ClickAsync();
                    Console.WriteLine("  → Clicked Save & Close");
                    await Page.WaitForTimeoutAsync(3000);
                }
            }
            
            // ===== VERIFY RESULTS =====
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("VERIFYING CALCULATIONS");
            Console.WriteLine(new string('=', 60));
            
            // Wait for summary cards to appear
            await Page.WaitForTimeoutAsync(2000);
            
            // Check if summary cards are now visible
            var hasSummaryCards = await _savingsPage.HasSummaryCardsAsync();
            
            if (hasSummaryCards)
            {
                Console.WriteLine("\n✅ SUCCESS! Summary cards are visible.");
                
                var finalSavings = await _savingsPage.GetFinalSavingsAmountAsync();
                var totalContributions = await _savingsPage.GetTotalContributionsAsync();
                var totalGrowth = await _savingsPage.GetTotalGrowthAsync();
                var taxesPaid = await _savingsPage.GetTaxesPaidAsync();
                
                Console.WriteLine($"\n📊 CALCULATION RESULTS:");
                Console.WriteLine($"  💰 Final Savings:        {finalSavings}");
                Console.WriteLine($"  💵 Total Contributions:  {totalContributions}");
                Console.WriteLine($"  📈 Total Growth:         {totalGrowth}");
                Console.WriteLine($"  💸 Taxes Paid:           {taxesPaid}");
                
                // Validate the calculations
                var finalAmount = SavingsPage.ExtractCurrencyValue(finalSavings);
                var contributions = SavingsPage.ExtractCurrencyValue(totalContributions);
                var growth = SavingsPage.ExtractCurrencyValue(totalGrowth);
                
                Assert.That(finalAmount, Is.GreaterThan(0), "Final savings should be greater than 0");
                Assert.That(contributions, Is.GreaterThan(0), "Total contributions should be greater than 0");
                Assert.That(growth, Is.GreaterThan(0), "Total growth should be greater than 0");
                
                // Expected contributions over 30 years:
                // ($50k + $75k + $25k) initial + ($500 + $1500 + $500) * 12 * 30 monthly
                // = $150k initial + $2500 * 360 months = $150k + $900k = $1,050,000
                Console.WriteLine($"\n📊 VALIDATION:");
                Console.WriteLine($"  Expected total contributions: ~$1,050,000");
                Console.WriteLine($"  Actual total contributions: {totalContributions}");
                
                // Verify table exists
                var tableVisible = await _savingsPage.YearlyProjectionTable.IsVisibleAsync();
                Assert.That(tableVisible, Is.True, "Yearly projection table should be visible");
                
                var rowCount = await _savingsPage.GetTableRowCountAsync();
                Console.WriteLine($"\n📈 Yearly Projection Table: {rowCount} years of data");
                Assert.That(rowCount, Is.EqualTo(30), "Should have 30 years of projections (age 35-65)");
                
                Console.WriteLine("\n" + new string('=', 60));
                Console.WriteLine("✅ ALL VALIDATIONS PASSED!");
                Console.WriteLine(new string('=', 60));
            }
            else
            {
                Console.WriteLine("\n⚠️ Summary cards not visible yet - checking wizard state");
                
                // Take a screenshot for debugging
                await Page.ScreenshotAsync(new() { Path = "test-results/data-entry-final-state.png", FullPage = true });
                Console.WriteLine("  📸 Screenshot saved to: test-results/data-entry-final-state.png");
                
                // Check if wizard is still open
                var wizardVisible = await Page.Locator("button:has-text('Finish')").IsVisibleAsync();
                if (wizardVisible)
                {
                    Console.WriteLine("  ⚠️ Wizard still open - may need to click Finish again");
                }
                
                Assert.Fail("Summary cards should be visible after completing the wizard");
            }
        }
    }
}
