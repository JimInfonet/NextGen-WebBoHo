namespace InfonetCSCR.Web.Abstractions.Services;

public interface ICharacteristicService
{
  Task<IList<CharacteristicViewModel>?> GetAllCharacteristics();
  Task<bool> AddCharacteristic(CharacteristicViewModel characteristic);
  Task<bool> UpdateCharacteristic(CharacteristicViewModel characteristic);
  Task<bool> DeleteCharacteristic(string characteristicId);
}
