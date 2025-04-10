namespace InfonetCSCR.Web.Abstractions.Services;

public interface IPolicyByClassService
{
  Task<SelectList> GetPolicyClassesAsync();
  Task<IEnumerable<PolicyCompanyViewModel>?> GetPoliciesByClass(string className);
  Task<IEnumerable<PolicyCompanyViewModel>> GetPolicySettingsByNameAsync(string policyName);
  Task<bool> UpdateTimesheetPayPeriodOnLoadAsync();
}
