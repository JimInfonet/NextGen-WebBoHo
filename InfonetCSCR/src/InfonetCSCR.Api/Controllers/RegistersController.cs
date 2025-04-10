namespace InfonetCSCR.Api.Controllers;

[Route("api/Registers")]
[ApiController]
[Authorize]

public class RegistersController : ControllerBase
{
  private readonly IRegistersService _registersService;

  public RegistersController(IRegistersService registersService)
  {
    _registersService = registersService;
  }

  // GET api/Registers/GetRegisters
  [HttpGet]
  [Route("GetRegisters")]
  public async Task<IActionResult> GetRegisters()
  {
    var registers = await _registersService.GetRegisters();
    return Ok(registers);
  }

  // GET api/Registers/GetCustomerDisplay
  [HttpGet]
  [Route("GetCustomerDisplay")]
  public async Task<IActionResult> GetCustomerDisplay()
  {
    var customerDisplay = await _registersService.GetCustomerDisplay();
    return Ok(customerDisplay);
  }

  // POST api/Registers/GetRegistersByRegNo
  [HttpPost]
  [Route("GetRegistersByRegNo")]
  public async Task<IActionResult> GetRegistersByRegNo([FromBody] int regNo)
  {
    var registers = await _registersService.GetRegistersByRegNo(regNo);
    return Ok(registers);
  }

  // GET api/Registers/GetDevices
  [HttpGet]
  [Route("GetDevices")]
  public async Task<IActionResult> GetDevices()
  {
    var devices = await _registersService.GetDevices();
    return Ok(devices);
  }

  // POST api/Registers/AddRegister
  [HttpPost]
  [Route("AddRegister")]
  public async Task<IActionResult> AddRegister(List<Register> registerList)
  {
    var isAdded = await _registersService.AddRegister(registerList);
    return Ok(isAdded);
  }

  // POST api/Registers/UpdateRegister
  [HttpPost]
  [Route("UpdateRegister")]
  public async Task<IActionResult> UpdateRegister([FromBody] List<Register> registerList)
  {
    var isUpdated = await _registersService.UpdateRegister(registerList);
    return Ok(isUpdated);
  }

  // POST api/Registers/DeleteRegister
  [HttpPost]
  [Route("DeleteRegister")]
  public async Task<IActionResult> DeleteRegister([FromBody] int regNo)
  {
    var isDeleted = await _registersService.DeleteRegister(regNo);
    return Ok(isDeleted);
  }

  // POST api/Registers/IsRegisterExists
  [HttpPost]
  [Route("IsRegisterExists")]
  public async Task<IActionResult> IsRegisterExists([FromBody] int regNo)
  {
    var alreadyExists = await _registersService.IsRegisterExists(regNo);
    return Ok(alreadyExists);
  }
}
