namespace InfonetCSCR.Services;

public class DataDefinitionService : IDataDefinitionService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IStoreService _storeService;
  private readonly IRegionService _regionService;
  private readonly IStockTransferService _stockTransferService;
  private readonly IPriceGroupService _priceGroupService;
  private readonly IVendorService _vendorService;

  public DataDefinitionService(
    IApplicationConfig applicationConfig,
    IStoreService storeService,
    IStockTransferService stockTransferService,
    IRegionService regionService,
    IPriceGroupService priceGroupService,
    IVendorService vendorService)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCTransferConnectionString);
    this._storeService = storeService;
    _stockTransferService = stockTransferService;
    _regionService = regionService;
    _priceGroupService = priceGroupService;
    _vendorService = vendorService;
  }

  public async Task<IEnumerable<DataDefinition>> GetDataDefinitionsByCriteria(GetDataDefinitionsByCriteriaDto model)
  {
    IEnumerable<DataDefinition> dataDefinitions = await _unitOfWork.DataDefinitions.GetDataDefinitionsByCriteriaAsync(model);
    _unitOfWork.Commit();
    return dataDefinitions;
  }

  public async Task<IEnumerable<Store>> GetStoresFromStockTransfer()
  {
    IEnumerable<string> storeIds = await _stockTransferService.GetStoreIds();
    IEnumerable<Store> stores = await _storeService.GetStoresByStoreIds(storeIds);

    return stores;
  }

  public async Task<IEnumerable<Region>> GetRegionsFromStockTransfer()
  {
    IEnumerable<string> regionIds = await _stockTransferService.GetRegionIds();
    IEnumerable<Region> regions = await _regionService.GetRegionsByRegionIds(regionIds);

    return regions;
  }

  public async Task<IEnumerable<PriceGroup>> GetPriceGroupsFromStockTransfer()
  {
    IEnumerable<string> priceGroupIds = await _stockTransferService.GetPriceGroupIds();
    IEnumerable<PriceGroup> priceGroups = await _priceGroupService.GetPriceGroupsByPriceGroupIdsAsync(priceGroupIds);

    return priceGroups;
  }

  public async Task<IEnumerable<Vendor>> GetVendorsFromStockTransfer()
  {
    IEnumerable<string> vendorIds = await _stockTransferService.GetVendorIds();
    IEnumerable<Vendor> vendors = await _vendorService.GetVendorsByVendorIds(vendorIds);

    return vendors;
  }

  public async Task<bool> SaveDataDefinition(DataDefinitionDto dataDefinitionDto)
  {
    List<DataDefinition> dataDefinitions = new List<DataDefinition>();
    List<string>? defCodes = (List<string>?)dataDefinitionDto.DefCodes;
    List<string>? storeIds = (List<string>?)dataDefinitionDto.StoreIds;
    List<DataDefinitionMinimalDto>? dataDefinitionMinimalDtos = (List<DataDefinitionMinimalDto>?)dataDefinitionDto.DataDefinitions;

    if (defCodes.Count == 0)
    {
      defCodes.Add("REMOVE");
    }

    if (dataDefinitionDto.IsAllProductSelected == true)
    {
      ProductSearchParameterDto productSearchParameterDto = new ProductSearchParameterDto
      {
        StoreIds = dataDefinitionDto.StoreIds,
        RegionIds = dataDefinitionDto.RegionIds,
        PriceGroupIds = dataDefinitionDto.PriceGroupIds,
        VendorIds = dataDefinitionDto.VendorIds,
        IsAllProductSelected = true
      };

      List<StockTransferDto> stockTransferDtos = new List<StockTransferDto>();

      if (dataDefinitionDto.ShowByOption == "by-store")
      {
        StockTransferFilterDto stockTransferFilter = await _stockTransferService.GetStockTransfersByStoreIds(productSearchParameterDto);
        stockTransferDtos = (List<StockTransferDto>)stockTransferFilter.stockTransferDtos;
      }
      else if (dataDefinitionDto.ShowByOption == "by-region")
      {
        StockTransferFilterDto stockTransferFilter = await _stockTransferService.GetStockTransfersByRegionIds(productSearchParameterDto);
        stockTransferDtos = (List<StockTransferDto>)stockTransferFilter.stockTransferDtos;
      }
      else if (dataDefinitionDto.ShowByOption == "by-price-group")
      {
        StockTransferFilterDto stockTransferFilter = await _stockTransferService.GetStockTransfersByPriceGroupIds(productSearchParameterDto);
        stockTransferDtos = (List<StockTransferDto>)stockTransferFilter.stockTransferDtos;
      }
      else if (dataDefinitionDto.ShowByOption == "by-vendor")
      {
        StockTransferFilterDto stockTransferFilter = await _stockTransferService.GetStockTransfersByVendorIds(productSearchParameterDto);
        stockTransferDtos = (List<StockTransferDto>)stockTransferFilter.stockTransferDtos;
      }

      dataDefinitionMinimalDtos.Clear();
      foreach (StockTransferDto stockTransferDto in stockTransferDtos)
      {
        dataDefinitionMinimalDtos.Add(new DataDefinitionMinimalDto
        {
          ProductID = stockTransferDto.ProductID,
          Description = stockTransferDto?.Description,
          VendorId = ""
        });
      }
    }

    foreach (DataDefinitionMinimalDto dataDefinitionMinimalDto in dataDefinitionMinimalDtos)
    {
      foreach (string defCode in defCodes)
      {
        foreach (string storeId in storeIds)
        {
          dataDefinitions.Add(new DataDefinition
          {
            ProductID = dataDefinitionMinimalDto.ProductID,
            Description = dataDefinitionMinimalDto.Description,
            StoreId = storeId,
            DefCode = defCode,
            DefType = dataDefinitionDto.DefinitionType,
            TransferDate = DateTime.Now,
            TransferUser = dataDefinitionDto.TransferUser,
            VendorId = dataDefinitionMinimalDto.VendorId,
            Trans_Flag = "O"
          });
        }

        if (dataDefinitionDto.IsAllStoreSelected == true)
        {
          dataDefinitions.Add(new DataDefinition
          {
            ProductID = dataDefinitionMinimalDto.ProductID,
            Description = dataDefinitionMinimalDto.Description,
            StoreId = "ALL",
            DefCode = defCode,
            DefType = dataDefinitionDto.DefinitionType,
            TransferDate = DateTime.Now,
            TransferUser = dataDefinitionDto.TransferUser,
            VendorId = dataDefinitionMinimalDto.VendorId,
            Trans_Flag = "O"
          });
        }
      }
    }

    int SLICE_SIZE = 1000; // We have choosen 1000 as max insertions per call
    List<DataDefinition> tmpDataDefinitions = new List<DataDefinition>();
    List<Task<bool>> dataDefinitionTasks = new List<Task<bool>>();

    foreach (DataDefinition dataDefinition in dataDefinitions)
    {
      tmpDataDefinitions.Add(dataDefinition);
      if (tmpDataDefinitions.Count == SLICE_SIZE)
      {
        dataDefinitionTasks.Add(_unitOfWork.DataDefinitions.DeleteFromDataDefinitionBatch(tmpDataDefinitions));
        tmpDataDefinitions = new List<DataDefinition>();
      }
    }
    dataDefinitionTasks.Add(_unitOfWork.DataDefinitions.DeleteFromDataDefinitionBatch(tmpDataDefinitions));
    await Task.WhenAll(dataDefinitionTasks);

    tmpDataDefinitions = new List<DataDefinition>();
    dataDefinitionTasks = new List<Task<bool>>();

    foreach (DataDefinition dataDefinition in dataDefinitions)
    {
      tmpDataDefinitions.Add(dataDefinition);
      if (tmpDataDefinitions.Count == SLICE_SIZE)
      {
        dataDefinitionTasks.Add(_unitOfWork.DataDefinitions.InsertIntoDataDefinitionBatch(tmpDataDefinitions));
        tmpDataDefinitions = new List<DataDefinition>();
      }
    }
    dataDefinitionTasks.Add(_unitOfWork.DataDefinitions.InsertIntoDataDefinitionBatch(tmpDataDefinitions));
    await Task.WhenAll(dataDefinitionTasks);

    bool result = _unitOfWork.Commit();

    return (result);
  }
}
