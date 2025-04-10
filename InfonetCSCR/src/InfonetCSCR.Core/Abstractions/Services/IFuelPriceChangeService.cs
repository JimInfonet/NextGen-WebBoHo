namespace InfonetCSCR.Core.Abstractions.Services;

public interface IFuelPriceChangeService
{
  Task<StoreDataInHoDbRecord> CheckStoreDataInHeadOfficeDatabase(FuelPriceChangeStoreCheckRecord fuelPriceChangeStoreCheckRecord);
  Task<IList<GradeTierServiceLevelTableData>?> GetDataFromGradeTierServiceLevelTable(GradeGridRecord gradeGridRecord);
  Task<bool> TransferFuelPriceChange(IList<FuelPriceChangeTransferRecord> storesData);
  Task<IList<GradeTierServiceLevelTableData>?> GetFuelPriceChangeReportData(IList<string> storeIds, bool isFuelCp);
}
