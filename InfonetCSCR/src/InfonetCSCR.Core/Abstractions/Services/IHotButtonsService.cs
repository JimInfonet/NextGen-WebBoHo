namespace InfonetCSCR.Core.Abstractions.Services;

public interface IHotButtonsService
{
  Task<IEnumerable<HotButtonsWithProductName>> GetHotButtonsWithProductName();
  Task<IList<HotButtons>> GetHotButtonsByButtonIdRange(HotButtonsIdRangeRecord hotButtonsIdRangeRecord);
  Task<bool> InsertHotButtons(HotButtons hotButtons);
  Task<bool> UpdateHotButtonsColor(HotButtons hotButtons);
  Task<bool> DeleteHotButton(HotButtons hotButtons);
  Task<HotButtons> GetBottleReturnDataByProductCode(string productCode);

}
