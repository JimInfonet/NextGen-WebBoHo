namespace InfonetCSCR.Web.Services;

public class FunctionButtonsService : IFunctionButtonsService
{
  private readonly IHttpService _http;

  public FunctionButtonsService(IHttpService http)
  {
    _http = http;
  }

  public async Task<IList<FunctionButtonViewModel>?> GetAllFunctionButtonsData()
  {
    string url = "FunctionButtons/GetAllFunctionButtonsData";
    IList<FunctionButtonViewModel>? functionButtons = await _http.GetAndParseAsync<IList<FunctionButtonViewModel>>(url);
    return functionButtons;
  }

  public async Task<bool> SwapFunctionButtonData(int sourceButtonId, int targetButtonId)
  {
    string url = @$"FunctionButtons/SwapFunctionButtonData/{sourceButtonId}/{targetButtonId}";
    bool isUpdated = await _http.PostAndParseAsync<bool>(url, null);
    return isUpdated;
  }

  public async Task<bool> InsertFunctionButton(FunctionButtonViewModel functionButton)
  {
    string url = @$"FunctionButtons/InsertFunctionButton";
    bool isAdded = await _http.PostAndParseAsync<bool>(url, functionButton);
    return isAdded;
  }

  public async Task<bool> UpdateFunctionButton(FunctionButtonViewModel functionButton)
  {
    string url = @$"FunctionButtons/UpdateFunctionButton";
    bool isUpdated = await _http.PostAndParseAsync<bool>(url, functionButton);
    return isUpdated;
  }

  public async Task<bool> DeleteFunctionButton(int buttonId)
  {
    string url = @$"FunctionButtons/DeleteFunctionButton/{buttonId}";
    bool isDeleted = await _http.PostAndParseAsync<bool>(url, null);
    return isDeleted;
  }
}
