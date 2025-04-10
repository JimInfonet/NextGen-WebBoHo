namespace InfonetCSCR.Web.Abstractions.Services;

public interface IHotButtonsService
{
  Task<List<HotButtonsViewModel>?> GetHotButtonsByButtonIdRange(HotButtonsIdRangeRecordViewModel hotButtonsIdRangeRecordViewModel);
  Task<bool?> InsertHotButtons(HotButtonsViewModel hotButtonsViewModel);
  Task<bool?> UpdateHotButtonsColor(HotButtonsViewModel hotButtonsViewModel);
  Task<bool?> DeleteHotButton(HotButtonsViewModel hotButtonsViewModel);
  Task<PLUMastViewModel?> GetPLUMastByPLUCode(string pluCode);
  Task<HotButtonsViewModel?> GetBottleReturnDataByProductCode(string productCode);
}
