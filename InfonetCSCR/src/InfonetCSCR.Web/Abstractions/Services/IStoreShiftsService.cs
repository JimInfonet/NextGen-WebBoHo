namespace InfonetCSCR.Web.Abstractions.Services;

public interface IStoreShiftsService
{
  string GetPolicies();
  Task<IList<StoreShiftViewModel>?> GetStoreShifts();
  Task<bool> AddStoreShift(StoreShiftViewModel storeShift);
  Task<bool> DeleteStoreShift(int shiftNumber);
  Task<bool> UpdateStoreShifts(IList<StoreShiftViewModel> storeShifts, bool isUpdateAll);
}
