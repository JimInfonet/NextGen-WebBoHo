namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IGradeRepository
{
  Task<IList<Grade>> GetGradeByStoreIdsAsync(IList<string> storeIds);
  Task<IList<GradeTierServiceLevelTableData>> GetDataFromGradeTierServiceLevelTableAsync(string storeId);
  Task<IList<Grade>?> GetGradeByJoiningGradeSaleLineSaleHeadTables(DateTime shiftDate, string? department);
  Task<IList<string>?> GetUniqueStockCodesAsync();
  Task<IList<Grade>?> GetGradesByFuelTypeAsync();
}
