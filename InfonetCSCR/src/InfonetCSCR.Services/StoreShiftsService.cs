namespace InfonetCSCR.Services;

public class StoreShiftsService : IStoreShiftsService
{
  private readonly IUnitOfWork _unitOfWork;

  public StoreShiftsService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IList<StoreShift>> GetStoreShifts()
  {
    IList<StoreShift> storeShifts = await _unitOfWork.StoreShifts.GetStoreShiftsAsync();
    _unitOfWork.Commit();
    return storeShifts;
  }

  public async Task<bool> AddStoreShift(StoreShiftDto storeShift)
  {
    bool isAdded = await _unitOfWork.StoreShifts.AddStoreShiftAsync(storeShift);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> DeleteStoreShift(int shiftNumber)
  {
    bool isDeleted = await _unitOfWork.StoreShifts.DeleteStoreShiftAsync(shiftNumber);
    _unitOfWork.Commit();
    return isDeleted;
  }

  public async Task<bool> UpdateStoreShifts(IList<StoreShiftDto> storeShifts, bool isUpdateAll)
  {
    bool isUpdated = await _unitOfWork.StoreShifts.UpdateStoreShiftsAsync(storeShifts, isUpdateAll);
    _unitOfWork.Commit();
    return isUpdated;
  }
}
