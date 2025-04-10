namespace InfonetCSCR.Web.Abstractions.Services;

public interface IExcelReportsService
{
  Task<bool?> CheckSaleDataExists(ExcelReportGeneratorViewModel excelReportGeneratorViewModel);
  Task<FileBase64DataViewModel?> FuelDailySalesReport(ExcelReportGeneratorViewModel excelReportGeneratorViewModel);
  Task<FileBase64DataViewModel?> FLOrFDOrDSSummaryReport(ExcelReportGeneratorViewModel excelReportGeneratorViewModel);
  Task<FileBase64DataViewModel?> SalesSummaryFuelProductsReport(ExcelReportGeneratorViewModel excelReportGeneratorViewModel);
  Task<FileBase64DataViewModel?> North60OrYTGFleetCardReport(ExcelReportGeneratorViewModel excelReportGeneratorViewModel);
  Task<FileBase64DataViewModel?> SalesByHourReport(ExcelReportGeneratorViewModel excelReportGeneratorViewModel);
  Task<FileBase64DataViewModel?> FNGTRFuelSalesReport(ExcelReportGeneratorViewModel excelReportGeneratorViewModel);
  Task<FileBase64DataViewModel> ConvertTextReportToExcelAsync(TextReportParamsViewModel textReportParams);
}
