namespace InfonetCSCR.Web.Services;

public class RegistersService : IRegistersService
{
  private readonly IHttpService _http;

  public RegistersService(IHttpService http)
  {
    _http = http;
  }

  public async Task<IEnumerable<int>?> GetRegisters()
  {
    string url = $"Registers/GetRegisters";
    var registers = await _http.GetAndParseAsync<IEnumerable<int>?>(url);
    return registers;
  }

  public async Task<IEnumerable<CustomerDisplayViewModel>?> GetCustomerDisplay()
  {
    string url = $"Registers/GetCustomerDisplay";
    var customerDisplay = await _http.GetAndParseAsync<IEnumerable<CustomerDisplayViewModel>?>(url);
    return customerDisplay;
  }

  public async Task<IEnumerable<DeviceViewModel>?> GetDevices()
  {
    string url = $"Registers/GetDevices";
    var devices = await _http.GetAndParseAsync<IEnumerable<DeviceViewModel>?>(url);
    return devices;
  }

  public async Task<IEnumerable<RegisterViewModel>?> GetRegistersByRegNo(int regNo)
  {
    string url = $"Registers/GetRegistersByRegNo";
    var registers = await _http.PostAndParseAsync<IEnumerable<RegisterViewModel>?>(url, regNo);
    return registers;
  }

  public async Task<bool> SaveRegister(List<RegisterViewModel> register, bool isUpdate)
  {
    string url = isUpdate ? "Registers/UpdateRegister" : "Registers/AddRegister";
    var isSaved = await _http.PostAndParseAsync<bool>(url, register);
    return isSaved;
  }

  public async Task<bool> IsRegisterExists(int regNo)
  {
    string url = "Registers/IsRegisterExists";
    var IsRegisterExists = await _http.PostAndParseAsync<bool>(url, regNo);
    return IsRegisterExists;
  }

  public async Task<bool> DeleteRegister(int regNo)
  {
    string url = "Registers/DeleteRegister";
    var isDeleted = await _http.PostAndParseAsync<bool>(url, regNo);
    return isDeleted;
  }
}
