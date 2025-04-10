namespace InfonetCSCR.Core.Abstractions.Services;

public interface IPolicyByEntityService
{
  Task<IEnumerable<Entity>> GetEntityListAsync();
  Task<IEnumerable<PolicyCompany>> GetPolicySettingsByEntityAsync(Entity entity);
}
