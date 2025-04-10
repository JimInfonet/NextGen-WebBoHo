namespace InfonetCSCR.Web.Services;

public class HotButtonPagesService : IHotButtonPagesService
{
  private readonly IHttpService _http;

  public HotButtonPagesService(IHttpService http)
  {
    _http = http;
  }

  public async Task<IList<HotButtonPagesViewModel>?> GetAllHotButtonPages()
  {
    string url = "HotButtonPages/GetAllHotButtonPages";
    IList<HotButtonPagesViewModel>? hotButtonPagesViewModels = await _http.GetAndParseAsync<IList<HotButtonPagesViewModel>>(url);
    return hotButtonPagesViewModels;
  }

  public async Task<bool?> UpdateHotButtonPage(HotButtonPagesViewModel? hotButtonPagesViewModel)
  {
    string url = "HotButtonPages/UpdateHotButtonPage";
    bool isUpdated = await _http.PostAndParseAsync<bool>(url, hotButtonPagesViewModel);
    return isUpdated;
  }
}
