namespace InfonetCSCR.Services;

public class CharacteristicService : ICharacteristicService
{
  private readonly IUnitOfWork _unitOfWork;

  public CharacteristicService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IList<Characteristic>> GetAllCharacteristics()
  {
    IList<Characteristic> characteristics = await _unitOfWork.Characteristic.GetAllCharacteristicsAsync();
    _unitOfWork.Commit();
    return characteristics;
  }

  public async Task<bool> AddCharacteristic(Characteristic characteristic)
  {
    bool isAdded = await _unitOfWork.Characteristic.AddCharacteristicAsync(characteristic);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> UpdateCharacteristic(Characteristic characteristic)
  {
    var isUpdated = await _unitOfWork.Characteristic.UpdateCharacteristicAsync(characteristic);
    _unitOfWork.Commit();
    return isUpdated;
  }
  
  public async Task<bool> DeleteCharacteristic(string characteristicId)
  {
    var isDeleted= await _unitOfWork.Characteristic.DeleteCharacteristicAsync(characteristicId);
    _unitOfWork.Commit();
    return isDeleted;
  }
}
