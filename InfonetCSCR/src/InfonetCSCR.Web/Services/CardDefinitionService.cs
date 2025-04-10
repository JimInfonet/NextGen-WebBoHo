namespace InfonetCSCR.Web.Services;

public class CardDefinitionService : ICardDefinitionService
{
  private readonly IHttpService _http;
  private readonly ILogger<CardDefinitionService> _logger;

  public CardDefinitionService(IHttpService http, ILogger<CardDefinitionService> logger)
  {
    _http = http;
    _logger = logger;
  }

  public async Task<List<string>> GetOptionalProfileIDs()
  {
    string url = "CardDefinition/GetOptionalProfileIDs";
    var optionalProfileIDs = await _http.GetAndParseAsync<List<string>>(url);
    return optionalProfileIDs;
  }

  public async Task<CardDefinitionViewModel?> GetCardDefinitionByCardCode(string cardCode)
  {
    string url = $"CardDefinition/GetCardDefinition/{cardCode}";
    var cardDefinition = await _http.GetAndParseAsync<CardDefinitionViewModel>(url);
    return cardDefinition;
  }

  public async Task<bool?> AddCardForUpdateTender(CardViewModel cardViewModel)
  {
    string url = "CardDefinition/AddCardForUpdateTender";
    bool added = await _http.PostAndParseAsync<bool>(url, cardViewModel);
    return added;
  }

  public async Task<string> DeleteCardDefinition(string cardCode)
  {
    string url = "CardDefinition/DeleteCardDefinition";

    var response = await _http.PostAsync(url, cardCode);
    string result = "";

    if (response.IsSuccessStatusCode)
    {
      result = await response.Content.ReadAsStringAsync();
    }
    else
    {
      _logger.LogError("Internal server Error"); // TODO: change this to add modelstate error
    }

    return result;
  }
}
