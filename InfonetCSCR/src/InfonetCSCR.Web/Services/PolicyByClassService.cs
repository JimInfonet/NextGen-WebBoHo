namespace InfonetCSCR.Web.Services;

public class PolicyByClassService : IPolicyByClassService
{
  private readonly IHttpService _http;
  private readonly IPolicyService _policyService;

  public PolicyByClassService(IHttpService http, IPolicyService policyService)
  {
    _http = http;
    _policyService = policyService;
  }

  public async Task<SelectList> GetPolicyClassesAsync()
  {
    const string url = "PolicyByClass/GetPolicyClasses";
    var policies = await _http.GetAndParseAsync<IEnumerable<PolicyClassViewModel>?>(url);
    bool timeSheets = _policyService.GetPolicy(PolicyName.TIMESHEETS);

    if (!timeSheets)
    {
      policies = policies.Where(policy => policy.Class != "TIMESHEETS");
    }

    return new SelectList(policies, "Class", "ClassName");
  }

  public async Task<IEnumerable<PolicyCompanyViewModel>?> GetPoliciesByClass(string className)
  {
    if (string.IsNullOrEmpty(className))
    {
      return Enumerable.Empty<PolicyCompanyViewModel>();
    }

    string url = $"PolicyByClass/GetPoliciesByClass?className={className}";
    var policies = await _http.GetAndParseAsync<IEnumerable<PolicyCompanyViewModel>?>(url);

    foreach (var policy in policies)
    {
      policy.ParsedChoices = _policyService.ParsePolicyChoices(policy.Choices);
      policy.LevelsList = GetLevelsList(policy.Levels);
    }

    return policies;
  }

  public async Task<IEnumerable<PolicyCompanyViewModel>?> GetPolicySettingsByNameAsync(string policyName)
  {
    if (string.IsNullOrEmpty(policyName))
    {
      return Enumerable.Empty<PolicyCompanyViewModel>();
    }

    string url = $"PolicyByClass/GetPolicySettingsByName?policyName={policyName}";
    var setings = await _http.GetAndParseAsync<IEnumerable<PolicyCompanyViewModel>?>(url);
    return setings;
  }

  public async Task<bool> UpdateTimesheetPayPeriodOnLoadAsync()
  {
    if (_policyService.GetPolicy(PolicyName.HAVE_USERID))
    {
      var initUserID = _policyService.GetPolicy(PolicyName.INITUSERID);
      string url = $"PolicyByClass/UpdateTimesheetPayPeriodOnLoad?initialEmpID={initUserID}";
      return await _http.GetAndParseAsync<bool>(url);
    }

    return false;
  }

  #region Private Methods

  private List<string> GetLevelsList(string levels)
  {
    if (string.IsNullOrEmpty(levels))
    {
      return new List<string>();
    }

    levels = levels.Trim('{', '}');
    return levels.Split(',').Select(level => level.Trim()).ToList();
  }

  #endregion
}
