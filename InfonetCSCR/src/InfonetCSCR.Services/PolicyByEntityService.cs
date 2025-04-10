namespace InfonetCSCR.Services;

public class PolicyByEntityService : IPolicyByEntityService
{
  private readonly IUnitOfWork _unitOfWork;

  public PolicyByEntityService(IApplicationConfig config)
  {
    _unitOfWork = new UnitOfWork(config, config.CSCMasterConnectionString);
  }

  public async Task<IEnumerable<Entity>> GetEntityListAsync()
  {
    var entityList = await _unitOfWork.PolicyByEntity.GetEntityListAsync();
    _unitOfWork.Commit();
    return entityList;
  }

  public async Task<IEnumerable<PolicyCompany>> GetPolicySettingsByEntityAsync(Entity entity)
  {
    var policySettings = await _unitOfWork.PolicyByEntity.GetPolicySettingsByEntityAsync(entity);
    _unitOfWork.Commit();
    return policySettings;
  }
}
