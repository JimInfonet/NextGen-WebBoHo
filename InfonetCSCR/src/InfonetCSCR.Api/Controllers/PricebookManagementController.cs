namespace InfonetCSCR.Api.Controllers;

[Route("api/PricebookManagement")]
[ApiController]
[Authorize]
public class PricebookManagementController : ControllerBase
{
  private readonly IPricebookManagementService _pricebookManagementService;

  public PricebookManagementController(IPricebookManagementService pricebookManagementService)
  {
    _pricebookManagementService = pricebookManagementService;
  }

  // GET api/PricebookManagement/GetLabelPrintingReportData
  [HttpGet]
  [Route("GetLabelPrintingReportData")]
  public async Task<IActionResult> GetLabelPrintingReportData()
  {
    IEnumerable<LabelPrintDto> subDetail = await _pricebookManagementService.GetLabelPrintingReportData();
    return Ok(subDetail);
  }

  // GET api/PricebookManagement/GetLabelPrintingStandardReportData
  [HttpGet]
  [Route("GetLabelPrintingStandardReportData")]
  public async Task<IActionResult> GetLabelPrintingStandardReportData()
  {
    IEnumerable<LabelPrintDto> subDetail = await _pricebookManagementService.GetLabelPrintingStandardReportData();
    return Ok(subDetail);
  }
}
