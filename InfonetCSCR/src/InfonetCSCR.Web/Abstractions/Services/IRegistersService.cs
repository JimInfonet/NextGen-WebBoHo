namespace InfonetCSCR.Web.Abstractions.Services;

public interface IRegistersService
{
  Task<IEnumerable<int>?> GetRegisters();
  Task<IEnumerable<RegisterViewModel>?> GetRegistersByRegNo(int regNo);
  Task<IEnumerable<CustomerDisplayViewModel>?> GetCustomerDisplay();
  Task<IEnumerable<DeviceViewModel>?> GetDevices();
  Task<bool> SaveRegister(List<RegisterViewModel> register, bool isUpdate);
  Task<bool> IsRegisterExists(int regNo);
  Task<bool> DeleteRegister(int regNo);
}
