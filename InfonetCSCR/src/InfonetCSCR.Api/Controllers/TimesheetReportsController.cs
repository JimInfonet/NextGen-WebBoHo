namespace InfonetCSCR.Api.Controllers;

[Route("api/TimesheetReports")]
[ApiController]
[Authorize]

public class TimesheetReportsController : Controller
{
  #region Private Properties and Constructor
  private readonly ITimesheetReportsService _timesheetReportsService;
  private readonly ICheckInCheckOutService _checkInCheckOutService;

  public TimesheetReportsController(
    ITimesheetReportsService timesheetReportsService,
    ICheckInCheckOutService checkInCheckOutService)
  {
    _timesheetReportsService = timesheetReportsService;
    _checkInCheckOutService = checkInCheckOutService;
  }
  #endregion

  // POST api/TimesheetReports/GetReports
  [HttpPost]
  [Route("GetReports")]
  public async Task<IActionResult> GetReports(GetReportsDto model)
  {
    List<TimeSheetHeader> reports = await _timesheetReportsService.GetReports(model);
    return Ok(reports);
  }

  // GET api/TimesheetReports/GetTimesheetHeaderDetails
  [HttpGet]
  [Route("GetTimesheetHeaderDetails")]
  public async Task<IActionResult> GetTimesheetHeaderDetails([FromQuery] int reportId)
  {
    TimesheetHeaderWithUserDetailsDto timesheetHeader = await _timesheetReportsService.GetTimesheetHeaderDetails(reportId);
    return Ok(timesheetHeader);
  }

  // GET api/TimesheetReports/GetOutstandingShortages
  [HttpGet]
  [Route("GetOutstandingShortages")]
  public async Task<IActionResult> GetOutstandingShortages(string userCode, int reportId, DateTime periodStarting)
  {
    List<OutstandingStortageDto> outstandingShortages = await _timesheetReportsService.GetOutstandingShortages(userCode, reportId, periodStarting);
    return Ok(outstandingShortages);
  }

  // POST api/TimesheetReports/UpdateShortages
  [HttpPost]
  [Route("UpdateShortages")]
  public async Task<IActionResult> UpdateShortages()
  {
    bool updated = await _timesheetReportsService.UpdateShortages();
    return Ok(updated);
  }

  // POST api/TimesheetReports/UpdateMissingReportIdInShortages
  [HttpPost]
  [Route("UpdateMissingReportIdInShortages")]
  public async Task<IActionResult> UpdateMissingReportIdInShortages([FromQuery] string userCode, [FromQuery] int reportId, [FromQuery] DateTime periodStarting)
  {
    bool updated = await _timesheetReportsService.UpdateMissingReportIdInShortages(userCode, reportId, periodStarting);
    return Ok(updated);
  }

  // GET api/TimesheetReports/GetTimeSheetDetails
  [HttpGet]
  [Route("GetTimeSheetDetails")]
  public async Task<IActionResult> GetTimeSheetDetails([FromQuery] string userCode, [FromQuery] int reportId)
  {
    List<TimesheetDetailDto> details = await _timesheetReportsService.GetTimeSheetDetailsByReportId(userCode, reportId);
    return Ok(details);
  }

  // POST api/TimesheetReports/UpdateUniformPurchases
  [HttpPost]
  [Route("UpdateUniformPurchases")]
  public async Task<IActionResult> UpdateUniformPurchases([FromQuery] string userCode, [FromQuery] double uniformPurchases, [FromQuery] DateTime periodStarting, [FromQuery] DateTime periodEnding)
  {
    bool updated = await _timesheetReportsService.UpdateUniformPurchases(userCode, uniformPurchases, periodStarting, periodEnding);
    return Ok(updated);
  }

  // GET api/TimesheetReports/MissingCheckOutsExist
  [HttpGet]
  [Route("MissingCheckOutsExist")]
  public async Task<IActionResult> MissingCheckOutsExist([FromQuery] string userCode, [FromQuery] DateTime periodEnding)
  {
    bool missingCheckOutsExist = await _timesheetReportsService.MissingCheckOutsExist(userCode, periodEnding);
    return Ok(missingCheckOutsExist);
  }

  // GET api/TimesheetReports/MissingCommentExist
  [HttpGet]
  [Route("MissingCommentExist")]
  public async Task<IActionResult> MissingCommentExist([FromQuery] string userCode)
  {
    bool missingCommentExist = await _timesheetReportsService.MissingCommentExist(userCode);
    return Ok(missingCommentExist);
  }

  // POST api/TimesheetReports/PostPayPeriod
  [HttpPost]
  [Route("PostPayPeriod")]
  public async Task<IActionResult> PostPayPeriod([FromQuery] string userCode, [FromQuery] DateTime periodStarting)
  {
    bool posted = await _timesheetReportsService.PostPayPeriod(userCode, periodStarting);
    return Ok(posted);
  }

  // POST api/TimesheetReports/GetDataToGeneratePYFile
  [HttpPost]
  [Route("GetDataToGeneratePYFile")]
  public async Task<IActionResult> GetDataToGeneratePYFile(GeneratePYFileDto model)
  {
    GeneratePYFileReturnDto data = await _timesheetReportsService.GetDataToGeneratePYFile(model);
    return Ok(data);
  }

  // POST api/TimesheetReports/UpdateShiftForm
  [HttpPost]
  [Route("UpdateShiftForm")]
  public async Task<IActionResult> UpdateShiftForm(UpdateShiftFormDto model)
  {
    UpdateShiftFormReturnDto data = await _timesheetReportsService.UpdateShiftForm(model);
    return Ok(data);
  }

  // POST api/TimesheetReports/GetTimeSheetReportData
  [HttpGet]
  [Route("GetTimeSheetReportData")]
  public async Task<IActionResult> GetTimeSheetReportData(long reportId, string userCode, DateTime shiftDate)
  {
    TimeSheetReportDto data = await _timesheetReportsService.GetTimeSheetReportData(reportId, userCode, shiftDate);
    return Ok(data);
  }
}
