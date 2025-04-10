namespace InfonetCSCR.Web.Controllers;

public class ElectronicJournalController : Controller
{
  private readonly IElectronicJournalService _electronicJournalService;

  public ElectronicJournalController(IElectronicJournalService electronicJournalService)
  {
    _electronicJournalService = electronicJournalService;
  }

  public async Task<IActionResult> Index()
  {
    return View();
  }

  public async Task<IActionResult> ElectronicJournalReports()
  {
    return View();
  }

  public async Task<IActionResult> ElectronicJournalDetailReport(int pageNumber = 1, int pageSize = 10, int? totalRecords = null)
  {
    if(!totalRecords.HasValue)
    {
      totalRecords = (await _electronicJournalService.GetEJDetailReportData()).Count();
    }

    ViewBag.CurrentPage = pageNumber;
    ViewBag.PageSize = pageSize;
    ViewBag.TotalRecords = totalRecords;
    var ejDetailReportData = await _electronicJournalService.GetEJDetailReportData(pageNumber, pageSize);
    return PartialView("_EJDetailReport", ejDetailReportData);
  }

  public async Task<IActionResult> GetElectronicJournalOptions()
  {
    IEnumerable<string>? options = await _electronicJournalService.GetElectronicJournalOptions();
    return Ok(options);
  }

  public async Task<IActionResult> GetElectronicJournalFilters(string optionName, bool isWareHouse = false)
  {
    IEnumerable<EJSearchDescriptionViewModel>? options = await _electronicJournalService.GetElectronicJournalFilters(optionName, isWareHouse);
    return Ok(options);
  }

  public async Task<IActionResult> GetSearchDropdownValues([FromBody] EJSearchDescriptionViewModel desc)
  {
    IEnumerable<string>? values = await _electronicJournalService.GetSearchDropdownValues(desc);
    return Ok(values);
  }

  public async Task<IActionResult> GetSetupCStore()
  {
    return Json(await _electronicJournalService.GetSetupCStore());
  }

  public async Task<IActionResult> GetStoreDropdownValues()
  {
    IEnumerable<StoreOptionViewModel>? storeOptions = await _electronicJournalService.GetStoreDropdownValues();
    return Ok(storeOptions);
  }

  public async Task<IActionResult> GetElectronicJournalGridData([FromBody] EJSearchParamViewModel ejSearchParam)
  {
    EJGridDataFilterViewModel ejGridFilterData = await _electronicJournalService.GetElectronicJournalGridData(ejSearchParam);

    return Ok(new JQueryDataTableFilterResult
    {
      ITotalRecords = ejGridFilterData.TotalItems,
      ITotalDisplayRecords = ejGridFilterData.TotalItems,
      Data = ejGridFilterData.ElectronicJournalGridData
    });
  }
}
