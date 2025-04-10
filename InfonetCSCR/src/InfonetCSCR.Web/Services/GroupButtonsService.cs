namespace InfonetCSCR.Web.Services;

public class GroupButtonsService : IGroupButtonsService
{
  private readonly IHttpService _http;

  public GroupButtonsService(IHttpService http)
  {
    _http = http;
  }

  public async Task<IList<GroupButtonViewModel>?> GetAllGroupButtonsData()
  {
    string url = "GroupButtons/GetAllGroupButtonsData";
    IList<GroupButtonViewModel>? groupButtons = await _http.GetAndParseAsync<IList<GroupButtonViewModel>>(url);
    return groupButtons;
  }

  public async Task<bool> SwapGroupButtonData(int sourceButtonId, int targetButtonId)
  {
    string url = @$"GroupButtons/SwapGroupButtonData/{sourceButtonId}/{targetButtonId}";
    bool isUpdated = await _http.PostAndParseAsync<bool>(url, null);
    return isUpdated;
  }

  public async Task<bool> InsertGroupButton(GroupButtonViewModel groupButton)
  {
    string url = @$"GroupButtons/InsertGroupButton";
    bool isAdded = await _http.PostAndParseAsync<bool>(url, groupButton);
    return isAdded;
  }

  public async Task<bool> UpdateGroupButton(GroupButtonViewModel groupButton)
  {
    string url = @$"GroupButtons/UpdateGroupButton";
    bool isUpdated = await _http.PostAndParseAsync<bool>(url, groupButton);
    return isUpdated;
  }

  public async Task<bool> DeleteGroupButton(int buttonId)
  {
    string url = @$"GroupButtons/DeleteGroupButton/{buttonId}";
    bool isDeleted = await _http.PostAndParseAsync<bool>(url, null);
    return isDeleted;
  }
}
