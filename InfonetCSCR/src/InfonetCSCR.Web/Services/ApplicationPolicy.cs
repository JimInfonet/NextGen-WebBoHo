namespace InfonetCSCR.Web.Services;

public class ApplicationPolicy : IApplicationPolicy
{
  private readonly List<PolicyCompanyViewModel>? _company = null;
  private readonly List<PolicySettingsViewModel>? _settings = null;
  private readonly List<PolicyClassViewModel>? _class = null;
  private readonly List<PolicyCanbeViewModel>? _canbe = null;
  private readonly IApplicationPolicyService _policy;

  public ApplicationPolicy(IApplicationPolicyService policy)
  {
    _policy = policy;
  }

  public List<PolicyCompanyViewModel> PolicyCompany => _company ?? _policy.GetCompany().Result;
  public List<PolicySettingsViewModel> PolicySettings => _settings ?? _policy.GetSettings().Result;
  public List<PolicyClassViewModel> PolicyClass => _class ?? _policy.GetClass().Result;
  public List<PolicyCanbeViewModel> PolicyCanbe => _canbe ?? _policy.GetCanbe().Result;
}
