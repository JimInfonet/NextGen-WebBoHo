namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IFuelPriceChangeRepository
{
  Task<bool> DeleteStoreAsync(IList<FuelPriceChangeTransferRecord> storeIds);
  Task<int> GetReportIdAsync(string tableName);
  Task<IEnumerable<FuelPriceChangeTransferRecord>> GetMaxReportIdsAsync(IEnumerable<string> storeIDs);
  Task<bool> AddNewFuelPricesAsync(IList<FuelPriceChangeTransferRecord> stores);
  Task<bool> AddNewFuelPriceHistsAsync(IList<FuelPriceChangeTransferRecord> stores);
  Task<IList<FuelPriceHistory>> GetAllFuelPriceHistoryByStoreIdAndReportIdAsync(IList<FuelPriceChangeTransferRecord> stores);
  Task<IList<GradeTierServiceLevelTableData>> GetFuelPriceChangeReportDataAsync(IList<string> storeIds);
}
