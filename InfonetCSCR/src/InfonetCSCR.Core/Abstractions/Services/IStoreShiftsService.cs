namespace InfonetCSCR.Core.Abstractions.Services;

public interface IStoreShiftsService
{
  Task<IList<StoreShift>> GetStoreShifts();
  Task<bool> AddStoreShift(StoreShiftDto storeShift);
  Task<bool> DeleteStoreShift(int shiftNumber);
  Task<bool> UpdateStoreShifts(IList<StoreShiftDto> storeShifts, bool isUpdateAll);
}
