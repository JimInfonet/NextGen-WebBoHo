namespace InfonetCSCR.Web.Services;

public class HotButtonsService : IHotButtonsService
{
  private readonly IHttpService _http;

  public HotButtonsService(IHttpService http)
  {
    _http = http;
  }

  public async Task<List<HotButtonsViewModel>?> GetHotButtonsByButtonIdRange(HotButtonsIdRangeRecordViewModel hotButtonsIdRangeRecordViewModel)
  {
    string url = "HotButtons/GetHotButtonsByButtonIdRange";
    List<HotButtonsViewModel>? hotButtons = await _http.PostAndParseAsync<List<HotButtonsViewModel>>(url, hotButtonsIdRangeRecordViewModel);
    return hotButtons;
  }

  public async Task<bool?> InsertHotButtons(HotButtonsViewModel hotButtonsViewModel)
  {
    string url = "HotButtons/InsertHotButtons";
    bool isAdded = await _http.PostAndParseAsync<bool>(url, hotButtonsViewModel);
    return isAdded;
  }

  public async Task<bool?> UpdateHotButtonsColor(HotButtonsViewModel hotButtonsViewModel)
  {
    string url = "HotButtons/UpdateHotButtonsColor";
    bool isColorUpdated = await _http.PostAndParseAsync<bool>(url, hotButtonsViewModel);
    return isColorUpdated;
  }

  public async Task<bool?> DeleteHotButton(HotButtonsViewModel hotButtonsViewModel)
  {
    string url = "HotButtons/DeleteHotButton";
    bool isDeleted = await _http.PostAndParseAsync<bool>(url, hotButtonsViewModel);
    return isDeleted;
  }

  public async Task<PLUMastViewModel?> GetPLUMastByPLUCode(string pluCode)
  {
    string url = @$"Product/GetPLUMastByPLUCode/{pluCode}";
    PLUMastViewModel? pluMastViewModel = await _http.GetAndParseAsync<PLUMastViewModel>(url);
    return pluMastViewModel;
  }

  public async Task<HotButtonsViewModel?> GetBottleReturnDataByProductCode(string productCode)
  {
    string url = @$"HotButtons/GetBottleReturnDataByProductCode/{productCode}";
    HotButtonsViewModel? hotButtonsViewModel = await _http.GetAndParseAsync<HotButtonsViewModel>(url);
    return hotButtonsViewModel;
  }
}
