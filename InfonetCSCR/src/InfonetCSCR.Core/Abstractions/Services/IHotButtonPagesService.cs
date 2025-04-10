namespace InfonetCSCR.Core.Abstractions.Services;

public interface IHotButtonPagesService
{
  Task<IList<HotButtonPages>> GetAllHotButtonPages();
  Task<bool> UpdateHotButtonPage(HotButtonPages hotButtonPages);
}
