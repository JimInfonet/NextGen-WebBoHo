namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class ElectronicJournalController : ControllerBase
{
  private readonly IElectronicJournalService _electronicJournalService;

  public ElectronicJournalController(IElectronicJournalService electronicJournalService)
  {
    _electronicJournalService = electronicJournalService;
  }

  // GET api/ElectronicJournal/GetElectronicJournalReportData
  [HttpGet]
  [Route("GetElectronicJournalReportData")]
  public async Task<IActionResult> GetElectronicJournalReportData(string saleNo, bool isFromTills)
  {
    IEnumerable<ElectronicJournalReportData> electronicJournalReportsData = await _electronicJournalService.GetElectronicJournalReportData(saleNo, isFromTills);
    return Ok(electronicJournalReportsData);
  }

  [HttpGet]
  [Route("GetEJSubreportData")]
  public async Task<IActionResult> GetEJSubreportData(string saleNo, bool isFromTills)
  {
    IEnumerable<ElectronicJournalSubreport?> ejSubreportData = await _electronicJournalService.GetEJSubreportData(saleNo, isFromTills);
    return Ok(ejSubreportData);
  }

  // GET api/ElectronicJournal/GetElectronicJournalOptions
  [HttpGet]
  [Route("GetElectronicJournalOptions")]
  public async Task<IActionResult> GetElectronicJournalOptions()
  {
    IEnumerable<string> electronicJournalReportsData = await _electronicJournalService.GetElectronicJournalOptions();
    return Ok(electronicJournalReportsData);
  }

  // GET api/ElectronicJournal/GetElectronicJournalFilters
  [HttpGet]
  [Route("GetElectronicJournalFilters")]
  public async Task<IActionResult> GetElectronicJournalFilters(string optionName, bool isWareHouse)
  {
    IEnumerable<EJSearchDescriptionDto> filters = await _electronicJournalService.GetElectronicJournalFilters(optionName, isWareHouse);
    return Ok(filters);
  }

  // GET api/ElectronicJournal/GetSearchDropdownValues
  [HttpPost]
  [Route("GetSearchDropdownValues")]
  public async Task<IActionResult> GetSearchDropdownValues([FromBody] EJSearchDescriptionDto desc)
  {
    IEnumerable<string> values = await _electronicJournalService.GetSearchDropdownValues(desc);
    return Ok(values);
  }

  // GET api/ElectronicJournal/GetStoreDropdownValues
  [HttpGet]
  [Route("GetStoreDropdownValues")]
  public async Task<IActionResult> GetStoreDropdownValues()
  {
    IEnumerable<StoreOptionDto> values = await _electronicJournalService.GetStoreDropdownValues();
    return Ok(values);
  }

  // POST api/ElectronicJournal/GetElectronicJournalGridData
  [HttpPost]
  [Route("GetElectronicJournalGridData")]
  public async Task<IActionResult> GetElectronicJournalGridData([FromBody] EJSearchParamDto ejSearchParam)
  {
    EJGridDataFilterDto ejGridDataFilterDto = await _electronicJournalService.GetElectronicJournalGridData(ejSearchParam);
    return Ok(ejGridDataFilterDto);
  }

  // GET api/ElectronicJournal/GetElectronicJournalSummaryReportData
  [HttpGet]
  [Route("GetElectronicJournalSummaryReportData")]
  public async Task<IActionResult> GetElectronicJournalSummaryReportData()
  {
    IEnumerable<ElectronicJournalSummaryReport> ejSummaryReportData = await _electronicJournalService.GetElectronicJournalSummaryReportData();
    return Ok(ejSummaryReportData);
  }

  // GET api/ElectronicJournal/GetEJSummarySubReportAData
  [HttpGet]
  [Route("GetEJSummarySubReportAData")]
  public async Task<IActionResult> GetEJSummarySubReportAData()
  {
    EJSummarySubreportAData ejSummarySubReportAData = await _electronicJournalService.GetEJSummarySubReportAData();
    return Ok(ejSummarySubReportAData);
  }

  // GET api/ElectronicJournal/GetEJSummarySubReportTData
  [HttpGet]
  [Route("GetEJSummarySubReportTData")]
  public async Task<IActionResult> GetEJSummarySubReportTData()
  {
    IEnumerable<EJSummarySubreportTData>? ejSummarySubReportTData = await _electronicJournalService.GetEJSummarySubReportTData();
    return Ok(ejSummarySubReportTData);
  }

  // GET api/ElectronicJournal/GetEJDetailsReportData
  [HttpGet]
  [Route("GetEJDetailsReportData")]
  public async Task<IActionResult> GetEJDetailsReportData(int? pageNumber = null, int? pageSize = null)
  {
    IEnumerable<EJDetailsReportData>? ejSummarySubReportTData = await _electronicJournalService.GetEJDetailsReportData(pageNumber, pageSize);
    return Ok(ejSummarySubReportTData);
  }

  // GET api/ElectronicJournal/GetEJTenderDetailsReportData
  [HttpGet]
  [Route("GetEJTenderDetailsReportData")]
  public async Task<IActionResult> GetEJTenderDetailsReportData(int saleNo)
  {
    string? ejTenderDetailsReportData = await _electronicJournalService.GetEJTenderDetailsReportData(saleNo);
    return Ok(ejTenderDetailsReportData);
  }
}
