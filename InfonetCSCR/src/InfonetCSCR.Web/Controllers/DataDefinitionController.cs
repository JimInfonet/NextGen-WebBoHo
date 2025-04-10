namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class DataDefinitionController : Controller
{
  private readonly IHttpService _http;

  public DataDefinitionController(IHttpService http)
  {
    _http = http;
  }

  public IActionResult Index()
  {
    ViewData["UserCode"] = HttpContext.Session.GetString("UserCode");
    return View();
  }

  public async Task<IActionResult> GetStoresData()
  {
    string url = "DataDefinition/GetStoresData";
    var stores = await _http.GetAndParseAsync<List<StoreViewModel>>(url);
    return Ok(stores);
  }

  public async Task<IActionResult> GetRegionsData()
  {
    string url = "DataDefinition/GetRegionsData";
    var regions = await _http.GetAndParseAsync<List<RegionViewModel>>(url);
    return Ok(regions);
  }

  public async Task<IActionResult> GetPriceGroupsData()
  {
    string url = "DataDefinition/GetPriceGroupsData";
    var priceGroups = await _http.GetAndParseAsync<List<PriceGroupViewModel>>(url);
    return Ok(priceGroups);
  }

  public async Task<IActionResult> GetVendorsData()
  {
    string url = "DataDefinition/GetVendorsData";
    var vendors = await _http.GetAndParseAsync<List<VendorViewModel>>(url);
    return Ok(vendors);
  }

  public async Task<IActionResult> GetDataDefinitionsByCriteria(GetDataDefinitionsByCriteriaDtoViewModel model)
  {
    string url = $"DataDefinition/GetDataDefinitionsByCriteria?productId={model.ProductId}&storeId={model.StoreId}&defType={model.DefType}";
    if (!string.IsNullOrWhiteSpace(model.VendorId)) url += $"&vendorId={model.VendorId}";

    var dataDefinitions = await _http.GetAndParseAsync<List<DataDefinitionViewModel>>(url);
    return Ok(dataDefinitions);
  }

  public async Task<IActionResult> GetTaxMastersData()
  {
    string url = "DataDefinition/GetTaxMastersData";
    var taxMasters = await _http.GetAndParseAsync<List<TaxMasterViewModel>>(url);
    return Ok(taxMasters);
  }

  public async Task<IActionResult> GetStockTaxByStockCodeAndStore(string stockCode, string storeId)
  {
    string url = $"DataDefinition/GetStockTaxByStockCodeAndStore?stockCode={stockCode}&storeId={storeId}";
    var stockTaxes = await _http.GetAndParseAsync<List<TaxMasterViewModel>>(url);
    return Ok(stockTaxes);
  }

  public async Task<IActionResult> GetAssociations()
  {
    string url = "DataDefinition/GetAssociations";
    var associations = await _http.GetAndParseAsync<List<AssocViewModel>>(url);
    return Ok(associations);
  }

  public async Task<IActionResult> GetStockAssociationsByStockCodeAndStore(string stockCode, string storeId)
  {
    string url = $"DataDefinition/GetStockAssociationsByStockCodeAndStore?stockCode={stockCode}&storeId={storeId}";
    var associations = await _http.GetAndParseAsync<List<AssocViewModel>>(url);
    return Ok(associations);
  }

  public async Task<IActionResult> GetStoresByStoreIds([FromBody] IEnumerable<string> storeIds)
  {
    string url = "DataDefinition/GetStoresByStoreIds";
    var stores = await _http.PostAndParseAsync<List<StoreViewModel>>(url, storeIds);
    return Ok(stores);
  }

  public async Task<IActionResult> GetStoresByVendorIds([FromBody] IEnumerable<string> vendorIds)
  {
    string url = "DataDefinition/GetStoresByVendorIds";
    var stores = await _http.PostAndParseAsync<List<StoreViewModel>>(url, vendorIds);
    return Ok(stores);
  }

  public async Task<IActionResult> GetStoresByPriceGroupIds([FromBody] IEnumerable<string> priceGroupIds)
  {
    string url = "DataDefinition/GetStoresByPriceGroupIds";
    var stores = await _http.PostAndParseAsync<List<StoreViewModel>>(url, priceGroupIds);
    return Ok(stores);
  }

  public async Task<IActionResult> GetStoresByRegionId([FromBody] IEnumerable<string> regionIds)
  {
    string url = "DataDefinition/GetStoresByRegionIds";
    var stores = await _http.PostAndParseAsync<List<StoreViewModel>>(url, regionIds);
    return Ok(stores);
  }

  public async Task<IActionResult> GetStockTransfersByStoreIds([FromBody] ProductSearchParameterViewModel productSearchParameterViewModel)
  {
    string url = "DataDefinition/GetStockTransfersByStoreIds";
    var stockTransfers = await _http.PostAndParseAsync<StockTransferFilterViewModel>(url, productSearchParameterViewModel);
    return Ok(new
    {
      iTotalRecords = stockTransfers!.TotalItems,          //Do not Rename variable name
      iTotalDisplayRecords = stockTransfers.TotalItems,   //Do not Rename variable name
      Data = stockTransfers.stockTransferDtos             //Do not Rename variable name
    });
  }

  public async Task<IActionResult> GetStockTransfersByPriceGroupIds([FromBody] ProductSearchParameterViewModel productSearchParameterViewModel)
  {
    string url = "DataDefinition/GetStockTransfersByPriceGroupIds";
    var stockTransfers = await _http.PostAndParseAsync<StockTransferFilterViewModel>(url, productSearchParameterViewModel);
    return Ok(new
    {
      iTotalRecords = stockTransfers!.TotalItems,          //Do not Rename variable name
      iTotalDisplayRecords = stockTransfers.TotalItems,   //Do not Rename variable name
      Data = stockTransfers.stockTransferDtos             //Do not Rename variable name
    });
  }

  public async Task<IActionResult> GetStockTransfersByRegionIds([FromBody] ProductSearchParameterViewModel productSearchParameterViewModel)
  {
    string url = "DataDefinition/GetStockTransfersByRegionIds";
    var stockTransfers = await _http.PostAndParseAsync<StockTransferFilterViewModel>(url, productSearchParameterViewModel);
    return Ok(new
    {
      iTotalRecords = stockTransfers!.TotalItems,          //Do not Rename variable name
      iTotalDisplayRecords = stockTransfers.TotalItems,   //Do not Rename variable name
      Data = stockTransfers.stockTransferDtos             //Do not Rename variable name
    });
  }

  public async Task<IActionResult> GetStockTransfersByVendorIds([FromBody] ProductSearchParameterViewModel productSearchParameterViewModel)
  {
    string url = "DataDefinition/GetStockTransfersByVendorIds";
    var stockTransfers = await _http.PostAndParseAsync<StockTransferFilterViewModel>(url, productSearchParameterViewModel);
    return Ok(new
    { 
      iTotalRecords = stockTransfers!.TotalItems,          //Do not Rename variable name
      iTotalDisplayRecords = stockTransfers.TotalItems,   //Do not Rename variable name
      Data = stockTransfers.stockTransferDtos             //Do not Rename variable name
    });
  }

  public async Task<IActionResult> SaveDataDefinition([FromBody] DataDefintionDtoViewModel dataDefintionDtoViewModel)
  {
    string url = "DataDefinition/SaveDataDefinition";
    bool saved = await _http.PostAndParseAsync<bool>(url, dataDefintionDtoViewModel);
    return Ok(saved);
  }
}
