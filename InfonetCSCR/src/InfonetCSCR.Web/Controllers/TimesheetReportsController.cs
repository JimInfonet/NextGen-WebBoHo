namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class TimesheetReportsController : Controller
{
  private readonly IHttpService _http;
  private readonly ITimesheetReportsService _timesheetReportsService;

  public TimesheetReportsController(
        IHttpService http,
        ITimesheetReportsService timesheetReportsService)
  {
    _http = http;
    _timesheetReportsService = timesheetReportsService;
  }

  public IActionResult Index()
  {
    return View();
  }

  public async Task<IActionResult> GetReports([FromBody] GetReportsDtoViewModel model)
  {
    string url = "TimesheetReports/GetReports";
    var reports = await _http.PostAndParseAsync<List<TimeSheetHeaderViewModel>>(url, model);
    return Ok(reports);
  }

  public async Task<IActionResult> GetTimesheetHeaderDetails([FromQuery] int reportId)
  {
    string url = $"TimesheetReports/GetTimesheetHeaderDetails?reportId={reportId}";
    var timesheetHeader = await _http.GetAndParseAsync<TimesheetHeaderWithUserDetailsDtoViewModel>(url);
    return Ok(timesheetHeader);
  }

  public async Task<IActionResult> GetOutstandingShortages(string userCode, int reportId, DateTime periodStarting)
  {
    string url = $"TimesheetReports/GetOutstandingShortages?userCode={userCode}&reportId={reportId}&periodStarting={periodStarting}";
    var outstandingShortages = await _http.GetAndParseAsync<List<OutstandingShortageDtoViewModel>>(url);
    return Ok(outstandingShortages);
  }

  public async Task<IActionResult> UpdateShortages()
  {
    string url = $"TimesheetReports/UpdateShortages";
    bool updated = await _http.PostAndParseAsync<bool>(url);
    return Ok(updated);
  }

  public async Task<IActionResult> UpdateMissingReportIdInShortages([FromQuery] string userCode, [FromQuery] int reportId, [FromQuery] DateTime periodStarting)
  {
    string url = $"TimesheetReports/UpdateMissingReportIdInShortages?userCode={userCode}&reportId={reportId}&periodStarting={periodStarting}";
    bool updated = await _http.PostAndParseAsync<bool>(url);
    return Ok(updated);
  }

  public async Task<IActionResult> GetTimeSheetDetails([FromQuery] string userCode, [FromQuery] int reportId)
  {
    string url = $"TimesheetReports/GetTimeSheetDetails?userCode={userCode}&reportId={reportId}";
    var timesheetDetails = await _http.GetAndParseAsync<List<TimesheetDetailDtoViewModel>>(url);
    return Ok(timesheetDetails);
  }

  public async Task<IActionResult> UpdateUniformPurchases([FromQuery] string userCode, [FromQuery] double uniformPurchases, [FromQuery] DateTime periodStarting, [FromQuery] DateTime periodEnding)
  {
    string url = $"TimesheetReports/UpdateUniformPurchases?userCode={userCode}&uniformPurchases={uniformPurchases}&periodStarting={periodStarting}&periodEnding={periodEnding}";
    bool updated = await _http.PostAndParseAsync<bool>(url);
    return Ok(updated);
  }

  public async Task<IActionResult> MissingCheckOutsExist([FromQuery] string userCode, [FromQuery] DateTime periodEnding)
  {
    string url = $"TimesheetReports/MissingCheckOutsExist?userCode={userCode}&periodEnding={periodEnding}";
    bool missingCheckOutsExist = await _http.GetAndParseAsync<bool>(url);
    return Ok(missingCheckOutsExist);
  }

  public async Task<IActionResult> MissingCommentExist([FromQuery] string userCode)
  {
    string url = $"TimesheetReports/MissingCommentExist?userCode={userCode}";
    bool missingCommentExist = await _http.GetAndParseAsync<bool>(url);
    return Ok(missingCommentExist);
  }

  public async Task<IActionResult> GeneratePYFile([FromQuery] GeneratePYFileDtoViewModel model, [FromBody] CheckInCheckOutPoliciesDtoViewModel policies)
  {
    (string contents, string storeCode) fileInfo = await _timesheetReportsService.GeneratePYFile(model, policies);
    return Json(new { content = fileInfo.contents, storeCode = fileInfo.storeCode });
  }

  public async Task<IActionResult> PostPayPeriod([FromQuery] string userCode, [FromQuery] string periodStarting)
  {
    string url = $"TimesheetReports/PostPayPeriod?userCode={userCode}&periodStarting={periodStarting}";
    bool posted = await _http.PostAndParseAsync<bool>(url);
    return Ok(posted);
  }

  public async Task<IActionResult> UpdateShiftForm([FromBody] UpdateShiftFormDtoViewModel model)
  {
    string url = $"TimesheetReports/UpdateShiftForm";
    var shiftFormData = await _http.PostAndParseAsync<UpdateShiftFormReturnDtoViewModel>(url, model);
    return Ok(shiftFormData);
  }
}
