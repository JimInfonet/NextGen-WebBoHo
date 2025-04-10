namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class CustomersGridController : Controller
{
  private readonly IHttpService _http;
  private readonly ICustomerService _customersService;

  public CustomersGridController(IHttpService http, ICustomerService customersService)
  {
    _http = http;
    _customersService = customersService;
  }

  public IActionResult Index()
  {
    var policies = _customersService.GetPolicies();
    ViewData["CUST_MEM"] = policies.CustMem;
    return View();
  }

  public async Task<IActionResult> GetAllCustomers(bool orderByCode = true)
  {
    string url = $"Customers/GetAllCustomers?orderByCode={orderByCode}";
    var customers = await _http.GetAndParseAsync<IEnumerable<CustomersGridViewModel>>(url);
    return Ok(customers);
  }

  public async Task<IActionResult> DeleteCustomer([FromBody] string code)
  {
    await _customersService.DeleteClient(code);
    return NoContent();
  }
}
