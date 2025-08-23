using ClosedXML.Excel;
using System.IO;
using static FinPlan.Web.Components.Pages.SavingsPlanner;

namespace FinPlan.Web.Services
{
    public class ExcelExportService : IExcelExportService
    {
        public byte[] GenerateSavingsExcel(SavingsResults results, 
                                         List<YearlyBreakdown> yearlyBreakdown, 
                                         SavingsCalculatorModel model)
        {
            using (var workbook = new XLWorkbook())
            {
                // Create Summary Sheet
                var summarySheet = workbook.Worksheets.Add("Summary");
                FormatSummarySheet(summarySheet, results, model);
                
                // Create Details Sheet
                var detailsSheet = workbook.Worksheets.Add("Yearly Breakdown");
                FormatDetailsSheet(detailsSheet, yearlyBreakdown, model);
                
                // Convert to byte array
                using (var ms = new MemoryStream())
                {
                    workbook.SaveAs(ms);
                    return ms.ToArray();
                }
            }
        }
        
        private void FormatSummarySheet(IXLWorksheet sheet, SavingsResults results, SavingsCalculatorModel model)
        {
            // Add title and styling
            sheet.Cell("A1").Value = "Retirement Savings Plan Summary";
            sheet.Range("A1:F1").Merge().Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            
            // Add subtitle with generation date
            sheet.Cell("A2").Value = $"Generated on {DateTime.Now:MMMM d, yyyy}";
            sheet.Range("A2:F2").Merge().Style.Font.SetItalic().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            
            // Add key metrics section header
            sheet.Cell("A4").Value = "Key Metrics";
            sheet.Cell("A4").Style.Font.SetBold().Font.SetFontSize(12);
            
            // Add key metrics
            sheet.Cell("A5").Value = "Total Retirement Balance:";
            sheet.Cell("B5").Value = results.FinalAmount;
            sheet.Cell("B5").Style.NumberFormat.Format = "$#,##0.00";
            sheet.Cell("B5").Style.Font.SetBold();
            
            sheet.Cell("A6").Value = "Total Contributions:";
            sheet.Cell("B6").Value = results.TotalContributions;
            sheet.Cell("B6").Style.NumberFormat.Format = "$#,##0.00";
            
            sheet.Cell("A7").Value = "Total Interest Earned:";
            sheet.Cell("B7").Value = results.TotalInterestEarned;
            sheet.Cell("B7").Style.NumberFormat.Format = "$#,##0.00";
            sheet.Cell("B7").Style.Font.SetFontColor(XLColor.Green);
            
            sheet.Cell("A8").Value = "Total Taxes Paid:";
            sheet.Cell("B8").Value = results.TotalTaxesPaid;
            sheet.Cell("B8").Style.NumberFormat.Format = "$#,##0.00";
            sheet.Cell("B8").Style.Font.SetFontColor(XLColor.Red);
            
            // Add account breakdown section header
            sheet.Cell("A10").Value = "Account Breakdown";
            sheet.Cell("A10").Style.Font.SetBold().Font.SetFontSize(12);
            
            // Add account breakdown
            sheet.Cell("A11").Value = "Traditional (401k/IRA):";
            sheet.Cell("B11").Value = results.TaxDeferredBalance;
            sheet.Cell("B11").Style.NumberFormat.Format = "$#,##0.00";
            
            sheet.Cell("A12").Value = "Roth Accounts:";
            sheet.Cell("B12").Value = results.RothBalance;
            sheet.Cell("B12").Style.NumberFormat.Format = "$#,##0.00";
            
            sheet.Cell("A13").Value = "Taxable Accounts:";
            sheet.Cell("B13").Value = results.TaxableBalance;
            sheet.Cell("B13").Style.NumberFormat.Format = "$#,##0.00";
            
            // Add plan parameters section header
            sheet.Cell("A15").Value = "Plan Parameters";
            sheet.Cell("A15").Style.Font.SetBold().Font.SetFontSize(12);
            
            // Add plan parameters
            sheet.Cell("A16").Value = "Current Age:";
            sheet.Cell("B16").Value = model.CurrentAge;
            
            sheet.Cell("A17").Value = "Retirement Age:";
            sheet.Cell("B17").Value = model.RetirementAge;
            
            sheet.Cell("A18").Value = "Years to Retirement:";
            sheet.Cell("B18").Value = model.Years;
            
            sheet.Cell("A19").Value = "Annual Growth Rate:";
            sheet.Cell("B19").Value = $"{model.AnnualGrowthRate}%";
            
            sheet.Cell("A20").Value = "Monthly Taxable Contribution:";
            sheet.Cell("B20").Value = model.MonthlyTaxableContribution;
            sheet.Cell("B20").Style.NumberFormat.Format = "$#,##0.00";
            
            sheet.Cell("A21").Value = "Monthly Traditional Contribution:";
            sheet.Cell("B21").Value = model.MonthlyTraditionalContribution;
            sheet.Cell("B21").Style.NumberFormat.Format = "$#,##0.00";
            
            sheet.Cell("A22").Value = "Monthly Roth Contribution:";
            sheet.Cell("B22").Value = model.MonthlyRothContribution;
            sheet.Cell("B22").Style.NumberFormat.Format = "$#,##0.00";
            
            // Add disclaimer
            sheet.Cell("A24").Value = "This projection is based on the provided inputs and assumptions. Actual results may vary.";
            sheet.Range("A24:F24").Merge().Style.Font.SetItalic().Font.SetFontSize(8);
            
            // Auto-fit columns
            sheet.Columns().AdjustToContents();
            
            // Add a border around the summary sections
            sheet.Range($"A4:B8").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            sheet.Range($"A10:B13").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            sheet.Range($"A15:B22").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }
        
        private void FormatDetailsSheet(IXLWorksheet sheet, List<YearlyBreakdown> yearlyBreakdown, SavingsCalculatorModel model)
        {
            // Headers
            sheet.Cell("A1").Value = "Year";
            sheet.Cell("B1").Value = "Age";
            sheet.Cell("C1").Value = "Total Balance";
            sheet.Cell("D1").Value = "Interest Earned";
            sheet.Cell("E1").Value = "Contributions";
            sheet.Cell("F1").Value = "Taxable Balance";
            sheet.Cell("G1").Value = "Traditional Balance";
            sheet.Cell("H1").Value = "Roth Balance";
            sheet.Cell("I1").Value = "Taxes Paid";
            
            // Style the header row
            sheet.Range("A1:I1").Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray);
            
            // Populate data
            int row = 2;
            foreach (var year in yearlyBreakdown)
            {
                sheet.Cell(row, 1).Value = year.Year;
                sheet.Cell(row, 2).Value = model.CurrentAge + year.Year;
                sheet.Cell(row, 3).Value = year.Balance;
                sheet.Cell(row, 4).Value = year.InterestEarned;
                sheet.Cell(row, 5).Value = year.ContributionsThisYear;
                sheet.Cell(row, 6).Value = year.TaxableBalance;
                sheet.Cell(row, 7).Value = year.TaxDeferredBalance;
                sheet.Cell(row, 8).Value = year.RothBalance;
                sheet.Cell(row, 9).Value = year.TaxesPaid;
                
                // Apply currency formatting
                sheet.Range($"C{row}:I{row}").Style.NumberFormat.Format = "$#,##0.00";
                
                // Apply conditional formatting (highlight milestones)
                if (year.Balance >= 1000000)
                {
                    sheet.Range($"A{row}:I{row}").Style.Fill.SetBackgroundColor(XLColor.LightGreen);
                }
                else if (year.Balance >= 500000)
                {
                    sheet.Range($"A{row}:I{row}").Style.Fill.SetBackgroundColor(XLColor.LightYellow);
                }
                
                row++;
            }
            
            // Add totals row
            sheet.Cell(row, 1).Value = "TOTAL";
            sheet.Range($"A{row}:B{row}").Merge().Style.Font.SetBold();
            
            // Sum formulas
            sheet.Cell(row, 3).FormulaA1 = $"SUM(C2:C{row-1})";
            sheet.Cell(row, 4).FormulaA1 = $"SUM(D2:D{row-1})";
            sheet.Cell(row, 5).FormulaA1 = $"SUM(E2:E{row-1})";
            sheet.Cell(row, 6).FormulaA1 = $"SUM(F2:F{row-1})";
            sheet.Cell(row, 7).FormulaA1 = $"SUM(G2:G{row-1})";
            sheet.Cell(row, 8).FormulaA1 = $"SUM(H2:H{row-1})";
            sheet.Cell(row, 9).FormulaA1 = $"SUM(I2:I{row-1})";
            
            // Style totals row
            sheet.Range($"A{row}:I{row}").Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray);
            sheet.Range($"C{row}:I{row}").Style.NumberFormat.Format = "$#,##0.00";
            
            // Auto-fit columns
            sheet.Columns().AdjustToContents();
            
            // Add borders
            sheet.Range($"A1:I{row}").Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            sheet.Range($"A1:I{row}").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            
            // Set print settings
            sheet.PageSetup.PaperSize = XLPaperSize.LetterPaper;
            sheet.PageSetup.FitToPages(1, 0);
            sheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            sheet.PageSetup.SetRowsToRepeatAtTop(1, 1); // Repeat headers
            sheet.PageSetup.Header.Center.AddText("Retirement Savings Plan - Yearly Breakdown");
            sheet.PageSetup.Footer.Right.AddText("Page &P of &N");
            sheet.PageSetup.Footer.Left.AddText("Generated on &D");
        }
    }
}
