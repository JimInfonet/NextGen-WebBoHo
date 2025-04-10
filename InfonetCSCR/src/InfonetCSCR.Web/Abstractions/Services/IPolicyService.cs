namespace InfonetCSCR.Web.Abstractions.Services;

public interface IPolicyService
{
  dynamic GetPolicy(string name, string pramValue);
  dynamic GetPolicy(string name, object? obj = null);
  dynamic GetNetworkUpdateSettings(string level, string value);
  PolicyChoiceViewModel ParsePolicyChoices(string choices);
  Task<bool> CheckIfPolicySettingExistsAsync(PolicyCompanyViewModel policySetting);
  Task<bool> AddPolicySettingAsync(PolicyCompanyViewModel policySetting);
  Task<bool> UpdatePolicySettingAsync(PolicyCompanyViewModel policySetting);
  Task<LogViewModel> GetSetVendPolicyLogAsync(PolicyCompanyViewModel policySetting);
  Task<bool> DeletePolicySettingAsync(PolicyCompanyViewModel policySetting);
  Task<IEnumerable<SubDepartmentViewModel>> GetSubDepartmentsListAsync();
  Task<IEnumerable<SubDetailViewModel>> GetSubDetailsListAsync();
  Task<IEnumerable<PriceGroupViewModel>> GetPriceGroupsListAsync();
  Task<IEnumerable<ClCategoryViewModel>> GetClCategoryListAsync();
  Task<IEnumerable<DiscCodeViewModel>> GetDiscCodesListAsync();
  Task<IEnumerable<BranchViewModel>> GetBranchesListAsync();
  Task<IEnumerable<UnitOfMeasureViewModel>> GetUomListAsync();
}
