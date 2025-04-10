namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IStoreShiftsRepository
{
  Task<IList<StoreShift>> GetStoreShiftsAsync();
  Task<bool> AddStoreShiftAsync(StoreShiftDto storeShift);
  Task<bool> DeleteStoreShiftAsync(int shiftNumber);
  Task<bool> UpdateStoreShiftsAsync(IList<StoreShiftDto> storeShifts, bool isUpdateAll);
}
