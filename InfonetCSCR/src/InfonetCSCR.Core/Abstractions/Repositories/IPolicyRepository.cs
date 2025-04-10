namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IPolicyRepository
{
  Task<IEnumerable<PolicyCompany>> GetPolicyAllCompany();
  Task<IEnumerable<PolicyCanbe>> GetPolicyAllCanbe();
  Task<IEnumerable<PolicyClass>> GetPolicyAllClass();
  Task<IEnumerable<PolicySettings>> GetPolicyAllSettings();
  Task<bool> GetPolicyNetworkUpdateByPriceGroup();
  Task<bool> GetPolicyNetworkUpdateByRegion();
  Task<bool> GetPolicyNetworkUpdate();
  Task<bool> DeletePSetAsync(string departmentId, string? subDepartmentId = null, string? detailId = null);
  Task<bool> CheckPSetExistsAsync(string pName, string pSet, string? pLevel = null, string? pValue = null);
  Task<IEnumerable<PSet>> GetPSetsForExptTaxAndTaxNameAsync(IEnumerable<string> pSets);
  Task<IEnumerable<PSet>> GetPSetsByPNamePSetPLevelPValueAsync(
    IList<(string pName, string pSet, string pLevel, string pValue)> pSets);
  Task<bool> CheckIfPolicySettingExistsAsync(PolicyCompany policySetting);
  Task<bool> AddPolicySettingAsync(PolicyCompany policySetting);
  Task<bool> UpdatePolicySetSettingAsync(PolicyCompany policySetting);
  Task<bool> UpdatePolicyCompSettingAsync(PolicyCompany policySetting);
  Task<bool> UpdateTimesheetPayPeriodAsync(string initialEmpID);
  Task<bool> UpdateSetVendYesAsync();
  Task<bool> UpdateSetVendNoAsync();
  Task<bool> DeletePolicySettingAsync(PolicyCompany policySetting);
}
