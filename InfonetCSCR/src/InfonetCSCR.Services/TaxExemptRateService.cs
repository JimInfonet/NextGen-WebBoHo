namespace InfonetCSCR.Services;

public class TaxExemptRateService : ITaxExemptRateService
{
  private readonly IUnitOfWork _unitOfWork;

  public TaxExemptRateService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IEnumerable<TaxExemptRate>> GetAllTaxExemptRate()
  {
    IEnumerable<TaxExemptRate> taxExemptRates = await _unitOfWork.TaxExemptRates.GetAllTaxExemptRateAsync();
    _unitOfWork.Commit();
    return taxExemptRates;
  }
}
