namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ITillsRepository
{
  Task<string> GetPumpFeaturesAsync();
  Task<int> GetMaxTillAsync(int tillNo);
  Task<IList<Till>> GetTillsAsync(int maxTill, bool isCashBonusPolicy);
  Task<int> ClearTillAsync(string[] tables, int tillNum);
  Task<bool> AddTillAsync(Till till);
  Task<int> GetTillsCountAsync(int tillNo);
  Task<bool> DeleteTillAsync(int tillNo);
  Task<bool> UpdateTillByFlipActiveOrFlipProcessAsync(Till till);
}
