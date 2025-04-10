namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class StockPriceController : Controller
{
  private readonly IHttpService _http;
  private readonly IApplicationConfig _config;
  private readonly IPolicyService _policyService;
  private readonly IHttpContextAccessor _contextAccessor;

  public StockPriceController(
    IHttpService http,
    IApplicationConfig config,
    IPolicyService policyService,
    IHttpContextAccessor contextAccessor)
  {
    _http = http;
    _config = config;
    _policyService = policyService;
    _contextAccessor = contextAccessor;
  }

  public IActionResult Index(string StockCode)
  {
    string UseARInvoices = _policyService.GetPolicy(PolicyName.USE_ARINVOICES) ? "Yes" : "No";
    ViewData["StockCode"] = StockCode;
    ViewData["UseARInvoices"] = UseARInvoices;
    return View();
  }

  public async Task<IActionResult> GetStockPricePolicies(string stockCode)
  {
    string url = $"StockItems/GetStockItemByCode/{stockCode}";
    var stockItem = await _http.GetAndParseAsync<ProductViewModel>(url);

    var policies = new StockPricePoliciesViewModel
    {
      NumberOfPrice = _policyService.GetPolicy(PolicyName.NUM_PRICE),
      CostType = _policyService.GetPolicy(PolicyName.COST_TYPE),
      UseTaxExempt = _policyService.GetPolicy(PolicyName.TAX_EXEMPT),
      PriceDec = _policyService.GetPolicy(PolicyName.PRICE_DEC, stockItem),
      CostDec = _policyService.GetPolicy(PolicyName.COST_DEC),
      SetVendor = _policyService.GetPolicy(PolicyName.SET_VEND),
      MarginCalcCostMkup = _policyService.GetPolicy(PolicyName.MARGINCALC).ToString().ToUpper() == "CostMkUp".ToUpper(),
      AllowNegativePrice = _policyService.GetPolicy(PolicyName.NEG_PRICE, stockItem),
      DefMargin = _policyService.GetPolicy(PolicyName.DEF_PR_CHG),
      DefPrice = _policyService.GetPolicy(PolicyName.DEF_PRICE),
      Warn0Price = _policyService.GetPolicy(PolicyName.WARN_0_PRICE),
      ConfirmPriceChange = _policyService.GetPolicy(PolicyName.CONFIRM_PC),
      UpdateNetWork = _policyService.GetPolicy(PolicyName.UPD_NETW),
      UseArinovices = _policyService.GetPolicy(PolicyName.USE_ARINVOICES),
      UpdVends = _policyService.GetPolicy(PolicyName.UPD_VENDS),
      UpdateAllStores = false,
      UpdateAll = false,
      UserAccessStockCode = true,
      ForceCost = false,
    };
    policies.PackCost = _policyService.GetPolicy(PolicyName.PACKCOST) && policies.CostType == "Standard";

    if (_config.IsBackOffice)
    {
      var user = new UserViewModel
      {
        Code = _contextAccessor.HttpContext?.Session.GetString("UserCode") ?? string.Empty
      };

      policies.UserAccessStockCode = _policyService.GetPolicy(PolicyName.U_STOCK, user);
    }
    else
    {
      policies.UpdateAllStores = _policyService.GetPolicy(PolicyName.UPD_STORES);
    }

    return Ok(policies);
  }

  public async Task<IActionResult> GetPriceGroup()
  {
    string url = "StockPrice/PriceGroup";
    var priceGroups = await _http.GetAndParseAsync<IEnumerable<PriceGroupViewModel>>(url);
    return Ok(priceGroups);
  }

  [HttpPost]
  public async Task<IActionResult> GetStockPrice([FromBody] GetStockPriceViewModel model)
  {
    bool isUseARInvoicePolicy = (!_config.IsBackOffice && _policyService.GetPolicy(PolicyName.USE_ARINVOICES));
    model.PricesInUse = _policyService.GetPolicy(PolicyName.NUM_PRICE);

    string url = $"StockPrice?isUseARInvoicePolicy={isUseARInvoicePolicy}";
    var stockPrice = await _http.PostAndParseAsync<StockPriceFormViewModel>(url, model);
    return Ok(stockPrice);
  }

  public async Task<IActionResult> GetStockPriceInfo([FromQuery] GetStockPriceInfoViewModel model)
  {
    string url = $@"StockPrice/GetStockPriceInfo?storeId={model.StoreId}&stockCode={model.StockCode}&vendorId={model.VendorId}&priceNumber={model.PriceNumber}&orderNo={model.OrderNo}";
    var stockPriceInfo = await _http.GetAndParseAsync<StockPriceInfoViewModel>(url);
    return Ok(stockPriceInfo);
  }

  [HttpPost]
  public async Task<IActionResult> GetStockPriceInfoBatch([FromBody] List<GetStockPriceInfoViewModel> models)
  {
    var url = "StockPrice/GetStockPriceInfoBatch";
    var stockPriceInfos = await _http.PostAndParseAsync<IEnumerable<StockPriceInfoViewModel>>(url, models);
    return Ok(stockPriceInfos);
  }

  [HttpPost]
  public async Task<IActionResult> UpdateVendorPartNo([FromBody] VendorPartViewModel model)
  {
    if (!ModelState.IsValid || model.VendorID == null || model.StockCode == null) return BadRequest();

    string url = "StockPrice/UpdateVendorPartNo";
    var stockPriceVendors = await _http.PostAndParseAsync<IEnumerable<StockPriceVendorsViewModel>>(url, model);
    return Ok(stockPriceVendors);
  }

  [HttpPost]
  public async Task<IActionResult> GetVendorsNotInStockCost([FromBody] StockPriceModel model)
  {
    var policySetVend = _policyService.GetPolicy(PolicyName.SET_VEND);
    if (!ModelState.IsValid || (model.VendorID == null && !policySetVend) || model.StockCode == null)
    {
      return BadRequest();
    }

    string url = "StockPrice/VendorsNotInStockCost";
    var stockPriceVendors = await _http.PostAndParseAsync<IEnumerable<StockPriceVendorsViewModel>>(url, model);
    return Ok(stockPriceVendors);
  }

  [HttpPost]
  public async Task<IActionResult> AddVendorToStockCost([FromBody] StockCostsViewModel model)
  {
    if (!ModelState.IsValid || model.VendorID == null || model.StockCode == null) return BadRequest();

    string url = "StockPrice/AddVendorStockCost";
    var stockPriceVendors = await _http.PostAndParseAsync<IEnumerable<StockPriceVendorsViewModel>>(url, model);
    return Ok(stockPriceVendors);
  }

  [HttpPost]
  public async Task<IActionResult> ToggleVendorStatus([FromBody] StockPriceModel model)
  {
    if (!ModelState.IsValid || model.VendorID == null || model.StockCode == null) return BadRequest();

    string url = "StockPrice/ToggleVendorStatus";
    var stockPriceVendors = await _http.PostAndParseAsync<IEnumerable<StockPriceVendorsViewModel>>(url, model);
    return Ok(stockPriceVendors);
  }

  [HttpPost]
  public async Task<IActionResult> UpdateStockCost([FromBody] CostViewModel model)
  {
    if (!ModelState.IsValid || model.StockCode == null) return BadRequest();

    string url = "StockPrice/UpdateStockCost";
    var stockCosts = await _http.PostAndParseAsync<IEnumerable<StockCostsViewModel>>(url, model);
    return Ok(stockCosts);
  }

  [HttpPost]
  public async Task<IActionResult> InsertFutureStockCosts([FromBody] StockCostsListModel model)
  {
    string url = "StockPrice/InsertFutureStockCosts";
    var stockCosts = await _http.PostAndParseAsync<IEnumerable<StockCostsViewModel>>(url, model);
    return Ok(stockCosts);
  }

  [HttpPost]
  public async Task<IActionResult> DeleteVendor([FromBody] StockPriceModel model)
  {
    if (!ModelState.IsValid || model.VendorID == null || model.StockCode == null) return BadRequest();

    string url = "StockPrice/DeleteVendor";
    var stockPriceVendors = await _http.PostAndParseAsync<IEnumerable<StockPriceVendorsViewModel>>(url, model);
    return Ok(stockPriceVendors);
  }

  [HttpPost]
  public async Task<IActionResult> UpdateStockPrice([FromBody] PriceViewModel model)
  {
    if (!ModelState.IsValid || model.VendorID == null || model.StockCode == null) return BadRequest();

    string url = "StockPrice/UpdateStockPrice";
    var stockPrices = await _http.PostAndParseAsync<IEnumerable<StockPriceViewModel>>(url, model);
    return Ok(stockPrices);
  }

  [HttpPost]
  public async Task<IActionResult> InsertFuturePrice([FromBody] PriceViewModel model)
  {
    if (!ModelState.IsValid || model.VendorID == null || model.StockCode == null) return BadRequest();

    string url = "StockPrice/InsertFuturePrice";
    var stockPrices = await _http.PostAndParseAsync<IEnumerable<StockPriceViewModel>>(url, model);
    return Ok(stockPrices);
  }

  [HttpPost]
  public async Task<IActionResult> AddRebate([FromBody] StockRebatesViewModel model)
  {
    if (!ModelState.IsValid || model.VendorID == null || model.StockCode == null) return BadRequest();

    string url = "StockPrice/AddRebate";
    var stockRebates = await _http.PostAndParseAsync<IEnumerable<StockRebatesViewModel>>(url, model);
    return Ok(stockRebates);
  }

  [HttpPost]
  public async Task<IActionResult> DeleteRebate([FromBody] StockRebatesViewModel model)
  {
    if (!ModelState.IsValid || model.VendorID == null || model.StockCode == null) return BadRequest();

    string url = "StockPrice/DeleteRebate";
    var stockRebates = await _http.PostAndParseAsync<IEnumerable<StockRebatesViewModel>>(url, model);
    return Ok(stockRebates);
  }

  [HttpPost]
  public async Task<IActionResult> AddSpecialPrice([FromBody] SpecialPriceViewModel model)
  {
    if (!ModelState.IsValid || model.VendorID == null || model.StockCode == null) return BadRequest();

    bool isUseARInvoice = (!_config.IsBackOffice && _policyService.GetPolicy(PolicyName.USE_ARINVOICES));
    bool isSetVendor = _policyService.GetPolicy(PolicyName.SET_VEND);
    string url = $"StockPrice/AddSpecialPrice?isUseARInvoice={isUseARInvoice}&isSetVendor={isSetVendor}";
    var stockPrices = await _http.PostAndParseAsync<IEnumerable<SpecialPriceViewModel>>(url, model);
    return Ok(stockPrices);
  }

  [HttpPost]
  public async Task<IActionResult> UpdateSpecialPriceForUseARInvoicePolicy([FromBody] SpecialPriceViewModel model)
  {
    if (!ModelState.IsValid || model.StockCode == null) return BadRequest();

    string url = "StockPrice/UpdateSpecialPriceForUseARInvoicePolicy";
    var stockPrices = await _http.PostAndParseAsync<IEnumerable<SpecialPriceViewModel>>(url, model);
    return Ok(stockPrices);
  }

  [HttpPost]
  public async Task<IActionResult> DeleteSpecialPrice([FromBody] SpecialPriceViewModel model)
  {
    model.UseArInvoice = (!_config.IsBackOffice && _policyService.GetPolicy(PolicyName.USE_ARINVOICES));
    if (!ModelState.IsValid || (model.VendorID == null && !model.UseArInvoice) || model.StockCode == null)
    {
      return BadRequest();
    }

    string url = "StockPrice/DeleteSpecialPrice";
    var stockPrices = await _http.PostAndParseAsync<IEnumerable<SpecialPriceViewModel>>(url, model);
    return Ok(stockPrices);
  }

  public IActionResult GetNetworkUpdateSettings(string level, string value)
  {
    var policies = new StockPricePoliciesViewModel();
    policies.UpdateAll = _policyService.GetNetworkUpdateSettings(level, value);

    return Ok(policies);
  }
}
