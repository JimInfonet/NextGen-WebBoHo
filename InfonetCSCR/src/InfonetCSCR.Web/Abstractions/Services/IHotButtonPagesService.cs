namespace InfonetCSCR.Web.Abstractions.Services;

public interface IHotButtonPagesService
{
  Task<IList<HotButtonPagesViewModel>?> GetAllHotButtonPages();
  Task<bool?> UpdateHotButtonPage(HotButtonPagesViewModel? hotButtonPagesViewModel);
}
