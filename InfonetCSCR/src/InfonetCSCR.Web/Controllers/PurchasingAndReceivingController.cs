namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class PurchasingAndReceivingController : Controller
{
  private readonly IHttpService _http;
  private readonly IPurchasingAndReceivingService _purchasingAndReceivingService;

  public PurchasingAndReceivingController(IHttpService http, IPurchasingAndReceivingService purchasingAndReceivingService)
  {
    _http = http;
    _purchasingAndReceivingService = purchasingAndReceivingService;
  }

  public IActionResult Index(string? poStatus, string? selectedPurchaseNo)
  {
    ViewData["UserCode"] = HttpContext.Session.GetString("UserCode");
    ViewData["PoStatus"] = poStatus;
    ViewData["SelectedPurchaseNo"] = selectedPurchaseNo;
    return View();
  }

  public IActionResult GetPurchasingAndReceivingPolicies()
  {
    PurchasingAndReceivingPoliciesViewModel policies = _purchasingAndReceivingService.GetPolicies();
    return Ok(policies);
  }

  public async Task<IActionResult> GetPurchaseHeadersByPOStatus(string poStatus, int columnIndex)
  {
    string url = $"PurchaseHeader/GetPurchaseHeadersByPOStatus/{poStatus}/{columnIndex}";
    var purchaseHeaders = await _http.GetAndParseAsync<IEnumerable<PurchaseHeaderViewModel>?>(url);
    return Ok(purchaseHeaders);
  }

  public async Task<IActionResult> GetPOFileData(long purchaseNo)
  {
    string url = $"PurchaseDetail/GetPOFileData?purchaseNo={purchaseNo}";
    var pofileData = await _http.GetAndParseAsync<POFileViewModel>(url);
    return Ok(pofileData);
  }

  [HttpPost]
  public async Task<IActionResult> UpdatePurchaseHeader([FromBody] PurchaseHeaderViewModel model)
  {
    string url = "PurchaseHeader/UpdatePurchaseHeaderEmailedStatus";
    await _http.PostAsync(url, model);
    return NoContent();
  }

  public async Task<IActionResult> GetVendorByPurchaseNo(long purchaseNo)
  {
    string url = $"Vendors/GetVendorByPurchaseNo?purchaseNo={purchaseNo}";
    var vendor = await _http.GetAndParseAsync<VendorViewModel>(url);
    return Ok(vendor);
  }

  public async Task<IActionResult> GetActiveVendorsOrderByColumnNumber(int columnNumber)
  {
    string url = $"vendors/GetActiveVendorsOrderByColumnNumber/{columnNumber}";
    var activeVendors = await _http.GetAndParseAsync<IEnumerable<VendorViewModel>?>(url);
    return Ok(activeVendors);
  }

  public async Task<IActionResult> AddPurchasingAndReceiving([FromBody] PurchasingAndReceivingViewModel model)
  {
    model.Policies = _purchasingAndReceivingService.GetPolicies();

    string url = "PurchasingAndReceiving/AddPurchasingAndReceiving";
    await _http.PostAsync(url, model);
    return NoContent();
  }

  public async Task<IActionResult> DeletePurchasingAndReceivingByPurchaseNo(string purchaseNo)
  {
    string url = $"PurchasingAndReceiving/DeletePurchasingAndReceivingByPurchaseNo/{purchaseNo}";
    await _http.PostAsync(url);
    return NoContent();
  }

  public async Task<IActionResult> GetShiftIdAndDateByShiftDate(string shiftDate)
  {
    string url = $"Shift/GetShiftIdAndDateByShiftDate?shiftDate={shiftDate}";
    var purchaseHeaders = await _http.GetAndParseAsync<IEnumerable<ShiftRecHeaderDtoViewModel>?>(url);
    return Ok(purchaseHeaders);
  }

  public async Task<IActionResult> GetShiftNumberByShiftDate(string shiftDate)
  {
    shiftDate ??= "";
    string url = $"Shift/GetShiftNumberByShiftDate?shiftDate={shiftDate}";
    var shiftNumbers = await _http.GetAndParseAsync<IEnumerable<string>?>(url);
    return Ok(shiftNumbers);
  }

  [HttpPost]
  public async Task<IActionResult> POPayment([FromBody] PurchaseAndReceivePostViewModel purchaseAndReceivePostViewModel, bool costChange)
  {
    var transferPOFile = await _purchasingAndReceivingService.POPayment(purchaseAndReceivePostViewModel, costChange);
    return Ok(transferPOFile);
  }
}
