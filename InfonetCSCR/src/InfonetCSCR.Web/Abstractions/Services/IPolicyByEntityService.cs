namespace InfonetCSCR.Web.Abstractions.Services;

public interface IPolicyByEntityService
{
  Task<SelectList> GetEntityListAsync();
  Task<IEnumerable<PolicyCompanyViewModel>> GetPolicySettingsByEntityAsync(EntityViewModel entity);
}
