namespace InfonetCSCR.Services;

public class DiscountTableService : IDiscountTableService
{
  private readonly IUnitOfWork _unitOfWork;

  public DiscountTableService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IList<DiscountTable>> GetAllDiscountTableData(DiscountTablePolicyRecord discountTablePolicyRecord)
  {
    IList<DiscountTable> discountTableData = await _unitOfWork.DiscountTables.GetAllDiscountTableDataAsync(discountTablePolicyRecord);
    _unitOfWork.Commit();

    return discountTableData;
  }

  public async Task<bool> InsertDataOnDiscountTable(DiscountTable discountTable)
  {
    bool isAdded = await _unitOfWork.DiscountTables.InsertDataOnDiscountTableAsync(discountTable);
    _unitOfWork.Commit();

    return isAdded;
  }

  public async Task<bool> UpdateDiscountPercentage(DiscountTable discountTable)
  {
    bool isUpdated = await _unitOfWork.DiscountTables.UpdateDiscountPercentageAsync(discountTable);
    _unitOfWork.Commit();

    return isUpdated;
  }

  public async Task<bool> DeleteDiscount(DiscountTable discountTable)
  {
    bool isDeleted = await _unitOfWork.DiscountTables.DeleteDiscountAsync(discountTable);
    _unitOfWork.Commit();

    return isDeleted;
  }
}
