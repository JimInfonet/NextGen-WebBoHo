namespace InfonetCSCR.Web.Services;

public class FunctionListService : IFunctionListService
{
  private readonly IHttpService _http;

  public FunctionListService(IHttpService http)
  {
    _http = http;
  }

  public async Task<IList<FunctionListViewModel>?> GetAllFunctionListData()
  {
    string url = "FunctionList/GetAllFunctionListData";
    IList<FunctionListViewModel>? functionLists = await _http.GetAndParseAsync<IList<FunctionListViewModel>>(url);
    return functionLists;
  }

  public async Task<IList<FunctionListViewModel>?> GetFunctionListByButtonType(int buttonType)
  {
    string url = $@"FunctionList/GetFunctionListByButtonType/{buttonType}";
    IList<FunctionListViewModel>? functionLists = await _http.GetAndParseAsync<IList<FunctionListViewModel>>(url);
    return functionLists;
  }
}
