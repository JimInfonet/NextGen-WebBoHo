namespace InfonetCSCR.Api.Controllers;

[Route("api/StockItems")]
[ApiController]
[Authorize]

public class StockItemsController : ControllerBase
{
  private readonly IStockItemService _stockItemService;
  private readonly IStockBranchService _stockBranchService;
  private readonly IKitMasterService _kitMasterService;
  private readonly IKitItemService _kitItemService;

  public StockItemsController(
    IStockItemService stockItemService,
    IStockBranchService stockBranchService,
    IKitMasterService kitMasterService,
    IKitItemService kitItemService)
  {
    _stockItemService = stockItemService;
    _stockBranchService = stockBranchService;
    _kitMasterService = kitMasterService;
    _kitItemService = kitItemService;
  }

  // POST api/stockitems/GetStockItemsByFilter
  [HttpPost]
  [Route("GetStockItemsByFilter")]
  public async Task<IActionResult> GetStockItemsByFilter([FromBody] SearchCriteria criteria)
  {
    StockItemsFilterData stockItemsFilterData = await _stockItemService.GetStockItemsByFilter(criteria);
    return Ok(stockItemsFilterData);
  }

  // GET api/stockitems/GetStockCostInfo
  [HttpGet]
  [Route("GetStockCostInfo")]
  public async Task<IActionResult> GetStockCostInfo([FromQuery] GetStockCostInfoDto model)
  {
    StockCostInfo stockCost = await _stockItemService.GetStockCostInfo(model);
    return Ok(stockCost);
  }

  // GET api/stockitems/GetStockCostInfoBatch
  [HttpPost]
  [Route("GetStockCostInfoBatch")]
  public async Task<IActionResult> GetStockCostInfoBatch([FromBody] List<GetStockCostInfoDto> models, [FromQuery] string? selectedVendor = "")
  {
    IEnumerable<StockCostInfo> stockCosts = await _stockItemService.GetStockCostInfoBatch(models, selectedVendor ?? "");
    return Ok(stockCosts);
  }

  // POST api/stockitems/GetStockItemsByDeptSubDeptAndSubDetail
  [HttpPost]
  [Route("GetStockItemsByDeptSubDeptAndSubDetail")]
  public async Task<IActionResult> GetStockItemsByDeptSubDeptAndSubDetail([FromBody] GetStockItemsByDeptSubDeptAndSubDetailDto searchParams)
  {
    StockItemMinimalFilterDto stockItemFilterData;
    if (searchParams.Depts?.Count == 0 &&
      searchParams.SubDepts?.Count == 0 &&
      searchParams.SubDetails?.Count == 0 &&
      searchParams.IsShowAllProduct == false &&
      searchParams.IsRecentChanges == false)
    {
      IEnumerable<StockItemMinimal> stockItems = new StockItemMinimal[] { };
      return Ok(new StockItemMinimalFilterDto
      {
        StockItems = stockItems,
        TotalItems = 0
      });
    }

    stockItemFilterData = await _stockItemService.GetStockItemsByDeptSubDeptAndSubDetail(searchParams);
    return Ok(stockItemFilterData);
  }

  // POST api/stockitems/GetStockItemsByVendor
  [HttpPost]
  [Route("GetStockItemsByVendor")]
  public async Task<IActionResult> GetStockItemsByVendor([FromBody] GetStockItemsByVendorDto searchParams)
  {
    StockItemMinimalFilterDto stockItemFilterData = await _stockItemService.GetStockItemsByVendor(searchParams);
    return Ok(stockItemFilterData);
  }

  // POST api/stockitems/UpdateStockItemActiveStatus
  [HttpPost]
  [Route("UpdateStockItemActiveStatus")]
  public async Task<IActionResult> UpdateStockItemActiveStatus(string stockCode, bool flag)
  {
    bool isUpdatedStockItemActiveStatus = await _stockBranchService.UpdateStockItemActiveStatus(stockCode, flag);
    return Ok(isUpdatedStockItemActiveStatus);
  }

  // POST api/stockitems/MakeStockItemKit
  [HttpPost]
  [Route("MakeStockItemKit")]
  public async Task<IActionResult> MakeStockItemKit(KitMast kitMast)
  {
    bool isKitItemExistsByStockCode = await _kitItemService.CheckKitItemExistsByStockCode(kitMast.StockCode);
    if (isKitItemExistsByStockCode)
    {
      return Conflict("Already in a kit");
    }

    bool isStockItemKitAdded = await _kitMasterService.MakeStockItemKit(kitMast);
    return Ok(isStockItemKitAdded);
  }

  // POST api/stockitems/RemoveStockItemKit
  [HttpPost]
  [Route("RemoveStockItemKit")]
  public async Task<IActionResult> RemoveStockItemKit([FromBody] string stockCode)
  {
    bool isStockItemKitRemoved = await _stockItemService.RemoveStockItemKit(stockCode);
    return Ok(true);
  }

  // Get api/stockitems/UpdateStockItemTransfer
  [HttpGet]
  [Route("UpdateStockItemTransfer")]
  public async Task<IActionResult> UpdateStockItemTransfer(string stockCode)
  {
    bool isUpdatedStockItemTransfer = await _stockItemService.UpdateStockItemTransfer(stockCode);
    return Ok(isUpdatedStockItemTransfer);
  }

  [HttpGet]
  [Route("GetStockItemByCode/{stockCode}")]
  public async Task<IActionResult> GetStockItemByCode(string stockCode)
  {
    var data = await _stockItemService.GetStockItemByCode(stockCode);
    return Ok(data);
  }

  // GET api/stockitems/GetPackingInfo
  [HttpGet]
  [Route("GetPackingInfo")]
  public async Task<IActionResult> GetPackingInfo(string stockCode)
  {
    PackingInfo packing = await _stockItemService.GetPackingInfo(stockCode);
    return Ok(packing);
  }

  // GET api/stockitems/GetPackingInfoBatch
  [HttpPost]
  [Route("GetPackingInfoBatch")]
  public async Task<IActionResult> GetPackingInfoBatch([FromBody] List<string> stockCodes)
  {
    IEnumerable<PackingInfo> packingInfos = await _stockItemService.GetPackingInfoBatch(stockCodes);
    return Ok(packingInfos);
  }

  // GET api/stockitems/GetStockMstForPromotion
  [HttpGet]
  [Route("GetStockMstForPromotion")]
  public async Task<IActionResult> GetStockMstForPromotion(bool sellInactive)
  {
    IEnumerable<StockCodesForModalDto> stockCodeMst = await _stockItemService.GetStockMstForPromotion(sellInactive);
    return Ok(stockCodeMst);
  }

  // GET api/stockitems/GetCountOfProductsInDepartment/{departmentId}
  [HttpGet]
  [Route("GetCountOfProductsInDepartment/{departmentId}")]
  public async Task<IActionResult> GetCountOfProductsInDepartment(string departmentId)
  {
    int countOfItems = await _stockItemService.GetCountOfProducts(departmentId);
    return Ok(countOfItems);
  }

  // GET api/stockitems/GetCountOfProductsInSubDepartment/{departmentId}/{subDepartmentId}
  [HttpGet]
  [Route("GetCountOfProductsInSubDepartment/{departmentId}/{subDepartmentId}")]
  public async Task<IActionResult> GetCountOfProductsInSubDepartment(string departmentId, string subDepartmentId)
  {
    int countOfItems = await _stockItemService.GetCountOfProducts(departmentId, subDepartmentId);
    return Ok(countOfItems);
  }

  // GET api/stockitems/GetCountOfProductsInDetail/{departmentId}/{subDepartmentId}/{detailId}
  [HttpGet]
  [Route("GetCountOfProductsInDetail/{departmentId}/{subDepartmentId}/{detailId}")]
  public async Task<IActionResult> GetCountOfProductsInDetail(string departmentId, string subDepartmentId, string detailId)
  {
    int countOfItems = await _stockItemService.GetCountOfProducts(departmentId, subDepartmentId, detailId);
    return Ok(countOfItems);
  }

  // GET api/stockitems/GetStockItemsByDepartmentSubDepartmentAndDetail
  [HttpPost]
  [Route("GetStockItemsByDepartmentSubDepartmentAndDetail")]
  public async Task<IActionResult> GetStockItemsByDepartmentSubDepartmentAndDetail([FromBody] DepartmentSubDepartmentAndDetailDto departmentSubDepartmentAndDetail)
  {
    IEnumerable<StockItem> stockItems = await _stockItemService.GetStockItemsByDepartmentSubDepartmentAndDetail(departmentSubDepartmentAndDetail);
    return Ok(stockItems);
  }

  // POST api/stockitems/AddProductDiscountCode/{productDiscountCode}
  [HttpPost]
  [Route("AddProductDiscountCode/{productDiscountCode}")]
  public async Task<IActionResult> AddProductDiscountCode([FromBody] DepartmentSubDepartmentAndDetailDto departmentSubDepartmentAndDetail, string productDiscountCode)
  {
    bool isAdded = await _stockItemService.AddProductDiscountCode(productDiscountCode, departmentSubDepartmentAndDetail);
    return Ok(isAdded);
  }

  // POST api/stockitems/DeleteProductDiscountCode
  [HttpPost]
  [Route("DeleteProductDiscountCode")]
  public async Task<IActionResult> DeleteProductDiscountCode([FromBody] DepartmentSubDepartmentAndDetailDto departmentSubDepartmentAndDetail)
  {
    bool isDeleted = await _stockItemService.DeleteProductDiscountCode(departmentSubDepartmentAndDetail);
    return Ok(isDeleted);
  }

  // GET api/stockitems/IsProductsExist
  [HttpGet]
  [Route("IsProductsExist")]
  public async Task<IActionResult> IsProductsExist(string departmentId, string? subDepartmentId, string? detailId)
  {
    bool isExist = await _stockItemService.IsProductsExist(departmentId, subDepartmentId, detailId);
    return Ok(isExist);
  }

  // POST api/StockItems/GetStockItemsByCriteria
  [HttpPost]
  [Route("GetStockItemsByCriteria")]
  public async Task<IActionResult> GetStockItemsByCriteria(StockItemsCriteriaDto criteria)
  {
    var stockItems = await _stockItemService.GetStockItemsByCriteriaAsync(criteria);
    return Ok(stockItems);
  }

  // GET api/StockItems/GetAllStockItemsList
  [HttpGet]
  [Route("GetAllStockItemsList")]
  public async Task<IActionResult> GetAllStockItemsList()
  {
    return Ok(await _stockItemService.GetAllStockItemsListAsync());
  }

  // GET api/StockItems/GetStockCodesWithNoNetworkValue
  [HttpGet]
  [Route("GetStockCodesWithNoNetworkValue")]
  public async Task<IActionResult> GetStockCodesWithNoNetworkValue()
  {
    return Ok(await _stockItemService.GetStockCodesWithNoNetworkValueAsync());
  }

  // GET api/StockItems/GetStockCodesWithNetworkVendorSetting
  [HttpGet]
  [Route("GetStockCodesWithNetworkVendorSetting")]
  public async Task<IActionResult> GetStockCodesWithNetworkVendorSetting()
  {
    return Ok(await _stockItemService.GetStockCodesWithNetworkVendorSettingAsync());
  }

  // GET api/StockItems/GetStockCodesWithNoVendorSetting
  [HttpGet]
  [Route("GetStockCodesWithNoVendorSetting")]
  public async Task<IActionResult> GetStockCodesWithNoVendorSetting()
  {
    return Ok(await _stockItemService.GetStockCodesWithNoVendorSettingAsync());
  }
}
