namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class PricebookManagementController : Controller
{
  private readonly IPricebookManagementService _pricebookManagementService;

  public PricebookManagementController(IPricebookManagementService pricebookManagementService)
  {
    _pricebookManagementService = pricebookManagementService;
  }

  public IActionResult Index()
  {
    return View();
  }

  [HttpPost]
  public async Task<IActionResult> GetSubDepartmentsByDepartments([FromBody] List<string> departments)
  {
    IEnumerable<SubDepartmentViewModel>? subDepartments = await _pricebookManagementService.GetSubDepartmentsByDepartments(departments);
    return Ok(subDepartments);
  }

  [HttpPost]
  public async Task<IActionResult> GetSubDetailsByDepartments([FromBody] List<string> departments)
  {
    IEnumerable<SubDetailViewModel>? subDetails = await _pricebookManagementService.GetSubDetailsByDepartments(departments);
    return Ok(subDetails);
  }

  [HttpGet]
  public async Task<IActionResult> GetVendorsOrderByColumnNumber(int columnNumber)
  {
    var vendors = await _pricebookManagementService.GetVendorsOrderByColumnNumber(columnNumber);
    return Ok(vendors);
  }
}
