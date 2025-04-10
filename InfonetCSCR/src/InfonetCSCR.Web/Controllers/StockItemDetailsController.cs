namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class StockItemDetailsController : Controller
{
  private readonly IHttpService _http;
  private readonly IApplicationConfig _config;
  private readonly IPolicyService _policyService;
  private readonly IHttpContextAccessor _contextAccessor;

  static bool _isAddTaxExempt = false;

  public StockItemDetailsController(
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

  public async Task<IActionResult> IndexAsync(string stockCode)
  {
    string TaxExemptPolicy = _policyService.GetPolicy(PolicyName.TAX_EXEMPT) ? "Yes" : "No";
    string TaxExemptByRatePolicy = _policyService.GetPolicy(PolicyName.TE_ByRate) ? "Yes" : "No";
    string TaxExemptTypePolicy = _policyService.GetPolicy(PolicyName.TE_Type);

    ViewData["StockCode"] = stockCode;
    ViewData["IsBackOffice"] = _config.IsBackOffice;
    ViewData["TaxExemptPolicy"] = TaxExemptPolicy;
    ViewData["TaxExemptByRatePolicy"] = TaxExemptByRatePolicy;
    ViewData["TaxExemptTypePolicy"] = TaxExemptTypePolicy;

    string url = $"TaxExempt/GetProductTaxExemptByProductKey/{stockCode}";
    var productTaxExempt = await _http.GetAndParseAsync<ProductTaxExemptViewModel>(url);

    if (productTaxExempt != null) ViewData["Description"] = productTaxExempt.Description;
    _isAddTaxExempt = productTaxExempt != null && productTaxExempt.CategoryFK == 0;
    ViewData["IsAddMode"] = _isAddTaxExempt;

    return View(productTaxExempt);
  }

  public IActionResult GetStockItemDetailsPolicy()
  {
    var policies = new StockItemDetailsPolicyViewModel { UserAccessStockCode = true };
    var user = new UserViewModel
    {
      Code = _contextAccessor.HttpContext?.Session.GetString("UserCode") ?? string.Empty
    };

    if (_config.IsBackOffice)
    {
      policies.UserAccessStockCode = _policyService.GetPolicy(PolicyName.U_STOCK, user);
    }

    return Ok(policies);
  }

  [HttpPost]
  public async Task<IActionResult> GetStockItemDetails([FromBody] StockItemDetailsCommonViewModel model)
  {
    string url = "StockItemDetails/GetStockItemDetails";
    var stockItem = await _http.PostAndParseAsync<StockItemDetailsViewModel>(url, model);
    return Ok(stockItem);
  }

  [HttpPost]
  public async Task<IActionResult> GetAssocNotInStockCodeAc([FromBody] StockItemDetailsCommonViewModel model)
  {
    string url = "StockItemDetails/GetAssocNotInStockCodeAc";
    var assocs = await _http.PostAndParseAsync<IEnumerable<AssocViewModel>>(url, model);
    return Ok(assocs);
  }

  [HttpPost]
  public async Task<IActionResult> GetTaxesAndCode([FromBody] StockItemDetailsCommonViewModel model)
  {
    string url = "StockItemDetails/GetTaxesAndCode";
    var taxes = await _http.PostAndParseAsync<IEnumerable<TaxNameAndCodeViewModel>>(url, model);
    return Ok(taxes);
  }

  [HttpPost]
  public async Task<IActionResult> DeleteCharge([FromBody] ChargesViewModel model)
  {
    string url = "StockItemDetails/DeleteCharge";
    var assocs = await _http.PostAndParseAsync<IEnumerable<AssocViewModel>>(url, model);
    return Ok(assocs);
  }

  [HttpPost]
  public async Task<IActionResult> InsertCharge([FromBody] ChargesViewModel model)
  {
    string url = "StockItemDetails/InsertCharge";
    var assocs = await _http.PostAndParseAsync<IEnumerable<AssocViewModel>>(url, model);
    return Ok(assocs);
  }

  public async Task<IActionResult?> GetPLUCodesByStockCode(string stockCode)
  {
    string url = $"Product/GetPLUCodesByPLUPrime/{stockCode}";
    var pluMasts = await _http.GetAndParseAsync<IEnumerable<PLUMastViewModel>>(url);
    return Ok(pluMasts);
  }

  public async Task<IActionResult> AddPLUCodeToPLUMast(string pluPrim, string pluCode)
  {
    PLUMastViewModel pluMastViewModel = new()
    {
      PLUCode = pluCode,
      PLUPrim = pluPrim,
      PLUType = "S" // Hard coded in old codebase
    };

    string url = "Product/AddPLUCodeToPLUMast";
    await _http.PostAsync(url, pluMastViewModel);
    return NoContent();
  }

  public async Task<IActionResult> DeletePLUMastByPLUCode(string pluCode)
  {
    string url = "Product/DeletePLUMastByPLUCode";
    await _http.PostAsync(url, pluCode);
    return NoContent();
  }

  [HttpPost]
  public async Task<IActionResult> InsertTaxes([FromBody] TaxViewModel model)
  {
    string url = "StockItemDetails/InsertTaxes";
    var taxes = await _http.PostAndParseAsync<IEnumerable<TaxViewModel>>(url, model);
    return Ok(taxes);
  }

  [HttpPost]
  public async Task<IActionResult> DeleteTaxes([FromBody] TaxViewModel model)
  {
    string url = "StockItemDetails/DeleteTaxes";
    var taxes = await _http.PostAndParseAsync<IEnumerable<TaxViewModel>>(url, model);
    return Ok(taxes);
  }

  [HttpPost]
  public async Task<IActionResult> InsertStockAvailabilityByDay([FromBody] StockCodeAndAvailabilityByDayViewModel model)
  {
    string url = "StockItemDetails/InsertStockAvailabilityByDay";
    var stockAvailabilities = await _http.PostAndParseAsync<List<StockAvailabilityByDayViewModel>>(url, model);
    return Ok(stockAvailabilities);
  }

  public async Task<JsonResult?> GetDataFromAPI(string url)
  {
    var response = await _http.GetAsync(url);
    var jsonResult = Json(await response.Content.ReadAsStringAsync());
    return jsonResult;
  }

  public async Task<JsonResult?> GetAllTaxExemptCategory()
  {
    return await GetDataFromAPI("TaxExempt/GetAllTaxExemptCategory");
  }

  public async Task<JsonResult?> GetAllTaxExemptVendorsByStockCode(string stockCode)
  {
    return await GetDataFromAPI($"TaxExempt/GetAllTaxExemptVendorsByStockCode/{stockCode}");
  }

  public async Task<JsonResult?> GetAllTaxExemptRate()
  {
    return await GetDataFromAPI("TaxExempt/GetAllTaxExemptRate");
  }

  public async Task<IActionResult> AddProductTaxExempt(ProductTaxExemptViewModel model)
  {
    var userCode = _contextAccessor.HttpContext?.Session?.GetString("UserCode");
    model.EmplID = userCode ?? "";

    string url = "TaxExempt/AddProductTaxExempt";
    await _http.PostAsync(url, model);
    return RedirectToAction("Index", new { stockCode = model.ProductKey });
  }

  public async Task<IActionResult> UpdateProductTaxExempt(ProductTaxExemptViewModel model)
  {
    string url = "TaxExempt/UpdateProductTaxExempt";
    await _http.PostAsync(url, model);
    return RedirectToAction("Index", new { stockCode = model.ProductKey });
  }

  public async Task<IActionResult> DeleteProductTaxExemptByProductKey(string productKey)
  {
    string url = "TaxExempt/DeleteProductTaxExemptByProductKey";
    await _http.PostAsync(url, productKey);
    return NoContent();
  }

  public async Task<IActionResult> TaxExemptFromSubmit(ProductTaxExemptViewModel productTaxExemptViewModel)
  {
    if (_isAddTaxExempt) return await AddProductTaxExempt(productTaxExemptViewModel);
    return await UpdateProductTaxExempt(productTaxExemptViewModel);
  }

  public async Task<IActionResult> CalculateTaxExemptPrice(string stockCode, int taxCode, int taxExemptCategory, int quantityPerPkg)
  {
    bool isUseFuelPolicy = _policyService.GetPolicy(PolicyName.USE_FUEL);
    string url = $"TaxExempt/CalculateTaxExemptPrice?stockCode={stockCode}&taxCode={taxCode}" +
                 $"&taxExemptCategory={taxExemptCategory}&quantityPerPkg={quantityPerPkg}&isUseFuelPolicy={isUseFuelPolicy}";

    var taxExemptPrice = await _http.GetAndParseAsync<string>(url);
    return Ok(taxExemptPrice);
  }
}
