namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class CompanySetupController : Controller
{
    private readonly ICompanySetupService _companySetupService;

    public CompanySetupController(ICompanySetupService companySetupService)
    {
        _companySetupService = companySetupService;
    }

    public async Task<IActionResult> GetCompany()
    {
        return Json(await _companySetupService.GetCompanyAsync());
    }
    public async Task<IActionResult> GetCompanyName()
    {
        return Json(await _companySetupService.GetCompanyNameAsync());
    }
}
