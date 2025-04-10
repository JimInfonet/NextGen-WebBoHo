namespace InfonetCSCR.Services;

public class TaxExemptCategoryService : ITaxExemptCategoryService
{
  private readonly IUnitOfWork _unitOfWork;

  public TaxExemptCategoryService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IEnumerable<TaxExemptCategory>> GetAllTaxExemptCategory()
  {
    IEnumerable<TaxExemptCategory> taxExemptCategories = await _unitOfWork.TaxExemptCategories.GetAllTaxExemptCategoryAsync();
    _unitOfWork.Commit();
    return taxExemptCategories;
  }

  public async Task<IEnumerable<Reason>> GetTaxExemptReasonsListByTypeAsync(string reasonType)
  {
    var taxExemptReasonsList = await _unitOfWork.TaxExemptCategories.GetTaxExemptReasonsListByTypeAsync(reasonType);
    _unitOfWork.Commit();
    return taxExemptReasonsList;
  }
}
