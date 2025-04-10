namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class DataDefinitionController : ControllerBase
{

  private readonly IPolicyService _policyService;
  private readonly IDataDefinitionService _dataDefinitionService;
  private readonly ITaxMasterService _taxMasterService;
  private readonly IAssociationService _associationService;
  private readonly IStoreService _storeService;
  private readonly IStockTransferService _stockTransferService;

  public DataDefinitionController(
    IPolicyService policyService,
    IDataDefinitionService dataDefinitionService,
    ITaxMasterService taxMasterService,
    IAssociationService associationService,
    IStoreService storeService,
    IStockTransferService stockTransferService)
  {
    _policyService = policyService;
    _dataDefinitionService = dataDefinitionService;
    _taxMasterService = taxMasterService;
    _associationService = associationService;
    _storeService = storeService;
    _stockTransferService = stockTransferService;
  }

  // GET api/DataDefinition/GetStoresData
  [HttpGet]
  [Route("GetStoresData")]
  public async Task<IActionResult> GetStoresData()
  {
    IEnumerable<Store> stores = await _dataDefinitionService.GetStoresFromStockTransfer();
    return Ok(stores);
  }

  // GET api/DataDefinition/GetRegionsData
  [HttpGet]
  [Route("GetRegionsData")]
  public async Task<IActionResult> GetRegionsData()
  {
    IEnumerable<Region> regions = await _dataDefinitionService.GetRegionsFromStockTransfer();
    return Ok(regions);
  }

  // GET api/DataDefinition/GetPriceGroupsData
  [HttpGet]
  [Route("GetPriceGroupsData")]
  public async Task<IActionResult> GetPriceGroupsData()
  {
    IEnumerable<PriceGroup> priceGroups = await _dataDefinitionService.GetPriceGroupsFromStockTransfer();
    return Ok(priceGroups);
  }

  // GET api/DataDefinition/GetVendorsData
  [HttpGet]
  [Route("GetVendorsData")]
  public async Task<IActionResult> GetVendorsData()
  {
    IEnumerable<Vendor> vendors = await _dataDefinitionService.GetVendorsFromStockTransfer();
    return Ok(vendors);
  }

  // GET api/DataDefinition/GetDataDefinitionsByCriteria
  [HttpGet]
  [Route("GetDataDefinitionsByCriteria")]
  public async Task<IActionResult> GetDataDefinitionsByCriteria([FromQuery] GetDataDefinitionsByCriteriaDto model)
  {
    IEnumerable<DataDefinition> dataDefinitions = await _dataDefinitionService.GetDataDefinitionsByCriteria(model);
    return Ok(dataDefinitions);
  }

  // GET api/DataDefinition/GetTaxMastersData
  [HttpGet]
  [Route("GetTaxMastersData")]
  public async Task<IActionResult> GetTaxMastersData()
  {
    IEnumerable<TaxMasterDto> taxMasters = await _taxMasterService.GetTaxMasters();
    return Ok(taxMasters);
  }

  // GET api/DataDefinition/GetStockTaxByStockCodeAndStore
  [HttpGet]
  [Route("GetStockTaxByStockCodeAndStore")]
  public async Task<IActionResult> GetStockTaxByStockCodeAndStore(string stockCode, string storeId)
  {
    IEnumerable<TaxMasterDto> taxMasters = await _taxMasterService.GetStockTaxByStockCodeAndStore(stockCode, storeId);
    return Ok(taxMasters);
  }

  // GET api/DataDefinition/GetAssociations
  [HttpGet]
  [Route("GetAssociations")]
  public async Task<IActionResult> GetAssociations()
  {
    IEnumerable<Assoc> associations = await _associationService.GetAssociations();
    return Ok(associations);
  }

  // GET api/DataDefinition/GetStockAssociationsByStockCodeAndStore
  [HttpGet]
  [Route("GetStockAssociationsByStockCodeAndStore")]
  public async Task<IActionResult> GetStockAssociationsByStockCodeAndStore(string stockCode, string storeId)
  {
    IEnumerable<Assoc> associations = await _associationService.GetStockAssociationsByStockCodeAndStore(stockCode, storeId);
    return Ok(associations);
  }

  // POST api/DataDefinition/GetStoresByStoreIds
  [HttpPost]
  [Route("GetStoresByStoreIds")]
  public async Task<IActionResult> GetStoresByStoreIds(IEnumerable<string> storeIds)
  {
    IEnumerable<Store> stores = await _storeService.GetStoresDataByStoreIds(storeIds);
    return Ok(stores);
  }

  // POST api/DataDefinition/GetStoresByVendorIds
  [HttpPost]
  [Route("GetStoresByVendorIds")]
  public async Task<IActionResult> GetStoresByVendorIds(IEnumerable<string> vendorIds)
  {
    IEnumerable<Store> stores = await _storeService.GetStoresByVendorIds(vendorIds);
    return Ok(stores);
  }

  // POST api/DataDefinition/GetStoresByPriceGroupIds
  [HttpPost]
  [Route("GetStoresByPriceGroupIds")]
  public async Task<IActionResult> GetStoresByPriceGroupIds(IEnumerable<string> priceGroupIds)
  {
    IEnumerable<Store> stores = await _storeService.GetStoresByPriceGroupIds(priceGroupIds);
    return Ok(stores);
  }

  // POST api/DataDefinition/GetStoresByRegionIds
  [HttpPost]
  [Route("GetStoresByRegionIds")]
  public async Task<IActionResult> GetStoresByRegionIds(IEnumerable<string> regionIds)
  {
    IEnumerable<Store> stores = await _storeService.GetStoresByRegionIds(regionIds);
    return Ok(stores);
  }

  // POST api/DataDefinition/GetStockTransfersByStoreIds
  [HttpPost]
  [Route("GetStockTransfersByStoreIds")]
  public async Task<IActionResult> GetStockTransfersByStoreIds(ProductSearchParameterDto productSearchParameterDto)
  {
    StockTransferFilterDto stockTransfers = await _stockTransferService.GetStockTransfersByStoreIds(productSearchParameterDto);
    return Ok(stockTransfers);
  }

  // POST api/DataDefinition/GetStockTransfersByPriceGroupIds
  [HttpPost]
  [Route("GetStockTransfersByPriceGroupIds")]
  public async Task<IActionResult> GetStockTransfersByPriceGroupIds(ProductSearchParameterDto productSearchParameterDto)
  {
    StockTransferFilterDto stockTransfers = await _stockTransferService.GetStockTransfersByPriceGroupIds(productSearchParameterDto);
    return Ok(stockTransfers);
  }

  // POST api/DataDefinition/GetStockTransfersByRegionIds
  [HttpPost]
  [Route("GetStockTransfersByRegionIds")]
  public async Task<IActionResult> GetStockTransfersByRegionIds(ProductSearchParameterDto productSearchParameterDto)
  {
    StockTransferFilterDto stockTransfers = await _stockTransferService.GetStockTransfersByRegionIds(productSearchParameterDto);
    return Ok(stockTransfers);
  }

  // POST api/DataDefinition/GetStockTransfersByVendorIds
  [HttpPost]
  [Route("GetStockTransfersByVendorIds")]
  public async Task<IActionResult> GetStockTransfersByVendorIds(ProductSearchParameterDto productSearchParameterDto)
  {
    StockTransferFilterDto stockTransfers = await _stockTransferService.GetStockTransfersByVendorIds(productSearchParameterDto);
    return Ok(stockTransfers);
  }

  // POST api/DataDefinition/SaveDataDefinition
  [HttpPost]
  [Route("SaveDataDefinition")]
  public async Task<IActionResult> SaveDataDefinition(DataDefinitionDto dataDefinitionDto)
  {
      bool networkUpdate = false;
      if (dataDefinitionDto.PriceGroupIds?.Count() > 0)
      {
        networkUpdate = await _policyService.GetPolicyNetworkUpdateByPriceGroup();
      }
      else if (dataDefinitionDto.RegionIds?.Count() > 0)
      {
        networkUpdate = await _policyService.GetPolicyNetworkUpdateByRegion();
      }
      else
      {
        networkUpdate = await _policyService.GetPolicyNetworkUpdate();
      }

      if (networkUpdate == true)
      {
        dataDefinitionDto.IsAllStoreSelected = true;
      }

      bool result = await _dataDefinitionService.SaveDataDefinition(dataDefinitionDto);
      return Ok(true);
  }
}
