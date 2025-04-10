namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IHotButtonPagesRepository
{
  Task<IList<HotButtonPages>> GetAllHotButtonPagesAsync();
  Task<HotButtonPages?> GetHotButtonPageByIdAsync(int pageId);
  Task<bool> InsertHotButtonPageAsync(HotButtonPages hotButtonPages);
  Task<bool> UpdateHotButtonPageAsync(HotButtonPages hotButtonPages);
}
