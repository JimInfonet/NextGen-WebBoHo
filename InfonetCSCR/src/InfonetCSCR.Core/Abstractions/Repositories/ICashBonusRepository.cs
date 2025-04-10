namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ICashBonusRepository
{
  Task<IEnumerable<CashBonusChart>> GetCashBonusDataAsync(string groupID);
  Task DeleteExistingCashBonusRecordsAsync(string groupID);
  Task<bool> AddCashBonusDataAsync(CashBonusChart cashBonusChart);
}
