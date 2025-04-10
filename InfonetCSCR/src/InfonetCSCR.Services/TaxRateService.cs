namespace InfonetCSCR.Services;

public class TaxRateService : ITaxRateService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly ITaxMasterService _taxMasterService;

  public TaxRateService(ITaxMasterService taxMasterService, IApplicationConfig config)
  {
    _taxMasterService = taxMasterService;
    _unitOfWork = new UnitOfWork(config, config.CSCMasterConnectionString);
  }

  public async Task<IEnumerable<TaxRate>> GetTaxRatesByTaxName(string taxName)
  {
    var taxRates = await _unitOfWork.TaxRate.GetTaxRatesByTaxNameAsync(taxName);
    _unitOfWork.Commit();
    return taxRates;
  }

  public async Task<bool> TaxCodeAlreadyExists(TaxRate taxRate)
  {
    var alreadyExists = await _unitOfWork.TaxRate.TaxCodeAlreadyExistsAsync(taxRate);
    _unitOfWork.Commit();
    return alreadyExists;
  }

  public async Task<bool> AddTaxRate(TaxRate taxRate)
  {
    var isAdded = await _unitOfWork.TaxRate.AddTaxRateAsync(taxRate);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> UpdateTaxRate(TaxRate taxRate)
  {
    var isUpdated = await _unitOfWork.TaxRate.UpdateTaxRateAsync(taxRate);
    _unitOfWork.Commit();
    return isUpdated;
  }

  public async Task<bool> DeleteTaxRate(string taxName, string taxCode)
  {
    bool isDeleted = await _taxMasterService.DeleteTaxRate(taxName, taxCode);
    _unitOfWork.Commit();
    return isDeleted;
  }
}
