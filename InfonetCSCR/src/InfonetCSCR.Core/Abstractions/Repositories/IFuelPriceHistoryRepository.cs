namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IFuelPriceHistoryRepository
{
  Task<IList<FuelPriceHistory>> GetFuelPriceHistoryByGradeId(IList<int?> gradeIds);
  Task<IList<FuelPriceHistory>> GetAllFuelPriceHistoryByStoreIdAndReportId(string storeId, int reportId);
  Task<int> GetMaximumReportIdByStoreId(string storeId);
}
