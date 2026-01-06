namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CompanySetupController : ControllerBase
{
    private readonly ICompanySetupService _companySetupService;
    private readonly ILogger<RepTillCloseController> _logger;
    public CompanySetupController(ICompanySetupService companySetupService, ILogger<RepTillCloseController> logger)
    {
        _companySetupService = companySetupService;
        _logger = logger;
    }

    // GET api/CompanySetup/GetCompany
    [HttpGet]
    [Route("GetCompany")]
    public async Task<IActionResult> GetCompany()
    {
        try
        {
            var company = await _companySetupService.GetCompany();
            return Ok(company);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting company info.");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
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
