namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class StockItemFormController : Controller
{
  private readonly IHttpService _http;
  private readonly IApplicationConfig _config;
  private readonly IPolicyService _policyService;

    static bool _isUpdateProduct;
    static string? _baseProductStockCode;
    static int? _packing = 1;

    public StockItemFormController(IHttpService http, IApplicationConfig config, IPolicyService policyService)
    {
      _http = http;
      _config = config;
      _policyService = policyService;
    }

    public async Task<IActionResult> Index(string? stockCodeValue, bool? isUpdateProduct, bool? isCopyProduct)
    {
      _isUpdateProduct = isUpdateProduct ?? false;
      if (isCopyProduct == true) _baseProductStockCode = stockCodeValue;

      ViewData["isUpdateProduct"] = _isUpdateProduct;
      ViewData["isAddProduct"] = !_isUpdateProduct;
      ViewData["IsProductTaxExempt"] = (stockCodeValue != null) ? await IsProductTaxExempt(stockCodeValue) : false;
      ViewData["ProductIsFuel"] = (stockCodeValue != null) ? await ProductIsFuel(stockCodeValue) : false;
      ViewData["isTransferred"] = false;
      ViewData["FuelRebatePolicy"] = _policyService.GetPolicy(PolicyName.FuelRebate);
      ViewData["UseFuelPolicy"] = _policyService.GetPolicy(PolicyName.USE_FUEL);
      ViewData["TaxRebatePolicy"] = _policyService.GetPolicy(PolicyName.TAX_REBATE);
      ViewData["TaxExemptGAPolicy"] = _policyService.GetPolicy(PolicyName.TAX_EXEMPT_GA);
      ViewData["SupportWarehousePolicy"] = _policyService.GetPolicy(PolicyName.SUPPORT_WAREHOUSE);
      ViewData["TaxExemptPolicy"] = _policyService.GetPolicy(PolicyName.TAX_EXEMPT);
      ViewData["ExemptTypePolicy"] = _policyService.GetPolicy(PolicyName.TE_Type);
      ViewData["TrackTEInvPolicy"] = _policyService.GetPolicy(PolicyName.TRACK_TEINV);
      ViewData["PackCost"] = _policyService.GetPolicy(PolicyName.PACKCOST);
      ViewData["ElgLoyPolicy"] = _policyService.GetPolicy(PolicyName.ELG_LOY);
      ViewData["ExpandTo13DigitsPolicy"] = _policyService.GetPolicy(PolicyName.ExpandTo13Dg);

      if (stockCodeValue == null) return View();

      string url = $"Product/GetProduct/{stockCodeValue}";
      var product = await _http.GetAndParseAsync<ProductViewModel?>(url);

      if (isCopyProduct == true && product != null)
      {
        product.StockCode = "";
        product.Description = "";
      }
      _packing = product?.Packing;
      ViewData["isTransferred"] = product?.Transfer;

      return View(product);
    }

    public async Task<IActionResult> SaveProduct(ProductViewModel productViewModel)
    {
      if (!ModelState.IsValid) return View("Index", productViewModel);

      string url = (_isUpdateProduct) ?
        "Product/UpdateProduct" :
        _baseProductStockCode != null ?
        $"Product/CopyProduct/{_baseProductStockCode}" :
        "Product/SaveProduct";

      productViewModel.Packing ??= _packing;
      productViewModel.Packing = productViewModel.Packing <= 0 ? 1 : productViewModel.Packing;

      productViewModel.PriceType = "R"; // Hard coded in old code base

      try
      {
        await _http.PostAsync(url, productViewModel);
      }
      catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
      {
        ViewData["StockCodeAlreadyExists"] = $"{productViewModel.StockCode}";
        ViewData["isAddProduct"] = true;
        ViewData["isUpdateProduct"] = false;
        return View("Index", productViewModel);
      }

      return RedirectToAction("Index", new { stockCodeValue = productViewModel.StockCode, isUpdateProduct = true });
    }

    public async Task<JsonResult?> GetDataFromAPI(string url)
    {
      var response = await _http.GetAsync(url);
      JsonResult jsonResult = Json(await response.Content.ReadAsStringAsync());
      return jsonResult;
    }

    public async Task<JsonResult?> GetAllFamilies()
    {
      return await GetDataFromAPI("Family/GetAllFamily");
    }

    public async Task<JsonResult?> GetAllBrands()
    {
      return await GetDataFromAPI("Brand/GetAllBrands");
    }

    public async Task<JsonResult?> GetAllGenerics()
    {
      return await GetDataFromAPI("Generic/GetAllGenerics");
    }

    public async Task<JsonResult?> GetAllCharacteristics()
    {
      return await GetDataFromAPI("Characteristic/GetAllCharacteristics");
    }

    public async Task<JsonResult?> GetAllUnitOfMeasures()
    {
      return await GetDataFromAPI("UnitOfMeasure/GetAllUnitOfMeasures");
    }

    public async Task<JsonResult?> GetAllOrderUnits()
    {
      return await GetDataFromAPI("OrderUnit/GetAllOrderUnits");
    }

    public async Task<JsonResult?> GetWarehouse()
    {
      if (!_config.IsBackOffice) return await GetDataFromAPI("StockDescriptionElements/Warehouse");
      return Json(null);
    }

    public async Task<JsonResult?> GetGrade(string? stockCode)
    {
      return await GetDataFromAPI($"StockDescriptionElements/Grade?stockCode={stockCode}");
    }

    public async Task<JsonResult?> GetProductTaxExempt(string stockCode)
    {
      return await GetDataFromAPI($"StockDescriptionElements/ProductTaxExempt?stockCode={stockCode}");
    }

    public async Task<IActionResult> UpdateStockItemTransfer(string stockCode)
    {
      string url = $"stockitems/UpdateStockItemTransfer?stockCode={stockCode}";
      await _http.GetAsync(url);
      return NoContent();
    }

    public async Task<IActionResult> ProductIsFuel(string? stockCode)
    {
      if (_policyService.GetPolicy(PolicyName.USE_FUEL))
      {
        if (_config.IsBackOffice)
        {
          var gradeJson = await GetGrade(stockCode);
          string grade = gradeJson.Value.ToString();
          if (grade != "[]") return Ok(true);
        }
      }
      return Ok(false);
    }

  public async Task<IActionResult> IsProductTaxExempt(string? stockCode)
  {
    if (_policyService.GetPolicy(PolicyName.USE_FUEL))
    {
      if (_config.IsBackOffice)
      {
        var productTaxExemptJson = await GetProductTaxExempt(stockCode);
        string productTaxExempt = productTaxExemptJson.Value.ToString();
        if (productTaxExempt != "") return Ok(true);
      }
    }
    return Ok(false);
  }
}
