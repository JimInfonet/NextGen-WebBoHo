namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class ProductClassificationController : Controller
{
  private readonly IHttpService _http;
  private readonly IPolicyService _policyService;

  public ProductClassificationController(IHttpService http, IPolicyService policyService)
  {
    _http = http;
    _policyService = policyService;
  }

  public IActionResult Index()
  {
    ViewData["EodGroupPolicy"] = _policyService.GetPolicy(PolicyName.EOD_GROUP);
    ViewData["TaxExemptPolicy"] = _policyService.GetPolicy(PolicyName.TAX_EXEMPT);
    ViewData["TaxExemptByRatePolicy"] = _policyService.GetPolicy(PolicyName.TE_ByRate) ? "Yes" : "No";
    ViewData["TaxExemptTypePolicy"] = _policyService.GetPolicy(PolicyName.TE_Type);
    ViewData["UserCode"] = HttpContext.Session.GetString("UserCode");

    return View();
  }

  public async Task<IActionResult> CheckGroupIdExists(string eodGroupId)
  {
    string url = $"EODGroup/CheckGroupIdExists/{eodGroupId}";
    bool exist = await _http.GetAndParseAsync<bool>(url);
    return Ok(exist);
  }

  public async Task<IActionResult> GetGroups()
  {
    string url = "EODGroup/GetGroups";
    var eodGroups = await _http.GetAndParseAsync<IEnumerable<EodGroupViewModel>>(url);
    return Ok(eodGroups);
  }

  public async Task<IActionResult> AddEODGroup([FromBody] EodGroupViewModel model)
  {
    string url = "EODGroup/AddEODGroup";
    bool added = await _http.PostAndParseAsync<bool>(url, model);
    return Ok(added);
  }

  public async Task<IActionResult> CheckDepartmentExistsByEodGroupId(string eodGroupId)
  {
    string url = $"Categories/CheckDepartmentExistsByEodGroupId/{eodGroupId}";
    bool exist = await _http.GetAndParseAsync<bool>(url);
    return Ok(exist);
  }

  public async Task<IActionResult> DeleteEodGroupById(bool isDeptExistsForEodGroupId, string eodGroupId)
  {
    string url = $"EODGroup/DeleteEodGroupById/{isDeptExistsForEodGroupId}/{eodGroupId}";
    var deleted = await _http.PostAndParseAsync<bool>(url);
    return Ok(deleted);
  }

  public async Task<IActionResult> UpdateEODGroupByDept(string eodGroupId, string dept)
  {
    string url = $"EODGroup/UpdateEODGroupByDept/{eodGroupId}/{dept}";
    bool updated = await _http.PostAndParseAsync<bool>(url);
    return Ok(updated);
  }

  public async Task<IActionResult> UpdateEodDetailByDept(string eodDetail, string dept)
  {
    string url = $"Categories/UpdateEodDetailByDept/{eodDetail}/{dept}";
    bool updated = await _http.PostAndParseAsync<bool>(url);
    return Ok(updated);
  }

  public async Task<IActionResult> UpdateEodCountDetailByDept(string eodCountDetail, string dept)
  {
    string url = $"Categories/UpdateEodCountDetailByDept/{eodCountDetail}/{dept}";
    bool updated = await _http.PostAndParseAsync<bool>(url);
    return Ok(updated);
  }

  public async Task<IActionResult> CheckDepartmentIdExists(string departmentId)
  {
    string url = $"categories/CheckDepartmentIdExists/{departmentId}";
    bool exist = await _http.GetAndParseAsync<bool>(url);
    return Ok(exist);
  }

  public async Task<IActionResult> AddDepartment([FromBody] DepartmentViewModel model)
  {
    string url = "Categories/AddDepartment";
    bool added = await _http.PostAndParseAsync<bool>(url, model);
    return Ok(added);
  }

  public async Task<IActionResult> CheckDepartmentExistsInFuelDept(string departmentId)
  {
    string url = $"categories/CheckDepartmentExistsInFuelDept/{departmentId}";
    bool exist = await _http.GetAndParseAsync<bool>(url);
    return Ok(exist);
  }

  public async Task<IActionResult> GetCountOfProductsInDepartment(string departmentId)
  {
    string url = $"stockitems/GetCountOfProductsInDepartment/{departmentId}";
    int count = await _http.GetAndParseAsync<int>(url);
    return Ok(count);
  }

  public async Task<IActionResult> GetCountOfProductsInSubDepartment(string departmentId, string subDepartmentId)
  {
    string url = $"stockitems/GetCountOfProductsInSubDepartment/{departmentId}/{subDepartmentId}";
    int count = await _http.GetAndParseAsync<int>(url);
    return Ok(count);
  }

  public async Task<IActionResult> DeleteCategoryByDepartmentId(string departmentId)
  {
    string url = $"categories/DeleteCategoryByDepartmentId/{departmentId}";
    bool deleted = await _http.PostAndParseAsync<bool>(url, null);
    return Ok(deleted);
  }

  public async Task<IActionResult> GetStockItemsByDepartmentSubDepartmentAndDetail(DepartmentSubDepartmentAndDetailViewModel model)
  {
    string url = $"stockitems/GetStockItemsByDepartmentSubDepartmentAndDetail";
    var stockItems = await _http.PostAndParseAsync<IEnumerable<StockItemViewModel>>(url, model);
    return Ok(stockItems);
  }

  public async Task<IActionResult> GetProductTaxes()
  {
    string url = "TaxMaster/GetProductTaxes";
    var productTaxes = await _http.GetAndParseAsync<IEnumerable<ProductTaxViewModel>>(url);
    return Ok(productTaxes);
  }

  public async Task<IActionResult> InsertStockTaxByStockTaxData([FromBody] ProductClassificationTaxViewModel model)
  {
    IEnumerable<StockTaxViewModel> stockTaxes = model.StockCodes.SelectMany(stockCode =>
    model.TaxNameAndCodes.Select(taxNameAndCode => new StockTaxViewModel
    {
      StockCode = stockCode,
      TaxName = taxNameAndCode.TaxName,
      TaxCode = taxNameAndCode.TaxCode
    }));

    string url = "StockTax/InsertStockTaxByStockTaxData";
    bool inserted = await _http.PostAndParseAsync<bool>(url, stockTaxes);
    return Ok(inserted);
  }

  public async Task<IActionResult> DeleteStockTaxByStockTaxData([FromBody] ProductClassificationTaxViewModel model)
  {
    IEnumerable<StockTaxViewModel> stockTaxes = model.StockCodes.SelectMany(stockCode =>
    model.TaxNameAndCodes.Select(taxNameAndCode => new StockTaxViewModel
    {
      StockCode = stockCode,
      TaxName = taxNameAndCode.TaxName,
      TaxCode = taxNameAndCode.TaxCode
    }));

    string url = "StockTax/DeleteStockTaxByStockTaxData";
    bool deleted = await _http.PostAndParseAsync<bool>(url, stockTaxes);
    return Ok(deleted);
  }

  public async Task<IActionResult> InsertStockChargesByStockChargeData([FromBody] ProductClassificationChargesViewModel model)
  {
    IEnumerable<StockAssociationViewModel> stockAssociations = model.StockCodes.SelectMany(stockCode =>
    model.Charges.Select(charge => new StockAssociationViewModel
    {
      StockCode = stockCode,
      Code = charge,
    }));

    string url = "StockAssociation/InsertStockChargesByStockChargeData";
    bool inserted = await _http.PostAndParseAsync<bool>(url, stockAssociations);
    return Ok(inserted);
  }

  public async Task<IActionResult> DeleteStockChargesByStockChargeData([FromBody] ProductClassificationChargesViewModel model)
  {
    IEnumerable<StockAssociationViewModel> stockAssociations = model.StockCodes.SelectMany(stockCode =>
    model.Charges.Select(charge => new StockAssociationViewModel
    {
      StockCode = stockCode,
      Code = charge,
    }));

    string url = "StockAssociation/DeleteStockChargesByStockChargeData";
    var deleted = await _http.PostAndParseAsync<bool>(url, stockAssociations);
    return Ok(deleted);
  }

  public async Task<IActionResult> AddProductTaxExemptForDepartmentSubDepartmentAndDetail([FromBody] TaxExemptCategoryViewModel model)
  {
    string url = "TaxExempt/AddProductTaxExemptForDepartmentSubDepartmentAndDetail";
    bool added = await _http.PostAndParseAsync<bool>(url, model);
    return Ok(added);
  }

  public async Task<IActionResult> DeleteProductTaxExempt([FromBody] DepartmentSubDepartmentAndDetailViewModel model)
  {
    string url = "TaxExempt/DeleteProductTaxExempt";
    bool deleted = await _http.PostAndParseAsync<bool>(url, model);
    return Ok(deleted);
  }

  public async Task<IActionResult> AddProductDiscountCode([FromBody] DepartmentSubDepartmentAndDetailViewModel departmentSubDepartmentAndDetail, string productDiscountCode)
  {
    string url = $"stockitems/AddProductDiscountCode/{productDiscountCode}";
    bool added = await _http.PostAndParseAsync<bool>(url, departmentSubDepartmentAndDetail);
    return Ok(added);
  }

  public async Task<IActionResult> DeleteProductDiscountCode([FromBody] DepartmentSubDepartmentAndDetailViewModel model)
  {
    string url = "stockitems/DeleteProductDiscountCode";
    bool deleted = await _http.PostAndParseAsync<bool>(url, model);
    return Ok(deleted);
  }

  public async Task<IActionResult> CheckSubDepartmentIdExistsForDepartment(string departmentId, string subDepartmentId)
  {
    string url = $"SubDepartment/CheckSubDepartmentIdExistsForDepartment/{departmentId}/{subDepartmentId}";
    bool exist = await _http.GetAndParseAsync<bool>(url);
    return Ok(exist);
  }

  public async Task<IActionResult> AddSubDepartment([FromBody] SubDepartmentViewModel subDepartmentViewModel)
  {
    string url = "SubDepartment/AddSubDepartment";
    bool added = await _http.PostAndParseAsync<bool>(url, subDepartmentViewModel);
    return Ok(added);
  }

  public async Task<IActionResult> DeleteSubDepartment(string departmentId, string subDepartmentId)
  {
    string url = $"SubDepartment/DeleteSubDepartment/{departmentId}/{subDepartmentId}";
    bool deleted = await _http.PostAndParseAsync<bool>(url, null);
    return Ok(deleted);
  }

  public async Task<IActionResult> CheckDetailIdExistsForDepartmentAndSubDepartment(string departmentId, string subDepartmentId, string detailId)
  {
    string url = $"SubDetail/CheckDetailIdExistsForDepartmentAndSubDepartment/{departmentId}/{subDepartmentId}/{detailId}";
    bool exist = await _http.GetAndParseAsync<bool>(url);
    return Ok(exist);
  }

  public async Task<IActionResult> AddDetail([FromBody] SubDetailViewModel subDetailViewModel)
  {
    string url = "SubDetail/AddDetail";
    bool added = await _http.PostAndParseAsync<bool>(url, subDetailViewModel);
    return Ok(added);
  }

  public async Task<IActionResult> DeleteDetail(string departmentId, string subDepartmentId, string detailId)
  {
    string url = $"SubDetail/DeleteDetail/{departmentId}/{subDepartmentId}/{detailId}";
    bool deleted = await _http.PostAndParseAsync<bool>(url, null);
    return Ok(deleted);
  }

  public async Task<IActionResult> GetCountOfProductsInDetail(string departmentId, string subDepartmentId, string detailId)
  {
    string url = $"StockItems/GetCountOfProductsInDetail/{departmentId}/{subDepartmentId}/{detailId}";
    int count = await _http.GetAndParseAsync<int>(url);
    return Ok(count);
  }

  public async Task<IActionResult> IsProductsExist(string departmentId, string? subDepartmentId, string? detailId)
  {
    string url = $"stockitems/IsProductsExist?departmentId={departmentId}&subDepartmentId={subDepartmentId}&detailId={detailId}";
    bool exist = await _http.GetAndParseAsync<bool>(url);
    return Ok(exist);
  }
}
