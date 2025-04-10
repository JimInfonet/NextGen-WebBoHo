namespace InfonetCSCR.Core.Abstractions.Services;

public interface ITillsService
{
  Task<string> GetPumpFeatures();
  Task<int> GetMaxTill(int tillNo);
  Task<IList<Till>> GetTills(int maxTill, bool isCashBonusPolicy);
  Task<bool> AddTill(Till till);
  Task<int> GetTillsCount(int tillNo);
  Task<bool> DeleteTill(int tillNo);
  Task<bool> UpdateTillByFlipActiveOrFlipProcess(Till till);
}
