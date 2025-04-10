namespace InfonetCSCR.Web.Services;

public class CardRestrictionProfileService : ICardRestrictionProfileService
{
  private readonly ILogger<CardRestrictionProfileService> _logger;
  private readonly IHttpService _http;

  public CardRestrictionProfileService(IHttpService http, ILogger<CardRestrictionProfileService> logger)
  {
    _http = http;
    _logger = logger;
  }

  public async Task<IEnumerable<CardRestrictionProfileViewModel>?> GetAllRestrictionProfiles()
  {
    string url = $"CardRestrictionProfile/GetAllRestrictionProfiles";
    var response = await _http.GetAsync(url);
    IEnumerable<CardRestrictionProfileViewModel>? responseData = null;

    if (response.IsSuccessStatusCode)
    {
      responseData = await response.Content.ReadFromJsonAsync<IEnumerable<CardRestrictionProfileViewModel>>();
    }
    else
    {
      _logger.LogError(Convert.ToString(response.StatusCode));
    }
    return responseData;
  }

  public async Task<bool> SaveTimeOfPurchaseRestriction(CardProfileTimeLimitViewModel cardProfileTimeLimit, bool isUpdate)
  {
    string url = isUpdate ? $"CardRestrictionProfile/UpdateTimeOfPurchaseRestriction" : $"CardRestrictionProfile/AddTimeOfPurchaseRestriction";
    var response = await _http.PostAndParseAsync<bool>(url, cardProfileTimeLimit);
    return response;
  }

  public async Task<IEnumerable<CardProfileTimeLimitViewModel>?> GetTimeOfPurchaseRestriction(string? profileId)
  {
    string url = $"CardRestrictionProfile/GetTimeOfPurchaseRestriction?profileid={HttpUtility.UrlEncode(profileId)}";
    var response = await _http.GetAndParseAsync<IEnumerable<CardProfileTimeLimitViewModel>>(url);
    return response;
  }

  public async Task<IEnumerable<ProfilePromptViewModel>?> GetAllProfilePrompts(string? profileId)
  {
    string url = $"CardRestrictionProfile/GetAllProfilePrompts?profileid={HttpUtility.UrlEncode(profileId)}";
    var response = await _http.GetAndParseAsync<IEnumerable<ProfilePromptViewModel>>(url);
    return response;
  }

  public async Task<CardRestrictionProfileViewModel?> GetCardRestrictionProfile(string? profileId)
  {
    string url = $"CardRestrictionProfile/GetCardRestrictionProfile?profileid={HttpUtility.UrlEncode(profileId)}";
    var response = await _http.GetAndParseAsync<CardRestrictionProfileViewModel>(url);
    return response;
  }

  public async Task<bool> SaveCardRestrictionProfile(CardRestrictionProfileViewModel cardRestrictionProfile, bool isUpdate)
  {
    string url = (isUpdate) ? "CardRestrictionProfile/UpdateCardRestrictionProfile" : "CardRestrictionProfile/AddCardRestrictionProfile";
    var response = await _http.PostAndParseAsync<bool>(url, cardRestrictionProfile);
    return response;
  }

  public async Task<ProfilePromptViewModel?> GetProfilePromptByPromptSequence(ProfilePromptViewModel profilePrompt)
  {
    string url = $"CardRestrictionProfile/GetProfilePromptByPromptSequence";
    var response = await _http.PostAndParseAsync<ProfilePromptViewModel?>(url, profilePrompt);
    return response;
  }

  public async Task<bool> AddProfilePrompt(ProfilePromptViewModel profilePrompt)
  {
    string url = $"CardRestrictionProfile/AddProfilePrompt";
    var response = await _http.PostAndParseAsync<bool>(url, profilePrompt);
    return response;
  }

  public async Task<bool> DeleteProfilePrompt(ProfilePromptViewModel profilePrompt)
  {
    string url = $"CardRestrictionProfile/DeleteProfilePrompt";
    var response = await _http.PostAndParseAsync<bool>(url, profilePrompt);
    return response;
  }

  public async Task<bool> DeleteCardRestrictionProfile(string profileId)
  {
    string url = $"CardRestrictionProfile/DeleteCardRestrictionProfile?profileid={HttpUtility.UrlEncode(profileId)}";
    var response = await _http.GetAndParseAsync<bool>(url);
    return response;
  }

  public async Task<bool> DeleteTimeOfPurchaseRestriction(string profileId)
  {
    string url = $"CardRestrictionProfile/DeleteTimeOfPurchaseRestriction?profileid={HttpUtility.UrlEncode(profileId)}";
    var response = await _http.GetAndParseAsync<bool>(url);
    return response;
  }

  public async Task<IEnumerable<ProfilePromptViewModel>?> GetPromptMessageByProfileID(string profileId)
  {
    string url = $"CardRestrictionProfile/GetPromptMessageByProfileID?profileid={HttpUtility.UrlEncode(profileId)}";
    var response = await _http.GetAndParseAsync<IEnumerable<ProfilePromptViewModel>>(url);
    return response;
  }
}
