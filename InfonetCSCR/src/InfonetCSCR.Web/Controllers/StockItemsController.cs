namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class StockItemsController : Controller
{
  private readonly IHttpService _http;
  private readonly IApplicationConfig _config;
  private readonly IPolicyService _policyService;
  private readonly IHttpContextAccessor _contextAccessor;

  public StockItemsController(
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

  public IActionResult Index(bool? isFromStockForm)
  {
    ViewData["isFromStockForm"] = isFromStockForm;
    if (_contextAccessor?.HttpContext?.User.Identity == null || !_contextAccessor.HttpContext.User.Identity.IsAuthenticated)
    {
      return RedirectToAction("Index", "Auth");
    }

    ViewData["TaxExemptPolicy"] = _policyService.GetPolicy(PolicyName.TAX_EXEMPT);
    return View();
  }

  public async Task<IActionResult> GetStockItems([FromBody] JqueryDatatableParam jqueryDatatableParam,
                                          string? stockCodeSearchText,
                                          string? selectVendor,
                                          string? descriptionSearchText,
                                          string? alternateCodeSearchText,
                                          string? keywordSearchText,
                                          string? selectPriceType,
                                          string? vendorPartSearchText,
                                          string? saleItemsOnly,
                                          string? scalableItemsOnly,
                                          string? taxExemptItemsOnly,
                                          string? inactiveItemsOnly,
                                          string? selectDepartment,
                                          string? selectSubDepartment,
                                          string? selectSubDetail,
                                          bool? showDetails,
                                          bool? vendorPartSelected)
  {
    bool selectDetail = false;
    if (showDetails != null) selectDetail = (bool)showDetails;
    bool isVendorPartSelected = false;
    if (vendorPartSelected != null) isVendorPartSelected = (bool)vendorPartSelected;

    stockCodeSearchText = $"{stockCodeSearchText}%";
    selectVendor += '%';
    descriptionSearchText += '%';
    alternateCodeSearchText += "";
    keywordSearchText = $"%{keywordSearchText}%";
    selectPriceType += '%';
    vendorPartSearchText += '%';
    saleItemsOnly += '%';
    scalableItemsOnly += '%';
    taxExemptItemsOnly += '%';
    inactiveItemsOnly += '%';
    selectDepartment += '%';
    selectSubDepartment += '%';
    selectSubDetail += '%';

    SearchCriteriaViewModel searchCriteria = new()
    {
      StockCodeSearchText = stockCodeSearchText,
      SelectVendor = selectVendor,
      DescriptionSearchText = descriptionSearchText,
      AlternateCodeSearchText = alternateCodeSearchText,
      KeywordSearchText = keywordSearchText,
      SelectPriceType = selectPriceType,
      VendorPartSearchText = vendorPartSearchText,
      SaleItemsOnly = saleItemsOnly,
      ScalableItemsOnly = scalableItemsOnly,
      TaxExemptItemsOnly = taxExemptItemsOnly,
      InactiveItemsOnly = inactiveItemsOnly,
      SelectDepartment = selectDepartment,
      SelectSubDepartment = selectSubDepartment,
      SelectSubDetail = selectSubDetail,
      ShowDetails = selectDetail,
      VendorPartSelected = isVendorPartSelected,
      Length = jqueryDatatableParam.Length,
      Start = jqueryDatatableParam.Start
    };

    string url = "StockItems/GetStockItemsByFilter";
    var stockItemsResult = await _http.PostAndParseAsync<StockItemsResultModel>(url, searchCriteria);
    return Ok(new JQueryDataTableFilterResult
    {
      ITotalRecords = stockItemsResult.totalItems,
      ITotalDisplayRecords = stockItemsResult.totalItems,
      Data = stockItemsResult.stockItems
    });
  }

  public IActionResult SearchCriteria()
  {
    return View();
  }

  [HttpPost]
  public async Task<IActionResult> GetStockItemsByVendor([FromBody] GetStockItemsByVendorDtoViewModel searchParams)
  {
    string url = "StockItems/GetStockItemsByVendor";
    var stockItemsFilterData = await _http.PostAndParseAsync<StockItemMinimalFilterDtoViewModel>(url, searchParams);
    return Ok(new JQueryDataTableFilterResult
    {
      ITotalRecords = stockItemsFilterData.TotalItems,
      ITotalDisplayRecords = stockItemsFilterData.TotalItems,
      Data = stockItemsFilterData.StockItems
    });
  }

  [HttpPost]
  public async Task<IActionResult> GetStockItemsByDeptSubDeptAndSubDetail([FromBody] GetStockItemsByDeptSubDeptAndSubDetailViewModel searchParams)
  {
    string url = "StockItems/GetStockItemsByDeptSubDeptAndSubDetail";
    var stockItemsFilterData = await _http.PostAndParseAsync<StockItemMinimalFilterDtoViewModel>(url, searchParams);
    return Ok(new JQueryDataTableFilterResult
    {
      ITotalRecords = stockItemsFilterData.TotalItems,
      ITotalDisplayRecords = stockItemsFilterData.TotalItems,
      Data = stockItemsFilterData.StockItems
    });
  }

  public async Task<IActionResult> GetStockCostInfo(GetStockCostInfoViewModel model)
  {
    var url = $"StockItems/GetStockCostInfo?storeId={model.StoreId}&stockCode={model.StockCode}&orderNo={model.OrderNo}";
    var stockCostInfo = await _http.GetAndParseAsync<StockCostInfoViewModel>(url);
    return Ok(stockCostInfo);
  }

  public async Task<IActionResult> GetStockCostInfoBatch([FromBody] List<GetStockCostInfoViewModel> models, [FromQuery] string? selectedVendor)
  {
    var url = $"StockItems/GetStockCostInfoBatch?selectedVendor={selectedVendor}";
    var stockCostInfos = await _http.PostAndParseAsync<IEnumerable<StockCostInfoViewModel>>(url, models);
    return Ok(stockCostInfos);
  }

  public async Task<IActionResult> GetDepartments()
  {
    string url = "Categories/Departments";
    var departments = await _http.GetAndParseAsync<IEnumerable<DepartmentViewModel>>(url);
    return Ok(departments);
  }

  public async Task<IActionResult> GetSubDepartments(string department)
  {
    var url = $"Categories/Subdepartments?department={department}";
    var subDepartments = await _http.GetAndParseAsync<IEnumerable<SubDepartmentViewModel>>(url);
    return Ok(subDepartments);
  }

  public async Task<IActionResult> GetSubDetails(string department, string subDepartment)
  {
    var url = $"Categories/Subdetail?department={department}&subdepartment={subDepartment}";
    var subDetails = await _http.GetAndParseAsync<IEnumerable<SubDetailViewModel>>(url);
    return Ok(subDetails);
  }

  public async Task<IActionResult> GetVendors()
  {
    var url = "Vendors/All";
    var vendors = await _http.GetAndParseAsync<IEnumerable<VendorViewModel>>(url);
    return Ok(vendors);
  }

  public async Task<IActionResult> SaleExistsForStockCode(string stockCode)
  {
    var url = $"Product/SaleExistsForStockCode/{stockCode}";
    bool exist = await _http.GetAndParseAsync<bool>(url);
    return Ok(exist);
  }

  public async Task<IActionResult> UpdateStockItemActiveStatus(string stockCode, bool flag)
  {
    var url = $"Stockitems/UpdateStockItemActiveStatus?stockCode={stockCode}&flag={flag}";
    await _http.PostAsync(url, null);
    return NoContent();
  }

  public async Task<IActionResult> MakeStockItemKit(string stockCode, string stockDescription)
  {
    KitItemViewModel kitItemViewModel = new();
    kitItemViewModel.StockCode = stockCode;
    kitItemViewModel.StockDescription = stockDescription;

    string url = "Stockitems/MakeStockItemKit";
    await _http.PostAsync(url, kitItemViewModel);
    return NoContent();
  }

  public async Task<IActionResult> RemoveStockItemKit(string stockCode)
  {
    var url = "Stockitems/RemoveStockItemKit";
    await _http.PostAsync(url, stockCode);
    return NoContent();
  }

  public async Task<IActionResult> DeleteProduct(string stockCode)
  {
    if (stockCode == null) return BadRequest();

    bool batchDelete = false;
    if (!_config.IsBackOffice)
    {
      batchDelete = _policyService.GetPolicy(PolicyName.BATCH_DEL);
    }
    bool useTaxExempt = _policyService.GetPolicy(PolicyName.TAX_EXEMPT);
    var userCode = _contextAccessor.HttpContext?.Session?.GetString("UserCode");

    var url = "Product/DeleteProduct";
    await _http.PostAsync(url, new { StockCode = stockCode, UserCode = userCode, BatchDelete = batchDelete, UseTaxExempt = useTaxExempt });
    return NoContent();
  }

  public async Task<IActionResult> GetPackingInfo(string stockCode)
  {
    var url = $"StockItems/GetPackingInfo?stockCode={stockCode}";
    var packing = await _http.GetAndParseAsync<PackingInfoViewModel>(url);
    return Ok(packing);
  }

  public async Task<IActionResult> GetPackingInfoBatch([FromBody] List<string> stockCodes)
  {
    var url = "StockItems/GetPackingInfoBatch";
    var stockCostInfos = await _http.PostAndParseAsync<IEnumerable<PackingInfoViewModel>>(url, stockCodes);
    return Ok(stockCostInfos);
  }

  public async Task<IActionResult> GetStockItemByCode(string stockCode)
  {
    var url = $"Stockitems/GetStockItemByCode/{stockCode}";
    var product = await _http.GetAndParseAsync<ProductViewModel>(url);
    return Ok(product);
  }

  public async Task<IActionResult> GetStockItemsList()
  {
    const string url = "StockItems/GetAllStockItemsList";
    var stockItemsList = await _http.GetAndParseAsync<IEnumerable<StockItemViewModel>>(url);
    return Ok(stockItemsList);
  }
}
