namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IHotButtonsRepository
{
  Task<IEnumerable<HotButtonsWithProductName>> GetHotButtonsWithProductNameAsync();
  Task<IList<HotButtons>> GetHotButtonsByButtonIdRangeAsync(HotButtonsIdRangeRecord hotButtonsIdRangeRecord);
  Task<bool> InsertHotButtonsAsync(HotButtons hotButtons);
  Task<bool> UpdateHotButtonsColorAsync(HotButtons hotButton);
  Task<bool> DeleteHotButtonAsync(HotButtons hotButton);
  Task<HotButtons> GetBottleReturnDataByProductCodeAsync(string productCode);
}
