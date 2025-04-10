namespace InfonetCSCR.Core.Abstractions.Services;

public interface IPolicyService
{
  Task<IEnumerable<PolicyCanbe>> GetPolicyAllCanbe();
  Task<IEnumerable<PolicyClass>> GetPolicyAllClass();
  Task<IEnumerable<PolicyCompany>> GetPolicyAllCompany();
  Task<IEnumerable<PolicySettings>> GetPolicyAllSettings();
  Task<bool> GetPolicyNetworkUpdateByPriceGroup();
  Task<bool> GetPolicyNetworkUpdateByRegion();
  Task<bool> GetPolicyNetworkUpdate();
  Task<bool> CheckTaxExempted(string stockCode, string taxName, string exptTaxPolicy);
  Task<IDictionary<(string stockCode, string taxName), bool>> GetTaxExemptedOrNotDictionary(
    IList<(string stockCode, string taxName)> stockCodeAndTaxNamePairs,
    string exptTaxPolicy);
  Task<bool> CheckIfPolicySettingExistsAsync(PolicyCompany policySetting);
  Task<bool> AddPolicySettingAsync(PolicyCompany policySetting);
  Task<bool> UpdatePolicySettingAsync(PolicyCompany policySetting);
  Task<bool> DeletePolicySettingAsync(PolicyCompany policySetting);
}