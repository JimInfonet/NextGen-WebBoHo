namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IExcelReportsRepository
{
  Task<IList<SummaryFuelProductsReportDto>?> GetSummaryFuelProductsReportDataAsync(ExcelReportGenerator excelReportGenerator);
  Task<IList<FleetCardReportDto>?> GetNorth60OrYTGFleetCardReportDataAsync(ExcelReportGenerator excelReportGenerator, bool isYTG);
  Task<IList<SaleLine>?> GetAmountFromSaleLineAsync(string? fuelDept);
  Task<bool> CreateFNGTRSalesReportDataAsync(ExcelReportGenerator excelReportGenerator);
  Task<IList<ReportData>?> GetAllReportDataAsync();
}
