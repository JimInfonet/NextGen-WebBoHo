namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CompanySetupController : ControllerBase
{
  private readonly ICompanySetupService _companySetupService;

  public CompanySetupController(ICompanySetupService companySetupService)
  {
    _companySetupService = companySetupService;
  }

  // GET api/CompanySetup/GetCompany
  [HttpGet]
  [Route("GetCompany")]
  public async Task<IActionResult> GetCompany()
  {
    Company? company = await _companySetupService.GetCompany();
    return Ok(company);
  }

  // GET api/CompanySetup/GetCompanyName
  [HttpGet]
  [Route("GetCompanyName")]
  public async Task<IActionResult> GetCompanyName()
  {
    string? companyName = await _companySetupService.GetCompanyName();
    return Ok(companyName);
  }

  // GET api/CompanySetup/GetCompanyStoreCode
  [HttpGet]
  [Route("GetCompanyStoreCode")]
  public async Task<IActionResult> GetCompanyStoreCode()
  {
    string storeCode = await _companySetupService.GetCompanyStoreCode();
    return Ok(storeCode);
  }
}
