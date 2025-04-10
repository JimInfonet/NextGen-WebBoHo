namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class PurchaseDetailController : Controller
{
  private readonly IHttpService _http;
  private readonly IPolicyService _policyService;
  private readonly IPurchasingAndReceivingService _purchasingAndReceivingService;

  public PurchaseDetailController(
    IHttpService http,
    IPolicyService policyService,
    IPurchasingAndReceivingService purchasingAndReceivingService)
  {
    _http = http;
    _policyService = policyService;
    _purchasingAndReceivingService = purchasingAndReceivingService;
  }

  public async Task<IActionResult> Index(string purchaseOrderNumber)
  {
    PurchaseDetailPoliciesViewModel policies = _GetPolicies();
    ViewData["SelectedPoNumber"] = purchaseOrderNumber;
    ViewData["POPDFilePolicy"] = policies.POPDFilePolicy;
    ViewData["POChargesPolicy"] = policies.POChargesPolicy;

    string url = @$"PurchaseDetail/GetVendorPartsFromVendorPartStockMstAndStockBr";
    ViewBag.VendorParts = await _http.GetAndParseAsync<IEnumerable<VendorPartDtoViewModel>>(url);

    return View();
  }

  public async Task<IActionResult> GetPurchaseHeadersByPurchaseNo(string purchaseNo)
  {
    string url = $"PurchaseHeader/GetPurchaseHeadersByPurchaseNo/{purchaseNo}";
    var purchaseHeader = await _http.GetAndParseAsync<PurchaseHeaderViewModel?>(url);
    return Ok(purchaseHeader);
  }

  public async Task<IActionResult> GetVendorByVendorCode(string code)
  {
    string url = $"vendors/GetVendorByVendorCode/{code}";
    var vendor = await _http.GetAndParseAsync<VendorViewModel?>(url);
    return Ok(vendor);
  }

  public async Task<IActionResult> GetCompany()
  {
    string url = "CompanySetup/GetCompany";
    var company = await _http.GetAndParseAsync<CompanyViewModel?>(url);
    return Ok(company);
  }

  public IActionResult GetPolicies()
  {
    PurchaseDetailPoliciesViewModel policies = _GetPolicies();
    return Ok(policies);
  }

  public async Task<IActionResult> GetPurchaseDetailsByPurchaseNumber(string purchaseNo)
  {
    string url = $"PurchaseDetail/GetPurchaseDetailsByPurchaseNumber/{purchaseNo}";
    var purchaseDetails = await _http.GetAndParseAsync<IEnumerable<PurchaseDetailViewModel>?>(url);
    return Ok(purchaseDetails);
  }

  public async Task<IActionResult> UpdatePurchaseDetail([FromBody] PurchaseDetailUpdateViewModel model)
  {
    model.Policies = _purchasingAndReceivingService.GetPolicies();

    string url = "PurchaseDetail/UpdatePurchaseDetail";
    await _http.PostAsync(url, model);
    return NoContent();
  }

  public async Task<IActionResult> DeletePurchaseDetail([FromBody] DeletePurchaseDetailParameterRecordViewModel model)
  {
    string url = "PurchaseDetail/DeletePurchaseDetail";
    await _http.PostAsync(url, model);
    return NoContent();
  }

  public async Task<IActionResult> UpdatePurchaseHeader([FromBody] PurchaseHeaderUpdateViewModel model)
  {
    PurchaseDetailPoliciesViewModel policies = _GetPolicies();
    model.IsEditTotals = (bool)policies.UserEditPoPolicy;

    string url = "PurchaseHeader/UpdatePurchaseHeader";
    await _http.PostAsync(url, model);
    return NoContent();
  }

  public async Task<IActionResult> GetQuantDecPolicyByStockCode(string stockCode)
  {
    string url = $"StockItems/GetStockItemByCode/{stockCode}";
    var stockItem = await _http.GetAndParseAsync<ProductViewModel>(url);

    int quantDec = _policyService.GetPolicy(PolicyName.QUANT_DEC, stockItem);
    return Ok(quantDec);
  }

  public async Task<IActionResult> GetCostDecPolicyByStockCode(string stockCode)
  {
    string url = $"StockItems/GetStockItemByCode/{stockCode}";
    var stockItem = await _http.GetAndParseAsync<ProductViewModel>(url);

    int costDec = _policyService.GetPolicy(PolicyName.COST_DEC, stockItem);
    return Ok(costDec);
  }

  public async Task<IActionResult> GetVendorUnitCostPercentage([FromBody] PurchaseDetailUpdateViewModel model)
  {
    model.Policies = _purchasingAndReceivingService.GetPolicies();

    string url = "PurchaseDetail/GetVendorUnitCostPercentage";
    var vendorUnitCostPercentage = await _http.PostAndParseAsync<VendorUnitCostPercentageRecordViewModel>(url, model);
    return Ok(vendorUnitCostPercentage);
  }

  public async Task<IActionResult> UpdatePurchaseHeaderSurChargeDiscount([FromBody] PurchaseHeaderViewModel model)
  {
    string url = "PurchaseHeader/UpdatePurchaseHeaderSurChargeDiscount";
    await _http.PostAsync(url, model);
    return NoContent();
  }

  public async Task<IActionResult> GetVendorPartsFromVendorPartStockMstAndStockBr()
  {
    string url = @$"PurchaseDetail/GetVendorPartsFromVendorPartStockMstAndStockBr";
    var vendorParts = await _http.GetAndParseAsync<IEnumerable<VendorPartDtoViewModel>>(url);
    return Ok(vendorParts);
  }

  public async Task<IActionResult> GetVendorPartFromVendorPartStockBrByPartNumber(string partNo)
  {
    string url = $"PurchaseDetail/GetVendorPartFromVendorPartStockBrByPartNumber/{partNo}";
    var vendorPart = await _http.GetAndParseAsync<VendorPartDtoViewModel>(url);
    return Ok(vendorPart);
  }

  public async Task<IActionResult> UpdateStockBranchReorderAndMaximumQuantity([FromBody] StockBranchViewModel model)
  {
    string url = @$"PurchaseDetail/UpdateStockBranchReorderAndMaximumQuantity";
    await _http.PostAsync(url, model);
    return NoContent();
  }

  public async Task<IActionResult> UpdatePurchaseDetailGSTByPurchaseNO([FromBody] UpdatePurchaseDetailsViewModel model)
  {
    string url = "PurchaseDetail/UpdatePurchaseDetailGSTByPurchaseNO";
    await _http.PostAsync(url, model.PurchaseDetails);
    return NoContent();
  }

  public async Task<IActionResult> AddTaxToPurchaseDetail([FromBody] AddTaxToPurchaseDetailParameterDtoViewModel model)
  {
    PurchasingAndReceivingPoliciesViewModel policies = _purchasingAndReceivingService.GetPolicies();
    model.ExptTaxPolicy = policies.ExptTaxPolicy;

    string url = @$"PurchaseDetail/AddTaxToPurchaseDetail";
    await _http.PostAsync(url, model);
    return NoContent();
  }

  private PurchaseDetailPoliciesViewModel _GetPolicies()
  {
    PurchaseDetailPoliciesViewModel policies = new()
    {
      POPDFilePolicy = _policyService.GetPolicy(PolicyName.POPD_FILE),
      POChargesPolicy = _policyService.GetPolicy(PolicyName.PO_CHG),
      UserEditPoPolicy = _policyService.GetPolicy(PolicyName.U_EDITPO)
    };

    return policies;
  }
}
