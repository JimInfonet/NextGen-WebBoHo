namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ICharacteristicRepository
{
  Task<IList<Characteristic>> GetAllCharacteristicsAsync();
  Task<bool> AddCharacteristicAsync(Characteristic characteristic);
  Task<bool> UpdateCharacteristicAsync(Characteristic characteristic);
  Task<bool> DeleteCharacteristicAsync(string characteristicId);
}
