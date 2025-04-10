namespace InfonetCSCR.Api.Controllers;

[Route("api/Customers")]
[ApiController]
[Authorize]

public class CustomersController : ControllerBase
{
  public readonly ICustomerService _customerService;
  public readonly IApplicationConfig _config;

  public CustomersController(ICustomerService customerService, IApplicationConfig config)
  {
    _customerService = customerService;
    _config = config;
  }

  // GET api/Customers/GetAllCustomers
  [HttpGet]
  [Route("GetAllCustomers")]
  public async Task<IActionResult> GetAllCustomers(bool orderByCode)
  {
    IEnumerable<CustomersDto> customers = await _customerService.GetAllCustomers(orderByCode);
    return Ok(customers);
  }

  // GET api/Customers/GetCustomer
  [HttpGet]
  [Route("GetCustomer")]
  public async Task<IActionResult> GetCustomer(string? code, string? loyaltyNum)
  {
    if (code == null && loyaltyNum == null)
      return Ok(null);

    CustomerFormDto customerForm = await _customerService.GetCustomerByCode(code, loyaltyNum);
    return Ok(customerForm);
  }

  // POST api/Customers/DeleteClientCard
  [HttpPost]
  [Route("DeleteClientCard")]
  public async Task<IActionResult> DeleteClientCard([FromBody] string code)
  {
    bool isDeleted = await _customerService.DeleteClientCard(code);
    return Ok(isDeleted);
  }

  // POST api/Customers/AddCustomer
  [HttpPost]
  [Route("AddCustomer")]
  public async Task<IActionResult> AddCustomer([FromBody] CustomerFormDto customerForm)
  {
    bool isCustomerAdded = await _customerService.AddCustomer(customerForm);
    return Ok(isCustomerAdded);
  }

  // POST api/Customers/UpdateCustomer
  [HttpPost]
  [Route("UpdateCustomer")]
  public async Task<IActionResult> UpdateCustomer([FromBody] CustomerFormDto customerForm)
  {
    bool isUpdated = await _customerService.UpdateCustomer(customerForm);
    return Ok(isUpdated);
  }

  // POST api/Customers/DeleteClient
  [HttpPost]
  [Route("DeleteClient")]
  public async Task<IActionResult> DeleteClient([FromBody] CustomerFormDto customerForm)
  {
    bool isDeleted = await _customerService.DeleteClient(customerForm);
    return Ok(isDeleted);
  }

  // GET api/Customers/GetAllCustomerGroups
  [HttpGet]
  [Route("GetAllCustomerGroups")]
  public async Task<IActionResult> GetAllCustomerGroups()
  {
    IEnumerable<CustomerGroup> groups = await _customerService.GetAllCustomerGroups();
    return Ok(groups);
  }

  // GET api/Customers/GetSecurityAR
  [HttpGet]
  [Route("GetSecurityAR")]
  public async Task<IActionResult> GetSecurityAR()
  {
    bool securityAR = await _customerService.GetSecurityAR();
    return Ok(securityAR);
  }
}
