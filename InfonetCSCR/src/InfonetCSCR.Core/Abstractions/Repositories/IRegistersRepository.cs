namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IRegistersRepository
{
  Task<IEnumerable<int>> GetRegistersAsync();
  Task<IEnumerable<Register>> GetRegistersByRegNoAsync(int regNo);
  Task<IEnumerable<CustomerDisplay>> GetCustomerDisplayAsync();
  Task<IEnumerable<Device>> GetDevicesAsync();
  Task<bool> AddRegisterAsync(List<Register> register);
  Task<bool> UpdateRegisterAsync(List<Register> register);
  Task<bool> DeleteRegisterAsync(int regNo);
  Task<bool> IsRegisterExistsAsync(int regNo);
}
