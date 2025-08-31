using FinPlan.Web.Models;
using System.IO;
using ClosedXML.Excel;
using FinPlan.Shared.Models;

namespace FinPlan.Web.Services
{
    public interface IExcelExportService
    {
        byte[] GenerateSavingsExcel(SavingsResults results, 
                                   List<YearlyBreakdown> yearlyBreakdown, 
                                   SavingsCalculatorModel model);
                                    
        byte[] GenerateRetirementSpendingExcel(SpendingResults results,
                                  List<YearlySpendingBreakdown> yearlyBreakdown,
                                  SpendingPlanModel model);
    }
}
