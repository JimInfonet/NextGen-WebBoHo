namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IPolicyByEntityRepository
{
  Task<IEnumerable<Entity>> GetEntityListAsync();
  Task<IEnumerable<PolicyCompany>> GetPolicySettingsByEntityAsync(Entity entity);
}
