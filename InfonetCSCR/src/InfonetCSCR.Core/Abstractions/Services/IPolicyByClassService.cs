namespace InfonetCSCR.Core.Abstractions.Services;

public interface IPolicyByClassService
{
  Task<IEnumerable<PolicyClass>> GetPolicyClassesAsync();
  Task<IEnumerable<PolicyCompany>> GetPoliciesByClassAsync(string className);
  Task<IEnumerable<PolicyCompany>> GetPolicySettingsByNameAsync(string policyName);
  Task<bool> UpdateTimesheetPayPeriodOnLoadAsync(long initialEmpID);
}
