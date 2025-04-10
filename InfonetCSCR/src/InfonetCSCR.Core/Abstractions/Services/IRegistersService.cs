namespace InfonetCSCR.Core.Abstractions.Services;

public interface IRegistersService
{
  Task<IEnumerable<CustomerDisplay>> GetCustomerDisplay();
  Task<IEnumerable<Register>> GetRegistersByRegNo(int regNo);
  Task<IEnumerable<int>> GetRegisters();
  Task<IEnumerable<Device>> GetDevices();
  Task<bool> AddRegister(List<Register> registerList);
  Task<bool> UpdateRegister(List<Register> registerList);
  Task<bool> DeleteRegister(int regNo);
  Task<bool> IsRegisterExists(int regNo);
}
