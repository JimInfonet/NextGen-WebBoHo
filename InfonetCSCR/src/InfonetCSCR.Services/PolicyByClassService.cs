namespace InfonetCSCR.Services;

public class PolicyByClassService : IPolicyByClassService
{
  private readonly IUnitOfWork _unitOfWork;

  public PolicyByClassService(IApplicationConfig config)
  {
    _unitOfWork = new UnitOfWork(config, config.CSCMasterConnectionString);
  }

  public async Task<IEnumerable<PolicyClass>> GetPolicyClassesAsync()
  {
    var items = await _unitOfWork.PolicyByClass.GetPolicyClassesAsync();
    _unitOfWork.Commit();
    return items;
  }

  public async Task<IEnumerable<PolicyCompany>> GetPoliciesByClassAsync(string className)
  {
    var items = await _unitOfWork.PolicyByClass.GetPoliciesByClassAsync(className);
    _unitOfWork.Commit();
    return items;
  }

  public async Task<IEnumerable<PolicyCompany>> GetPolicySettingsByNameAsync(string policyName)
  {
    var items = await _unitOfWork.PolicyByClass.GetPolicySettingsByNameAsync(policyName);
    _unitOfWork.Commit();
    return items;
  }

  public async Task<bool> UpdateTimesheetPayPeriodOnLoadAsync(long initialEmpID)
  {
    var resutl = await _unitOfWork.PolicyByClass.UpdateTimesheetPayPeriodOnLoadAsync(initialEmpID);
    _unitOfWork.Commit();
    return resutl;
  }
}
