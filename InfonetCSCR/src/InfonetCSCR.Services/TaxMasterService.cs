namespace InfonetCSCR.Services;

public class TaxMasterService : ITaxMasterService
{
  private readonly IUnitOfWork _unitOfWork;

  public TaxMasterService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IEnumerable<TaxMasterDto>> GetTaxMasters()
  {
    IEnumerable<TaxMasterDto> taxMasters = await _unitOfWork.TaxMasters.GetTaxMastersAsync();
    _unitOfWork.Commit();
    return taxMasters;
  }

  public async Task<IEnumerable<TaxMasterDto>> GetStockTaxByStockCodeAndStore(string stockCode, string storeId)
  {
    IEnumerable<TaxMasterDto> taxMasters = await _unitOfWork.TaxMasters.GetStockTaxByStockCodeAndStoreAsync(stockCode, storeId);
    _unitOfWork.Commit();
    return taxMasters;
  }

  public async Task<IEnumerable<ProductTaxDto>> GetProductTaxes()
  {
    IEnumerable<ProductTaxDto> productTaxes = await _unitOfWork.TaxMasters.GetProductTaxesAsync();
    _unitOfWork.Commit();
    return productTaxes;
  }
  public async Task<IEnumerable<TaxMaster>> GetTaxes()
  {
    var items = await _unitOfWork.TaxMasters.GetTaxesAsync();
    _unitOfWork.Commit();
    return items;
  }

  public async Task<bool> AddTax(TaxMaster tax)
  {
    var isAdded = await _unitOfWork.TaxMasters.AddTaxAsync(tax);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> UpdateTax(TaxMaster tax)
  {
    var isUpdated = await _unitOfWork.TaxMasters.UpdateTaxAsync(tax);
    _unitOfWork.Commit();
    return isUpdated;
  }

  public async Task<bool> DeleteTax(string taxName)
  {
    var isDeleted = await _unitOfWork.TaxMasters.DeleteTaxAsync(taxName);
    isDeleted &= await DeleteTaxRate(taxName);
    _unitOfWork.Commit();
    return isDeleted;
  }

  public async Task<bool> TaxNameAlreadyExists(string taxName, string? taxCode = null)
  {
    var alreadyExists = await _unitOfWork.TaxMasters.TaxNameAlreadyExistsAsync(taxName, taxCode);
    _unitOfWork.Commit();
    return alreadyExists;
  }

  public async Task<int> GetTaxUsageCount(string taxName, string? taxCode)
  {
    var usageCount = await _unitOfWork.TaxMasters.GetTaxUsageCountAsync(taxName, taxCode);
    _unitOfWork.Commit();
    return usageCount;
  }

  public async Task<bool> DeleteTaxRate(string taxName, string? taxCode = null)
  {
    var isDeleted = await _unitOfWork.TaxMasters.DeleteTaxRateAsync(taxName, taxCode);
    isDeleted &= await _unitOfWork.TaxMasters.DeleteStockTaxAsync(taxName, taxCode);
    isDeleted &= await _unitOfWork.TaxMasters.DeleteAssocTaxAsync(taxName, taxCode);
    _unitOfWork.Commit();
    return isDeleted;
  }

  public async Task<IEnumerable<TaxMaster>> GetTaxesOrderByTaxNameAsync()
  {
    var taxesList = await _unitOfWork.TaxMasters.GetTaxesOrderByTaxNameAsync();
    _unitOfWork.Commit();
    return taxesList;
  }
}
