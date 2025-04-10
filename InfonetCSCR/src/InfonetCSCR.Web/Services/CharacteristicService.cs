namespace InfonetCSCR.Web.Services;

public class CharacteristicService : ICharacteristicService
{
  private readonly IHttpService _http;

  public CharacteristicService(IHttpService http)
  {
    _http = http;
  }

  public async Task<IList<CharacteristicViewModel>?> GetAllCharacteristics()
  {
    string url = "Characteristic/GetAllCharacteristics";
    IList<CharacteristicViewModel>? characteristics = await _http.GetAndParseAsync<IList<CharacteristicViewModel>>(url);
    return characteristics;
  }

  public async Task<bool> AddCharacteristic(CharacteristicViewModel characteristic)
  {
    string url = $"Characteristic/AddCharacteristic";
    var isAdded = await _http.PostAndParseAsync<bool>(url, characteristic);
    return isAdded;
  }

  public async Task<bool> UpdateCharacteristic(CharacteristicViewModel characteristic)
  {
    string url = $"Characteristic/UpdateCharacteristic";
    var isUpdated = await _http.PostAndParseAsync<bool>(url, characteristic);
    return isUpdated;
  }
  
  public async Task<bool> DeleteCharacteristic(string characteristicId)
  {
    string url = $"Characteristic/DeleteCharacteristic";
    var isDeleted = await _http.PostAndParseAsync<bool>(url, characteristicId);
    return isDeleted;
  }
}
