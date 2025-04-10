namespace InfonetCSCR.Core.Abstractions;

public interface IExcelReportsService
{
  Task<bool> CheckSaleDataExists(ExcelReportGenerator excelReportGenerator);
  Task<ExcelReportDataDto?> GetFuelDailySalesReportData(DateTime shiftDate);
  Task<FuelLitresSummaryReportDto> GetFuelLitresOrDollarsSummaryReportData();
  Task<IList<string>?> GetUniqueStockCodes();
  Task<IList<StockItem>?> GetAllStockMaster();
  Task<IList<Grade>?> GetGradesByFuelType();
  Task<IList<SaleHeadDto>?> GetDistinctDatesFromSaleHead(ExcelReportGenerator excelReportGenerator);
  Task<IList<SummaryFuelProductsReportDto>?> GetSummaryFuelProductsReportData(ExcelReportGenerator excelReportGenerator);
  Task<IList<FleetCardReportDto>?> GetNorth60OrYTGFleetCardReportData(ExcelReportGenerator excelReportGenerator, bool isYTG);
  Task<IList<SaleLine>?> GetAmountFromSaleLine();
  Task<IList<SaleHead>?> GetSalesByHourReport(ExcelReportGenerator excelReportGenerator);
  Task<IList<ReportData>?> GetAllReportData();
}
