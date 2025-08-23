using static FinPlan.Web.Components.Pages.SavingsPlanner;
using System.IO;
using ClosedXML.Excel;

namespace FinPlan.Web.Services
{
    public interface IExcelExportService
    {
        byte[] GenerateSavingsExcel(SavingsResults results, 
                                   List<YearlyBreakdown> yearlyBreakdown, 
                                   SavingsCalculatorModel model);
    }
}
