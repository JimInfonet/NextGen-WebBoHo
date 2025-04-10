namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ISaleHeadRepository
{
  Task<IList<SaleHeadAndSaleLine>?> GetSaleData(DateTime shiftDate, string? fuelDept);
  Task<IList<SaleHeadAndSaleLine>> GetSaleHeadAndSaleLineDataByStockCodesAsync(IList<string> stockCodes, DateTime shiftDate, string? fuelDept);
  Task<IList<SaleHeadAndSaleLine>> GetSaleHeadAndSaleLineDataByGradeIdsAsync(IList<int?> gradeIds, DateTime shiftDate, string? fuelDept);
  Task<decimal> GetTotalDiscountByShiftDateAsync(DateTime shiftDate, string? fuelDept);
  Task<IList<SaleHeadDto>> GetDistinctDatesFromSaleHeadAsync(ExcelReportGenerator excelReportGenerator);
  Task<IList<SaleHead>?> GetSalesByHourReportAsync(ExcelReportGenerator excelReportGenerator);
}
