namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class RegistersController : Controller
{
  private readonly IRegistersService _registersService;

  public RegistersController(IRegistersService registersService)
  {
    _registersService = registersService;
  }

  public IActionResult Index()
  {
    return View();
  }

  public async Task<IActionResult> GetRegisters()
  {
    var registers = await _registersService.GetRegisters();
    return Ok(registers);
  }

  public async Task<IActionResult> GetRegistersByRegNo([FromBody] int regNo)
  {
    var registers = await _registersService.GetRegistersByRegNo(regNo);
    return Ok(registers);
  }

  public async Task<IActionResult> GetCustomerDisplay()
  {
    var customerDisplay = await _registersService.GetCustomerDisplay();
    return Ok(customerDisplay);
  }

  public async Task<IActionResult> GetDevices()
  {
    var devices = await _registersService.GetDevices();
    return Ok(devices);
  }

  public async Task<IActionResult> SaveRegister([FromBody] List<RegisterViewModel> register, bool isUpdate)
  {
    var isAddedOrUpdated = await _registersService.SaveRegister(register, isUpdate);
    return Ok(isAddedOrUpdated);
  }

  public async Task<IActionResult> IsRegisterExists([FromBody] int regNo)
  {
    var alreadyExists = await _registersService.IsRegisterExists(regNo);
    return Ok(alreadyExists);
  }

  public async Task<IActionResult> DeleteRegister([FromBody] int regNo)
  {
    var isDeleted = await _registersService.DeleteRegister(regNo);
    return Ok(isDeleted);
  }
}
