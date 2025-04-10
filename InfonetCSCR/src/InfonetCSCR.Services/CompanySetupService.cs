namespace InfonetCSCR.Services;

public class CompanySetupService : ICompanySetupService
{
  private readonly IUnitOfWork _unitOfWork;

  public CompanySetupService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCAdminConnectionString);
  }

  public async Task<Company?> GetCompany()
  {
    Company? company = await _unitOfWork.CompanySetups.GetCompanyAsync();
    _unitOfWork.Commit();
    return company;
  }

  public async Task<string?> GetCompanyName()
  {
    Company? company = await _unitOfWork.CompanySetups.GetCompanyAsync();
    _unitOfWork.Commit();
    return company?.CompanyName;
  }

  public async Task<string?> GetCompanyStoreCode()
  {
    Company? company = await _unitOfWork.CompanySetups.GetCompanyAsync();
    _unitOfWork.Commit();
    return company?.StoreCode;
  }
}
