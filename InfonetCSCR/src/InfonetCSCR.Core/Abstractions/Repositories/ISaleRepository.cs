namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ISaleRepository
{
  Task<bool> SaleExistsForStockCodeAsync(string stockCode);
  Task<bool> ClearSaleDataAsync();
  Task<bool> CreateFuelNonStatusDataAsync(ExcelReportGenerator excelReportGenerator);
  Task<bool> CreateFuelStatusDataAsync(ExcelReportGenerator excelReportGenerator);
  Task<bool> IsSaleDataExistsAsync();
  Task<IList<SaleData>?> GetAllSaleDataAsync();
  Task<bool> CreateSaleNonStatusDataAsync(ExcelReportGenerator excelReportGenerator);
  Task<bool> CreateSaleStatusDataAsync(ExcelReportGenerator excelReportGenerator);
  Task<bool> CreateSaleNonStatusDataByFamilyGroupAsync(ExcelReportGenerator excelReportGenerator);
  Task<bool> CreateSaleStatusDataByFamilyGroupAsync(ExcelReportGenerator excelReportGenerator);
}
