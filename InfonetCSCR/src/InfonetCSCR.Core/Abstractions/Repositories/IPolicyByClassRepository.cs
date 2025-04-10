namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IPolicyByClassRepository
{
  Task<IEnumerable<PolicyClass>> GetPolicyClassesAsync();
  Task<IEnumerable<PolicyCompany>> GetPoliciesByClassAsync(string className);
  Task<IEnumerable<PolicyCompany>> GetPolicySettingsByNameAsync(string policyName);
  Task<bool> UpdateTimesheetPayPeriodOnLoadAsync(long initialEmpID);
}
