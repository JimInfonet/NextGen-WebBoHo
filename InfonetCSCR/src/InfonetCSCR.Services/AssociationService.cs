namespace InfonetCSCR.Services;

public class AssociationService : IAssociationService
{
  private readonly IUnitOfWork _unitOfWork;

  public AssociationService(IApplicationConfig _applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(_applicationConfig, _applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IEnumerable<Assoc>> GetAssociations()
  {
    IEnumerable<Assoc> associations = await _unitOfWork.Associations.GetAssociationsAsync();
    _unitOfWork.Commit();
    return associations;
  }

  public async Task<IEnumerable<Assoc>> GetStockAssociationsByStockCodeAndStore(string stockCode, string storeId)
  {
    IEnumerable<Assoc> associations = await _unitOfWork.Associations.GetStockAssociationsByStockCodeAndStoreAsync(stockCode, storeId);
    _unitOfWork.Commit();
    return associations;
  }

  public async Task<bool> AddAssociation(Assoc assoc)
  {
    bool isAdded = await _unitOfWork.Associations.AddAssociationAsync(assoc);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> UpdateAssociation(Assoc assoc)
  {
    bool isUpdated = await _unitOfWork.Associations.UpdateAssociationAsync(assoc);
    _unitOfWork.Commit();
    return isUpdated;
  }

  public async Task<bool> DeleteAssociation(string code)
  {
    bool isDeleted = await _unitOfWork.Associations.DeleteAssociationAsync(code);
    _unitOfWork.Commit();
    return isDeleted;
  }

  public async Task<bool> IsAssociationExists(string code)
  {
    bool alreadyExists = await _unitOfWork.Associations.IsAssociationExistsAsync(code);
    _unitOfWork.Commit();
    return alreadyExists;
  }

  public async Task<IEnumerable<Tax>?> GetTaxes(string code)
  {
    var taxes = await _unitOfWork.Associations.GetTaxesAsync(code);
    _unitOfWork.Commit();
    return taxes;
  }

  public async Task<IEnumerable<Tax>?> GetAvailableTaxes(string code)
  {
    var taxes = await _unitOfWork.Associations.GetAvailableTaxesAsync(code);
    _unitOfWork.Commit();
    return taxes;
  }

  public async Task<bool> AddTax(AssocTax assocTax)
  {
    var isAdded = await _unitOfWork.Associations.AddTaxAsync(assocTax);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> DeleteTax(AssocTax assocTax)
  {
    var isDeleted = await _unitOfWork.Associations.DeleteTaxAsync(assocTax);
    _unitOfWork.Commit();
    return isDeleted;
  }

  public async Task<bool> IsChargeInUse(string code)
  {
    bool isChargeInUse = await _unitOfWork.Associations.IsChargeInUseAsync(code);
    _unitOfWork.Commit();
    return isChargeInUse;
  }
}
