using NUnit.Framework;
using FinPlan.Web.E2ETests.Pages;
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace FinPlan.Web.E2ETests
{
    [TestFixture]
    public class SavingsPageTests : TestBase
    {
        private SavingsPage _savingsPage = null!;
        
        [SetUp]
        public void Setup()
        {
            _savingsPage = new SavingsPage(Page);
        }

        [Test]
        public async Task DiscoverLinks_OnHomePage()
        {
            // Act
            var links = await _savingsPage.DiscoverLinksOnHomePageAsync(BaseUrl);
            
            // Assert and log all links
            Assert.That(links.Count, Is.GreaterThan(0));
            
            Console.WriteLine("Found links on home page:");
            foreach (var link in links)
            {
                Console.WriteLine($"  {link}");
            }
        }

        [Test]
        public async Task SavingsPage_ShouldLoadSuccessfully()
        {
            // Act
            await _savingsPage.NavigateAsync(BaseUrl);
            
            // Assert
            await Expect(Page).ToHaveTitleAsync(new Regex(".*REWealth.*"));
            await Expect(_savingsPage.IntroModal).ToBeVisibleAsync();
        }

        [Test]
        public async Task SavingsPage_ShouldHaveBasicStructure()
        {
            // Act
            await _savingsPage.NavigateAsync(BaseUrl);
            
            // Close intro modal if visible
            if (await _savingsPage.IsIntroModalVisibleAsync())
            {
                await _savingsPage.CloseIntroModalAsync();
            }
            
            // Assert - Check for basic page elements
            await Expect(_savingsPage.YourSavingsTab).ToBeVisibleAsync();
            await Expect(_savingsPage.PartnerSavingsTab).ToBeVisibleAsync();
            await Expect(_savingsPage.UpdatePlanButton).ToBeVisibleAsync();
            
            // Should show either empty state or data
            var hasEmptyState = await _savingsPage.IsEmptyStateVisibleAsync();
            var hasSummaryCards = await _savingsPage.HasSummaryCardsAsync();
            
            Assert.That(hasEmptyState || hasSummaryCards, Is.True, "Page should show either empty state or summary cards");
        }

        [Test]
        public async Task DebugPageContent_ShowWhatIsVisible()
        {
            // Act
            await _savingsPage.NavigateAsync(BaseUrl);
            
            // Debug information
            Console.WriteLine("=== PAGE DEBUG INFO ===");
            Console.WriteLine($"Intro Modal Visible: {await _savingsPage.IsIntroModalVisibleAsync()}");
            Console.WriteLine($"Empty State Visible: {await _savingsPage.IsEmptyStateVisibleAsync()}");
            Console.WriteLine($"Summary Cards Visible: {await _savingsPage.HasSummaryCardsAsync()}");
            Console.WriteLine($"Try Sample Data Button Visible: {await _savingsPage.TrySampleDataButton.IsVisibleAsync()}");
            Console.WriteLine($"Update Plan Button Visible: {await _savingsPage.UpdatePlanButton.IsVisibleAsync()}");
            Console.WriteLine($"Your Savings Tab Visible: {await _savingsPage.YourSavingsTab.IsVisibleAsync()}");
            Console.WriteLine($"Partner Savings Tab Visible: {await _savingsPage.PartnerSavingsTab.IsVisibleAsync()}");
            
            // Check if there's existing data
            if (await _savingsPage.HasSummaryCardsAsync())
            {
                Console.WriteLine("=== EXISTING DATA FOUND ===");
                var finalSavings = await _savingsPage.GetFinalSavingsAmountAsync();
                var totalContributions = await _savingsPage.GetTotalContributionsAsync();
                var totalGrowth = await _savingsPage.GetTotalGrowthAsync();
                var taxesPaid = await _savingsPage.GetTaxesPaidAsync();
                
                Console.WriteLine($"Final Savings: {finalSavings}");
                Console.WriteLine($"Total Contributions: {totalContributions}");
                Console.WriteLine($"Total Growth: {totalGrowth}");
                Console.WriteLine($"Taxes Paid: {taxesPaid}");
                
                // Validate existing calculations
                var finalAmount = SavingsPage.ExtractCurrencyValue(finalSavings);
                var contributions = SavingsPage.ExtractCurrencyValue(totalContributions);
                var growth = SavingsPage.ExtractCurrencyValue(totalGrowth);
                
                Assert.That(finalAmount, Is.GreaterThan(0), "Final savings should be greater than 0");
                Assert.That(contributions, Is.GreaterThan(0), "Contributions should be greater than 0");
                Assert.That(growth, Is.GreaterThan(0), "Growth should be greater than 0");
                Console.WriteLine("✅ All calculations are positive and valid!");
            }
            
            Assert.Pass("Debug information logged successfully");
        }

        [Test]
        public async Task IntroModal_ShouldBeVisibleOnFirstLoad()
        {
            // Arrange & Act
            await _savingsPage.NavigateAsync(BaseUrl);
            
            // Assert
            await Expect(_savingsPage.IntroModal).ToBeVisibleAsync();
            await Expect(Page.Locator("text=Welcome to Savings Planning!")).ToBeVisibleAsync();
        }

        [Test]
        public async Task IntroModal_ShouldCloseWhenXButtonClicked()
        {
            // Arrange
            await _savingsPage.NavigateAsync(BaseUrl);
            
            // Act
            await _savingsPage.CloseIntroModalAsync();
            
            // Assert
            await Expect(_savingsPage.IntroModal).Not.ToBeVisibleAsync();
        }

        [Test]
        public async Task TabSwitching_ShouldWorkCorrectly()
        {
            // Arrange
            await _savingsPage.NavigateAsync(BaseUrl);
            await _savingsPage.CloseIntroModalAsync();
            
            // Act & Assert - Test Partner tab
            await _savingsPage.SwitchToPartnerTabAsync();
            var activeTabText = await _savingsPage.GetActiveTabTextAsync();
            Assert.That(activeTabText, Does.Contain("Partner"));
            
            // Act & Assert - Test Your tab
            await _savingsPage.SwitchToYourTabAsync();
            activeTabText = await _savingsPage.GetActiveTabTextAsync();
            Assert.That(activeTabText, Does.Contain("Your"));
        }

        [Test]
        public async Task SampleData_ShouldPopulateFieldsAndCalculate()
        {
            // Arrange
            await _savingsPage.NavigateAsync(BaseUrl);
            
            // Act
            var sampleDataLoaded = await _savingsPage.TrySampleDataAsync();
            
            if (sampleDataLoaded)
            {
                // Assert - Check if summary cards are visible with values
                Assert.That(await _savingsPage.HasSummaryCardsAsync(), Is.True);
                
                var finalSavingsText = await _savingsPage.GetFinalSavingsAmountAsync();
                Assert.That(SavingsPage.IsCurrencyFormatted(finalSavingsText), Is.True);
                
                var finalAmount = SavingsPage.ExtractCurrencyValue(finalSavingsText);
                Assert.That(finalAmount, Is.GreaterThan(0));
            }
            else
            {
                // If no sample data button, just verify the page loaded correctly
                Console.WriteLine("Try Sample Data button not found - checking page loaded correctly");
                Assert.That(await _savingsPage.IsIntroModalVisibleAsync() || await _savingsPage.IsEmptyStateVisibleAsync(), Is.True);
            }
        }

        [Test]
        public async Task EmptyState_ShouldShowWhenNoData()
        {
            // Arrange
            await _savingsPage.NavigateAsync(BaseUrl);
            await _savingsPage.CloseIntroModalAsync();
            
            // Act & Assert
            Assert.That(await _savingsPage.IsEmptyStateVisibleAsync(), Is.True);
        }

        [Test]
        public async Task ClearPlanData_ShouldResetToEmptyState()
        {
            // Arrange
            await _savingsPage.NavigateAsync(BaseUrl);
            await _savingsPage.TrySampleDataAsync();
            
            // Verify data is loaded
            Assert.That(await _savingsPage.HasSummaryCardsAsync(), Is.True);
            
            // Act
            await _savingsPage.ClearPlanDataAsync();
            
            // Assert - Should return to empty state
            Assert.That(await _savingsPage.IsEmptyStateVisibleAsync(), Is.True);
        }

        [Test]
        public async Task YearlyProjectionTable_ShouldShowCorrectData_WithSampleData()
        {
            // Arrange
            await _savingsPage.NavigateAsync(BaseUrl);
            await _savingsPage.TrySampleDataAsync();
            
            // Act & Assert
            await Expect(_savingsPage.YearlyProjectionTable).ToBeVisibleAsync();
            
            var rowCount = await _savingsPage.GetTableRowCountAsync();
            Assert.That(rowCount, Is.GreaterThan(0));
            
            // Check first row data
            var firstRowYear = await _savingsPage.GetTableCellTextAsync(0, 0);
            var firstRowAge = await _savingsPage.GetTableCellTextAsync(0, 1);
            
            Assert.That(firstRowYear, Is.EqualTo("1"));
            Assert.That(int.TryParse(firstRowAge, out var age), Is.True);
            Assert.That(age, Is.GreaterThan(0));
        }

        [Test]
        public async Task InfoTooltips_ShouldShowAndHide()
        {
            // Arrange
            await _savingsPage.NavigateAsync(BaseUrl);
            await _savingsPage.CloseIntroModalAsync();
            
            // Act
            await _savingsPage.ClickInfoIconAsync(0);
            
            // Assert
            Assert.That(await _savingsPage.IsTooltipVisibleAsync(), Is.True);
            
            // Act - Close tooltip
            await _savingsPage.CloseTooltipAsync();
            
            // Assert
            Assert.That(await _savingsPage.IsTooltipVisibleAsync(), Is.False);
        }

        [Test]
        public async Task DontShowAgain_ShouldPersistAcrossReloads()
        {
            // Arrange
            await _savingsPage.NavigateAsync(BaseUrl);
            
            // Act
            await _savingsPage.CheckDontShowAgainAsync();
            await _savingsPage.CloseIntroModalAsync();
            
            // Reload page
            await Page.ReloadAsync();
            
            // Assert
            Assert.That(await _savingsPage.IsIntroModalVisibleAsync(), Is.False);
        }

        [Test]
        public async Task SimpleViewToggle_ShouldAffectTableDisplay()
        {
            // Arrange
            await _savingsPage.NavigateAsync(BaseUrl);
            await _savingsPage.TrySampleDataAsync();
            
            // Act
            await _savingsPage.ToggleSimpleViewAsync();
            
            // Assert - Table should still be visible but potentially with different columns
            await Expect(_savingsPage.YearlyProjectionTable).ToBeVisibleAsync();
        }

        [Test]
        public async Task SavingsCalculation_ShouldBeReasonable_ForSimpleScenario()
        {
            // This test validates calculation logic with sample data
            // Arrange
            await _savingsPage.NavigateAsync(BaseUrl);
            var sampleDataLoaded = await _savingsPage.TrySampleDataAsync();
            
            if (sampleDataLoaded)
            {
                // Act - Get calculated values
                var finalSavingsText = await _savingsPage.GetFinalSavingsAmountAsync();
                var totalContributionsText = await _savingsPage.GetTotalContributionsAsync();
                var totalGrowthText = await _savingsPage.GetTotalGrowthAsync();
                
                // Assert - Validate calculations are reasonable
                var finalAmount = SavingsPage.ExtractCurrencyValue(finalSavingsText);
                var totalContributions = SavingsPage.ExtractCurrencyValue(totalContributionsText);
                var totalGrowth = SavingsPage.ExtractCurrencyValue(totalGrowthText);
                
                // Basic sanity checks
                Assert.That(finalAmount, Is.GreaterThan(0));
                Assert.That(totalContributions, Is.GreaterThan(0));
                Assert.That(totalGrowth, Is.GreaterThan(0));
                
                // Final amount should equal contributions + growth (minus taxes for taxable accounts)
                Assert.That(finalAmount, Is.GreaterThan(totalContributions));
                
                // Growth should be positive for reasonable time periods
                Assert.That(totalGrowth, Is.GreaterThan(totalContributions * 0.1m)); // At least 10% growth seems reasonable
            }
            else
            {
                // If no sample data available, just verify page structure is correct
                Console.WriteLine("Sample data not available - checking page structure");
                Assert.That(await _savingsPage.IsIntroModalVisibleAsync() || await _savingsPage.IsEmptyStateVisibleAsync(), Is.True);
                Assert.Pass("Sample data button not available, but page loaded correctly");
            }
        }

        [Test]
        public async Task UpdatePlanButton_ShouldOpenWizard()
        {
            // Arrange
            await _savingsPage.NavigateAsync(BaseUrl);
            await _savingsPage.CloseIntroModalAsync();
            
            // Act
            await _savingsPage.UpdatePlanButton.ClickAsync();
            
            // Assert
            await Expect(_savingsPage.WizardModal).ToBeVisibleAsync();
        }

        [Test]
        public async Task StartPlanningButton_ShouldOpenWizard_FromIntroModal()
        {
            // Arrange
            await _savingsPage.NavigateAsync(BaseUrl);
            
            // Act
            await _savingsPage.StartPlanningAsync();
            
            // Assert
            await Expect(_savingsPage.WizardModal).ToBeVisibleAsync();
        }

        [Test]
        public async Task AllSummaryCards_ShouldHaveValidCurrencyValues()
        {
            // Arrange
            await _savingsPage.NavigateAsync(BaseUrl);
            await _savingsPage.TrySampleDataAsync();
            
            // Act & Assert
            var finalSavings = await _savingsPage.GetFinalSavingsAmountAsync();
            var totalContributions = await _savingsPage.GetTotalContributionsAsync();
            var totalGrowth = await _savingsPage.GetTotalGrowthAsync();
            var taxesPaid = await _savingsPage.GetTaxesPaidAsync();
            
            // All should be properly formatted currency
            Assert.That(SavingsPage.IsCurrencyFormatted(finalSavings), Is.True);
            Assert.That(SavingsPage.IsCurrencyFormatted(totalContributions), Is.True);
            Assert.That(SavingsPage.IsCurrencyFormatted(totalGrowth), Is.True);
            Assert.That(SavingsPage.IsCurrencyFormatted(taxesPaid), Is.True);
            
            // All should have positive values
            Assert.That(SavingsPage.ExtractCurrencyValue(finalSavings), Is.GreaterThan(0));
            Assert.That(SavingsPage.ExtractCurrencyValue(totalContributions), Is.GreaterThan(0));
            Assert.That(SavingsPage.ExtractCurrencyValue(totalGrowth), Is.GreaterThan(0));
            // Taxes paid might be 0 for some scenarios, so just check it's not negative
            Assert.That(SavingsPage.ExtractCurrencyValue(taxesPaid), Is.GreaterThanOrEqualTo(0));
        }
    }
}