namespace InfonetCSCR.Web.Abstractions.Services;

public interface ITillsService
{
  TillsPoliciesViewModel GetPolicies();
  Task<string> GetPumpFeatures();
  Task<int> GetMaxTill(int tillNo);
  Task<IList<TillViewModel>> GetTills(int maxTill);
  Task<bool> AddTill(TillViewModel till);
  Task<int> GetTillsCount(int tillNo);
  Task<bool> DeleteTill(int tillNo);
  Task<bool> UpdateTillByFlipActiveOrFlipProcess(TillViewModel till);
}
