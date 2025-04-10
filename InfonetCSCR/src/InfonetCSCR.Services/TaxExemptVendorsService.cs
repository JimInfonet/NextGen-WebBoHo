namespace InfonetCSCR.Services;

public class TaxExemptVendorsService : ITaxExemptVendorsService
{
  private readonly IUnitOfWork _unitOfWork;

  public TaxExemptVendorsService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IEnumerable<TaxExemptVendor>> GetAllTaxExemptVendors(string stockCode)
  {
    IEnumerable<TaxExemptVendor> taxExemptVendors = await _unitOfWork.TaxExemptVendors.GetAllTaxExemptVendorsAsync(stockCode);
    _unitOfWork.Commit();
    return taxExemptVendors;
  }
}
