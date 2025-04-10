namespace InfonetCSCR.Services;

public class StockItemService : IStockItemService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IApplicationConfig _config;

  public StockItemService(IApplicationConfig config)
  {
    _config = config;
    this._unitOfWork = new UnitOfWork(_config, _config.CSCMasterConnectionString);
  }

  #region Private Methods

  private int PoQuantityRound(double? valueToRound)
  {
    int tmp;

    if (valueToRound < 1)
    {
      return 1;
    }
    else
    {
      tmp = (int)valueToRound;
      if (valueToRound - tmp >= 0.8)
      {
        return tmp + 1;
      }
      else
      {
        return tmp;
      }
    }
  }

  #endregion

  public async Task<StockCostInfo> GetStockCostInfo(GetStockCostInfoDto model)
  {
    StockCostInfo stockCost = await _unitOfWork.StockCosts.GetStockCostInfoAsync(model);
    _unitOfWork.Commit();
    return stockCost;
  }

  public async Task<IEnumerable<StockCostInfo>> GetStockCostInfoBatch(List<GetStockCostInfoDto> models, string selectedVendor)
  {
    List<Task<IEnumerable<StockCostInfo>>> tasks = new List<Task<IEnumerable<StockCostInfo>>>();
    int startIndex = 0;
    int SLICE_SIZE = 1000;
    int total = models.Count();
    while (startIndex < total)
    {
      List<GetStockCostInfoDto> slice = models.GetRange(startIndex, Math.Min(SLICE_SIZE, total - startIndex));
      if (_config.IsBackOffice)
      {
        List<(string stockCode, string vendorId)> StockCodesAndVendorIds = new();
        
        foreach (var model in slice)
          StockCodesAndVendorIds.Add((model.StockCode!, model.VendorId!));

        GetStockCostInfoByMultipleVendorBatchBackOfficeDto stockCostInfoByMultipleVendorBatchDto = new ()
        {
          StockCodesAndVendorIds = StockCodesAndVendorIds,
          OrderNo = models[0].OrderNo
        };

        tasks.Add(_unitOfWork.StockCosts.GetStockCostInfoByMultipleVendorBatchPBMAsync(stockCostInfoByMultipleVendorBatchDto, selectedVendor));
      }
      else
        tasks.Add(_unitOfWork.StockCosts.GetStockCostInfoBatchAsync(slice));

      startIndex += SLICE_SIZE;
    }

    IEnumerable<StockCostInfo>[] result = await Task.WhenAll(tasks);
    _unitOfWork.Commit();
    IEnumerable<StockCostInfo> stockCosts = result.SelectMany(x => x);
    return stockCosts;
  }

  public async Task<string> GetActiveVendor(GetActiveVendorDto model)
  {
    var _unitOfWorkMaster = new UnitOfWork(_config, _config.CSCMasterConnectionString);

    string? activeVendor;
    StockCostInfo? stockCostInfo = await _unitOfWorkMaster.StockCosts.GetStockCostInfoSingleAsync(new GetStockCostInfoDto
    {
      StockCode = model.StockCode,
      StoreId = model.StoreId,
      OrderNo = model.OrderNo,
    });

    if (stockCostInfo != null) activeVendor = stockCostInfo.VendorId;
    else
    {
      if (model.StoreId != "ALL")
      {
        stockCostInfo = await _unitOfWorkMaster.StockCosts.GetStockCostInfoSingleAsync(new GetStockCostInfoDto
        {
          StockCode = model.StockCode,
          StoreId = "ALL",
          OrderNo = model.OrderNo,
        });

        if (stockCostInfo != null) activeVendor = stockCostInfo.VendorId;
        else if (model.OrderNo > 0)
        {
          stockCostInfo = await _unitOfWorkMaster.StockCosts.GetStockCostInfoSingleAsync(new GetStockCostInfoDto
          {
            StockCode = model.StockCode,
            StoreId = model.StoreId,
            OrderNo = 0,
          });
          if (stockCostInfo != null) activeVendor = stockCostInfo.VendorId;
          else activeVendor = "ALL";
        }
        else activeVendor = "ALL";
      }
      else activeVendor = "ALL";
    }

    if (activeVendor == null) activeVendor = "ALL";
    if (model?.ShowByOption == "by-vendor")
    {
      return model.VendorId ?? activeVendor;
    }

    _unitOfWorkMaster.Commit();

    return activeVendor;
  }

  public async Task<StockItemsFilterData> GetStockItemsByFilter(SearchCriteria searchCriteria)
  {
    IEnumerable<StockItem> stockItems = await _unitOfWork.StockItems.GetStockItemsByFilterAsync(searchCriteria);
    int stockItemsCount = searchCriteria.Length > 0 ? await _unitOfWork.StockItems.GetStockItemsByFilterCountAsync(searchCriteria) : stockItems.Count();
    _unitOfWork.Commit();

    StockItemsFilterData stockItemsFilterData = new()
    {
      stockItems = stockItems,
      totalItems = stockItemsCount
    };
    return stockItemsFilterData;
  }

  public async Task<StockItemMinimalFilterDto> GetStockItemsByDeptSubDeptAndSubDetail(GetStockItemsByDeptSubDeptAndSubDetailDto searchParams)
  {
    IEnumerable<StockItemMinimal> stockItems;
    int totalItems;

    if (_config.IsBackOffice)
    {
      stockItems = await _unitOfWork.StockItems.GetStockItemsByDeptSubDeptAndSubDetailAsync(searchParams);
      totalItems = await _unitOfWork.StockItems.GetStockItemsByDeptSubDeptAndSubDetailCountAsync(searchParams);
      _unitOfWork.Commit();

      return new StockItemMinimalFilterDto
      {
        StockItems = stockItems,
        TotalItems = totalItems
      };
    }

    if (searchParams.IsRecentChanges == true)
    {
      stockItems = await _unitOfWork.StockItems.GetStockItemsOfRecentChangesAsync(searchParams);
      totalItems = await _unitOfWork.StockItems.GetStockItemsOfRecentChangesCountAsync(searchParams);
    }
    else
    {
      stockItems = await _unitOfWork.StockItems.GetStockItemsByDeptSubDeptAndSubDetailAsync(searchParams);
      totalItems = await _unitOfWork.StockItems.GetStockItemsByDeptSubDeptAndSubDetailCountAsync(searchParams);
    }

    _unitOfWork.Commit();

    return new StockItemMinimalFilterDto
    {
      StockItems = stockItems,
      TotalItems = totalItems
    };
  }

  public async Task<StockItemMinimalFilterDto> GetStockItemsByVendor(GetStockItemsByVendorDto searchParams)
  {
    IEnumerable<StockItemMinimal> stockItems = await _unitOfWork.StockItems.GetStockItemsByVendorAsync(searchParams);
    int totalItems = await _unitOfWork.StockItems.GetStockItemsByVendorCountAsync(searchParams);
    _unitOfWork.Commit();

    return new StockItemMinimalFilterDto
    {
      StockItems = stockItems,
      TotalItems = totalItems
    };
  }

  public async Task<StockMaster> GetStockMasterByStockCode(string stockCode)
  {
    StockMaster stockMaster = await _unitOfWork.StockItems.GetStockMasterByStockCodeAsync(stockCode);
    _unitOfWork.Commit();
    return stockMaster;
  }

  public async Task<StockMaster> GetStockItemByCode(string stockCode)
  {
    StockMaster stockMaster = await _unitOfWork.StockItems.GetStockItemByCode(stockCode);
    _unitOfWork.Commit();
    return stockMaster;
  }

  public async Task<bool> UpdateStockItemTransfer(string stockCode)
  {
    await _unitOfWork.StockItems.UpdateStockItemTransfer(stockCode);
    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> RemoveStockItemKit(string stockCode)
  {
    await _unitOfWork.KitMasters.DeleteKitMasterByStockCodeAsync(stockCode);
    await _unitOfWork.KitItems.DeleteKitItemByKitCodeAsync(stockCode);
    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<int> GetStockItemsByFilterCount(SearchCriteria criteria)
  {
    int result = await _unitOfWork.StockItems.GetStockItemsByFilterCountAsync(criteria);
    _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> IsVendorActive(string stockCode, string storeId, string vendorId, short orderNo)
  {
    bool isExist = await _unitOfWork.StockItems.IsVendorActive(stockCode, storeId, vendorId, orderNo);
    return isExist;
  }

  public async Task<PackingInfo> GetPackingInfo(string stockCode)
  {
    PackingInfo packing = await _unitOfWork.StockItems.GetPackingInfo(stockCode);
    _unitOfWork.Commit();
    return packing;
  }

  public async Task<IEnumerable<PackingInfo>> GetPackingInfoBatch(List<string> models)
  {
    List<Task<IEnumerable<PackingInfo>>> tasks = new List<Task<IEnumerable<PackingInfo>>>();
    int startIndex = 0;
    int SLICE_SIZE = 1000;
    int total = models.Count();
    while (startIndex < total)
    {
      List<string> slice = models.GetRange(startIndex, Math.Min(SLICE_SIZE, total - startIndex));
      tasks.Add(_unitOfWork.StockItems.GetPackingInfoBatch(slice));
      startIndex += SLICE_SIZE;
    }
    IEnumerable<PackingInfo>[] result = await Task.WhenAll(tasks);
    _unitOfWork.Commit();
    IEnumerable<PackingInfo> packingInfos = result.SelectMany(x => x);
    return packingInfos;
  }

  public async Task<IEnumerable<StockCodesForModalDto>> GetStockMstForPromotion(bool sellInactive)
  {
    var stockCodeMst = await _unitOfWork.StockItems.GetStockMstForPromotion(sellInactive);

    _unitOfWork.Commit();

    return stockCodeMst;
  }

  public async Task<int> GetCountOfProducts(string departmentId, string? subDepartmentId = null, string? detail = null)
  {
    int countOfItems = await _unitOfWork.StockItems.GetCountOfProductsAsync(departmentId, subDepartmentId, detail);
    _unitOfWork.Commit();
    return countOfItems;
  }

  public async Task<IEnumerable<StockItem>> GetStockItemsByDepartmentSubDepartmentAndDetail(DepartmentSubDepartmentAndDetailDto departmentSubDepartmentAndDetail)
  {
    IEnumerable<StockItem> stockItems = await _unitOfWork.StockItems.GetStockItemsByDepartmentSubDepartmentAndDetailAsync(departmentSubDepartmentAndDetail);
    _unitOfWork.Commit();
    return stockItems;
  }

  public async Task<bool> AddProductDiscountCode(string productDiscountCode, DepartmentSubDepartmentAndDetailDto departmentSubDepartmentAndDetail)
  {
    bool isAdded = await _unitOfWork.StockItems.AddProductDiscountCodeAsync(productDiscountCode, departmentSubDepartmentAndDetail);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> DeleteProductDiscountCode(DepartmentSubDepartmentAndDetailDto departmentSubDepartmentAndDetail)
  {
    bool isDeleted = await _unitOfWork.StockItems.DeleteProductDiscountCodeAsync(departmentSubDepartmentAndDetail);
    _unitOfWork.Commit();
    return isDeleted;
  }

  public async Task<bool> IsProductsExist(string departmentId, string? subDepartmentId, string? detailId)
  {
    bool isExist = await _unitOfWork.StockItems.IsProductsExistAsync(departmentId, subDepartmentId, detailId);
    _unitOfWork.Commit();
    return isExist;
  }

  public async Task<Dictionary<string, int>> GetOrderQuantities(
    IList<string> stockCodes,
    Dictionary<string, bool> kitOrderDictionary,
    bool kitsInPOPolicy)
  {
    Dictionary<string, int> orderQuantitiesDictionary = new Dictionary<string, int>();

    IEnumerable<OrderQuantityDataDto> orderQuantityDatas =
      await _unitOfWork.StockItems.GetOrderQuantityDataAsync(stockCodes);

    foreach (OrderQuantityDataDto orderQuantityData in orderQuantityDatas)
    {
      int? packing = 1;
      double? inStock = 0;
      int orderQuantity = 0;

      if (orderQuantityData != null)
      {
        bool kitOrder = kitOrderDictionary[orderQuantityData.StockCode];
        if (!kitsInPOPolicy)
        {
          if (orderQuantityData.ReordQuantity > 0 && orderQuantityData.InStock <= orderQuantityData.ReordQuantity)
          {
            packing = (orderQuantityData.Packing == null || orderQuantityData.Packing == 0) ? 1 : orderQuantityData.Packing;
            inStock = (orderQuantityData.InStock < 0) ? 0 : orderQuantityData.InStock;
            orderQuantity = PoQuantityRound((orderQuantityData.MaxQuantity - inStock) / packing);
          }
        }
        else
        {
          packing = (orderQuantityData.Packing == null || orderQuantityData.Packing == 0) ? 1 : orderQuantityData.Packing;
          if (orderQuantityData.ReordQuantity > 0
            && (orderQuantityData.InStock <= orderQuantityData.ReordQuantity)
            && (orderQuantityData.MaxQuantity >= packing))
          {
            if (orderQuantityData.Packing == 1 || (orderQuantityData.Packing != 1 && kitOrder))
            {
              orderQuantity = (int)((double)(orderQuantityData.MaxQuantity - orderQuantityData.InStock)).Round();
            }
            else if (orderQuantityData.Packing != 1 && kitOrder == false)
            {
              orderQuantity = (int)((double)(((int)(orderQuantityData.MaxQuantity - orderQuantityData.InStock)) / packing)).Round();
            }
          }
        }
      }

      if (orderQuantityData != null)
      {
        orderQuantitiesDictionary.Add(orderQuantityData.StockCode, orderQuantity);
      }
    }

    _unitOfWork.Commit();
    return orderQuantitiesDictionary;
  }

  public async Task<IEnumerable<StockCodesForModalDto>> GetStockItemsByCriteriaAsync(StockItemsCriteriaDto criteria)
  {
    var stockItems = await _unitOfWork.StockItems.GetStockItemsByCriteriaAsync(criteria);
    _unitOfWork.Commit();
    return stockItems;
  }

  public async Task<IEnumerable<StockItem>> GetAllStockItemsListAsync()
  {
    var stockItems = await _unitOfWork.StockItems.GetAllStockItemsListAsync();
    _unitOfWork.Commit();
    return stockItems;
  }

  public async Task<IEnumerable<StockCost>> GetStockCodesWithNoNetworkValueAsync()
  {
    var items = await _unitOfWork.StockCosts.GetStockCodesWithNoNetworkValueAsync();
    _unitOfWork.Commit();
    return items;
  }

  public async Task<IEnumerable<StockCost>> GetStockCodesWithNetworkVendorSettingAsync()
  {
    var items = await _unitOfWork.StockCosts.GetStockCodesWithNetworkVendorSettingAsync();
    _unitOfWork.Commit();
    return items;
  }

  public async Task<IEnumerable<StockCost>> GetStockCodesWithNoVendorSettingAsync()
  {
    var items = await _unitOfWork.StockCosts.GetStockCodesWithNoVendorSettingAsync();
    _unitOfWork.Commit();
    return items;
  }

  public async Task<IEnumerable<StockMaster?>> GetStockMasterInfo(string startDepartment, string endDepartment)
  {
    var stockItems = await _unitOfWork.StockItems.GetStockMasterInfoAsync(startDepartment, endDepartment);
    _unitOfWork.Commit();
    return stockItems;
  }

  public async Task<IEnumerable<StockCostInfo>> GetStockCostInfoBatchForBackOffice(StockCostInfoBatchParameterDto stockCostInfoParameterBackOffice)
  {
    IEnumerable<StockCostInfo?> stockCost;

    if (_config.IsBackOffice)
    {
      StockCostInfoBatchParameterBackOfficeDto stockCostDto = new()
      {
        VendorId = stockCostInfoParameterBackOffice.VendorId,
        StockCodes = stockCostInfoParameterBackOffice.StockCodes,
        OrderNo = stockCostInfoParameterBackOffice.OrderNo,
      };

      stockCost = await _unitOfWork.StockCosts.GetStockCostInfoBatchForBackOfficeAsync(stockCostDto);
    }
    else
    {
      stockCost = await _unitOfWork.StockCosts.GetStockCostInfoBatchForHeadOfficeAsync(stockCostInfoParameterBackOffice);
    }

    _unitOfWork.Commit();
    return stockCost;
  }

  public async Task<IEnumerable<StockPriceInfoDto?>> GetStockPriceInfoBatch(IEnumerable<StockCostInfo> stockCostInfo)
  {
    IEnumerable<StockPriceInfoDto?> stockCostInfos =
      await _unitOfWork.StockCosts.GetStockPriceInfoBatchForBackOfficeAsync(stockCostInfo);
    _unitOfWork.Commit();
    return stockCostInfos;
  }

  public async Task<IEnumerable<StockTax?>> GetStockTaxInfoBatchForBackOffice(List<string> stockCodes, string storeID)
  {
    IEnumerable<StockTax?> stockTaxInfos =
      await _unitOfWork.StockCosts.GetStockTaxInfoBatchForBackOfficeAsync(stockCodes, storeID);
    _unitOfWork.Commit();
    return stockTaxInfos;
  }

  public async Task<IEnumerable<StockCharge>> GetStockChargeInfoBatchForBackOffice(List<string> stockCodes, string storeID)
  {
    IEnumerable<StockCharge?> stockTaxInfos =
      await _unitOfWork.StockCosts.GetStockChargeInfoBatchForBackOfficeAsync(stockCodes, storeID);
    _unitOfWork.Commit();
    return stockTaxInfos;
  }
}
