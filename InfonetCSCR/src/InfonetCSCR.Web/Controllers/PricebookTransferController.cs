namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class PricebookTransferController : Controller
{
  private readonly IHttpService _http;
  private readonly IApplicationConfig _config;
  private readonly IPolicyService _policyService;
  private readonly ILogger<PricebookTransferController> _logger;

  public PricebookTransferController(
        IHttpService http,
        IApplicationConfig config,
        IPolicyService policyService,
        ILogger<PricebookTransferController> logger)
  {
    _http = http;
    _config = config;
    _logger = logger;
    _policyService = policyService;
  }

  [HttpGet]
  public async Task<IActionResult> Index(bool? fromDataDefinition)
  {
    if (_config.IsBackOffice) return NotFound();
    ViewData["UserCode"] = HttpContext.Session.GetString("UserCode");
    ViewData["FromDataDefinition"] = fromDataDefinition;
    return await Task.Run(() => View());
  }

  [HttpGet]
  public IActionResult GetPolicies()
  {
    var policies = new PricebookTransferPoliciesViewModel
    {
      NumberOfPrice = _policyService.GetPolicy(PolicyName.NUM_PRICE),
      CostDec = _policyService.GetPolicy(PolicyName.COST_DEC),
      MarginCalcCostMkup = _policyService.GetPolicy(PolicyName.MARGINCALC).ToString().ToUpper() == "CostMkUp".ToUpper(),
      SetVend = _policyService.GetPolicy(PolicyName.SET_VEND),
      UpdateAllVendor = _policyService.GetPolicy(PolicyName.UPD_NETW),
      DefMargin = _policyService.GetPolicy(PolicyName.DEF_PR_CHG),
      ConfirmPc = _policyService.GetPolicy(PolicyName.CONFIRM_PC),
      PackCost = _policyService.GetPolicy(PolicyName.PACKCOST),
      VendorPrice = !_config.IsBackOffice && _policyService.GetPolicy(PolicyName.VENDOR_PRICE)
    };

    return Ok(policies);
  }

  [HttpGet]
  public ActionResult ShowReport()
  {
    return PartialView("_Report");
  }

  [HttpGet]
  public async Task<IActionResult> GetVendors()
  {
    string url = "vendors/all";
    var vendors = await _http.GetAndParseAsync<List<VendorViewModel>>(url);
    return Ok(vendors);
  }

  [HttpGet]
  public async Task<IActionResult> GetDepartments()
  {
    string url = "categories/departments";
    var departments = await _http.GetAndParseAsync<List<DepartmentViewModel>>(url);
    return Ok(departments);
  }

  [HttpGet]
  public async Task<IActionResult> GetSubDepartments()
  {
    string url = "categories/subdepartments";
    var subDepartments = await _http.GetAndParseAsync<List<SubDepartmentViewModel>>(url);
    return Ok(subDepartments);
  }

  [HttpGet]
  public async Task<IActionResult> GetSubDetails()
  {
    string url = "Categories/Subdetail";
    var subDetails = await _http.GetAndParseAsync<List<SubDetailViewModel>>(url);
    return Ok(subDetails);
  }

  [HttpGet]
  public async Task<IActionResult> GetHotButtons()
  {
    string url = "HotButtons/GetHotButtonsWithProductName";
    var hotButons = await _http.GetAndParseAsync<List<HotButtonsWithProductNameViewModel>>(url);
    return Ok(hotButons);
  }

  [HttpGet]
  public async Task<IActionResult> GetPromotions()
  {
    string url = "PromotionalItems/GetPromotions";
    var promoHeaders = await _http.GetAndParseAsync<List<PromoHeaderViewModel>>(url);
    return Ok(promoHeaders);
  }

  [HttpPost]
  public async Task<IActionResult> PendingTransfers([FromBody] PendingTransferDtoViewModel model)
  {
    var userCode = HttpContext.Session.GetString("UserCode");
    model.TransUser = userCode;

    string url = "PricebookTransfer/PendingTransfers";
    var response = await _http.PostAsync(url, model);

    if (response.IsSuccessStatusCode)
    {
      return NoContent();
    }
    else
    {
      _logger.LogError($"Request failed POST {url} - Status code: {(int)response.StatusCode}.");
      return StatusCode((int)response.StatusCode);
    }
  }

  [HttpPost]
  public async Task<IActionResult> PendingCommodityTransfers([FromBody] PendingCommodityTransferDtoViewModel model)
  {
    var userCode = HttpContext.Session.GetString("UserCode");
    string url = $"PricebookTransfer/PendingCommodityTransfers?userCode={userCode}";
    var response = await _http.PostAsync(url, model);

    if (response.IsSuccessStatusCode)
    {
      return NoContent();
    }
    else
    {
      _logger.LogError($"Request failed POST {url} - Status code: {(int)response.StatusCode}.");
      return StatusCode((int)response.StatusCode);
    }
  }

  [HttpPost]
  public async Task<IActionResult> PendingHotButtonsTransfers([FromBody] PendingHotButtonsTransferDtoViewModel model)
  {
    var userCode = HttpContext.Session.GetString("UserCode");
    string url = $"PricebookTransfer/PendingHotButtonsTransfers?userCode={userCode}";
    var response = await _http.PostAsync(url, model);

    if (response.IsSuccessStatusCode)
    {
      return NoContent();
    }
    else
    {
      _logger.LogError($"Request failed POST {url} - Status code: {(int)response.StatusCode}.");
      return StatusCode((int)response.StatusCode);
    }
  }

  [HttpPost]
  public async Task<IActionResult> PendingPromotionTransfers([FromBody] PendingPromotionTransferDtoViewModel model)
  {
    var userCode = HttpContext.Session.GetString("UserCode");
    string url = $"PricebookTransfer/PendingPromotionTransfers?userCode={userCode}";
    var response = await _http.PostAsync(url, model);

    if (response.IsSuccessStatusCode)
    {
      return NoContent();
    }
    else
    {
      _logger.LogError($"Request failed POST {url} - Status code: {(int)response.StatusCode}.");
      return StatusCode((int)response.StatusCode);
    }
  }

  [HttpGet]
  public async Task<IActionResult> GetPriceChangeTransfers(string? storeId)
  {
    string url = $"PricebookTransfer/GetPriceChangeTransfers";
    if (storeId != null) url = $"{url}?storeId={storeId}";

    var priceChangeTransfers = await _http.GetAndParseAsync<List<PriceChangeTransferViewModel>>(url);
    return Ok(priceChangeTransfers);
  }

  [HttpGet]
  public async Task<IActionResult> GetCommodityTransfers([FromQuery] string? storeId)
  {
    string url = $"PricebookTransfer/GetCommodityTransfers?storeId={storeId}";
    var commodityTransfers = await _http.GetAndParseAsync<IEnumerable<CommodityTransferViewModel>>(url);
    return Ok(commodityTransfers);
  }

  [HttpGet]
  public async Task<IActionResult> GetCostChangeTransfers(string? storeId)
  {
    string url = $"PricebookTransfer/GetCostChangeTransfers?storeId={storeId}";
    var costChangeTransfers = await _http.GetAndParseAsync<IEnumerable<CostChangeTransferViewModel>>(url);
    return Ok(costChangeTransfers);
  }

  [HttpGet]
  public async Task<IActionResult> GetHotButtonsTransfers([FromQuery] string? storeId)
  {
    string url = $"PricebookTransfer/GetHotButtonsTransfers?storeId={storeId}";
    var hotButtonsTransfers = await _http.GetAndParseAsync<List<HotButtonsTransferViewModel>>(url);
    return Ok(hotButtonsTransfers);
  }

  [HttpGet]
  public async Task<IActionResult> GetPromotionTransfers([FromQuery] string? storeId)
  {
    string url = $"PricebookTransfer/GetPromotionTransfers?storeId={storeId}";
    var promotionTransfers = await _http.GetAndParseAsync<List<PromotionTransferViewModel>>(url);
    return Ok(promotionTransfers);
  }

  [HttpGet]
  public async Task<IActionResult> GetPendingTransferStores(string dataTransferType)
  {
    string url = $"PricebookTransfer/GetPendingTransferStores?dataTransferType={dataTransferType}";
    var stores = await _http.GetAndParseAsync<List<StoreViewModel>>(url);
    return Ok(stores);
  }

  [HttpPost]
  public async Task<IActionResult> GetStockTransfersByStoreId([FromBody] ProductSearchParameterViewModel model)
  {
    string url = $"PricebookTransfer/GetStockTransfersByStoreId";
    var stockTransfers = await _http.PostAndParseAsync<StockTransferFilterViewModel>(url, model);
    return Ok(new
    {
      iTotalRecords = stockTransfers == null ? 0 : stockTransfers.TotalItems,           //Do not Rename variable name
      iTotalDisplayRecords = stockTransfers == null ? 0 : stockTransfers.TotalItems,    //Do not Rename variable name
      Data = stockTransfers?.stockTransferDtos                                                       //Do not Rename variable name
    });
  }

  [HttpPost]
  public async Task<IActionResult> PerformTransfer(string dataTransferType, PricebookTransferPoliciesViewModel policies)
  {
    var userCode = HttpContext.Session.GetString("UserCode");
    string url = $"PricebookTransfer/PerformTransfer?dataTransferType={dataTransferType}&userCode={userCode}";
    var response = await _http.PostAsync(url, policies);

    if (response.IsSuccessStatusCode)
    {
      return NoContent();
    }
    else
    {
      _logger.LogError($"Request failed POST {url} - Status code: {(int)response.StatusCode}.");
      return StatusCode((int)response.StatusCode);
    }
  }

  public async Task<IActionResult> DeleteFromTransferList(DeleteFromTransferListDtoViewModel model)
  {
    string url = "PricebookTransfer/DeleteFromTransferList";
    var response = await _http.PostAsync(url, model);

    if (response.IsSuccessStatusCode)
    {
      return NoContent();
    }
    else
    {
      _logger.LogError($"Request failed POST {url} - Status code: {(int)response.StatusCode}.");
      return StatusCode((int)response.StatusCode);
    }
  }

  [HttpGet]
  public async Task<IActionResult> GetLabelPrints()
  {
    string url = $"PricebookTransfer/GetLabelPrints";
    var reportData = await _http.GetAndParseAsync<List<StockItemMinimalViewModel>>(url);
    return Ok(reportData);
  }

  [HttpPost]
  public async Task<IActionResult> DeleteLabelPrint([FromBody] string? stockCode = null)
  {
    string url = $"PricebookTransfer/DeleteLabelPrint";
    await _http.PostAsync(url, stockCode);
    return NoContent();
  }

  [HttpGet]
  public async Task<IActionResult> GetPriceChangeReportData()
  {
    string url = $"/DataTransferReports/GetPricechangeReportData";
    var reportData = await _http.GetAndParseAsync<List<PriceChangeReportViewModel>>(url);
    return Ok(reportData);
  }

  [HttpGet]
  public async Task<IActionResult> GetStockTransferReportData()
  {
    string url = $"/DataTransferReports/GetStockTransferReportData";
    var reportData = await _http.GetAndParseAsync<List<StockTransferReportViewModel>>(url);
    return Ok(reportData);
  }

  [HttpGet]
  public async Task<IActionResult> GetCommodityTransferReportData()
  {
    string url = $"/DataTransferReports/GetCommodityTransferReportData";
    var reportData = await _http.GetAndParseAsync<List<CommodityTransferReportViewModel>>(url);
    return Ok(reportData);
  }

  [HttpGet]
  public async Task<IActionResult> GetHotButtonsReportData()
  {
    string url = $"/DataTransferReports/GetHotButtonsReportData";
    var reportData = await _http.GetAndParseAsync<List<HotButtonsReportViewModel>>(url);
    return Ok(reportData);
  }

  [HttpGet]
  public async Task<IActionResult> GetCostChangeReportData()
  {
    string url = $"/DataTransferReports/GetCostchangeReportData";
    var reportData = await _http.GetAndParseAsync<List<CostChangeReportViewModel>>(url);
    return Ok(reportData);
  }

  [HttpGet]
  public async Task<IActionResult> GetPromotionalDiscountTransferReportData()
  {
    string url = $"/DataTransferReports/GetPromotionalDiscountTransferReportData";
    var reportData = await _http.GetAndParseAsync<List<PromotionalDiscountTransferReportDto>>(url);
    return Ok(reportData);
  }
}
