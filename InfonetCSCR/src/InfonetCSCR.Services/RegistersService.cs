namespace InfonetCSCR.Services;

public class RegistersService : IRegistersService
{
  private readonly IUnitOfWork _unitOfWork;

  public RegistersService(IApplicationConfig config)
  {
    _unitOfWork = new UnitOfWork(config, config.CSCMasterConnectionString);
  }

  public async Task<IEnumerable<int>> GetRegisters()
  {
    var registers = await _unitOfWork.Registers.GetRegistersAsync();
    _unitOfWork.Commit();
    return registers;
  }

  public async Task<IEnumerable<Register>> GetRegistersByRegNo(int regNo)
  {
    var registers = await _unitOfWork.Registers.GetRegistersByRegNoAsync(regNo);
    _unitOfWork.Commit();
    return registers;
  }

  public async Task<IEnumerable<CustomerDisplay>> GetCustomerDisplay()
  {
    var customerDisplays = await _unitOfWork.Registers.GetCustomerDisplayAsync();
    _unitOfWork.Commit();
    return customerDisplays;
  }

  public async Task<IEnumerable<Device>> GetDevices()
  {
    var devices = await _unitOfWork.Registers.GetDevicesAsync();
    _unitOfWork.Commit();
    return devices;
  }

  public async Task<bool> AddRegister(List<Register> registerList)
  {
    var isAdded = await _unitOfWork.Registers.AddRegisterAsync(registerList);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> UpdateRegister(List<Register> registerList)
  {
    var isUpdated = await _unitOfWork.Registers.UpdateRegisterAsync(registerList);
    _unitOfWork.Commit();
    return isUpdated;
  }

  public async Task<bool> DeleteRegister(int regNo)
  {
    var isDeleted = await _unitOfWork.Registers.DeleteRegisterAsync(regNo);
    _unitOfWork.Commit();
    return isDeleted;
  }

  public async Task<bool> IsRegisterExists(int regNo)
  {
    var alreadyExists = await _unitOfWork.Registers.IsRegisterExistsAsync(regNo);
    _unitOfWork.Commit();
    return alreadyExists;
  }
}
