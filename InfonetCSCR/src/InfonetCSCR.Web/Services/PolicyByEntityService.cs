namespace InfonetCSCR.Web.Services;

public class PolicyByEntityService : IPolicyByEntityService
{
  private readonly IHttpService _http;
  private readonly IPolicyService _policyService;

  public PolicyByEntityService(IHttpService http, IPolicyService policyService)
  {
    _http = http;
    _policyService = policyService;
  }

  public async Task<SelectList> GetEntityListAsync()
  {
    const string url = "PolicyByEntity/GetEntityList";
    var entityList =  await _http.GetAndParseAsync<IEnumerable<EntityViewModel>>(url);
    return new SelectList(entityList, "EntityName", "Description");
  }

  public async Task<IEnumerable<PolicyCompanyViewModel>> GetPolicySettingsByEntityAsync(EntityViewModel entity)
  {
    const string url = "PolicyByEntity/GetPolicySettingsByEntity";
    var policySettings = await _http.PostAndParseAsync<IEnumerable<PolicyCompanyViewModel>>(url, entity);

    foreach (var policy in policySettings)
    {
      policy.ParsedChoices = _policyService.ParsePolicyChoices(policy.Choices);
    }

    return policySettings;
  }
}
