namespace InfonetCSCR.Services;

public class SecurityLevelService : ISecurityLevelService
{
  private readonly IUnitOfWork _unitOfWork;

  public SecurityLevelService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCAdminConnectionString);
  }

  public async Task<IEnumerable<SecurityLevel>?> GetSecurityLevels(int loggedInUserSecurityLevel)
  {
    IEnumerable<SecurityLevel>? securityLevels = await _unitOfWork.UserGroupSecurityLevel.GetSecurityLevels(loggedInUserSecurityLevel);
    _unitOfWork.Commit();

    return securityLevels;
  }
}
