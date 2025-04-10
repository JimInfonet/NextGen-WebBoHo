namespace InfonetCSCR.Core.Abstractions.Services;

public interface ICharacteristicService
{
  Task<IList<Characteristic>> GetAllCharacteristics();
  Task<bool> AddCharacteristic(Characteristic characteristic);
  Task<bool> UpdateCharacteristic(Characteristic characteristic);
  Task<bool> DeleteCharacteristic(string characteristicId);
}
