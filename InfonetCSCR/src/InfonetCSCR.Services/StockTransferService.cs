namespace InfonetCSCR.Services;

public class StockTransferService : IStockTransferService
{
  private readonly IUnitOfWork _unitOfWork;

  public StockTransferService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCTransferConnectionString);
  }

  public async Task<bool> AddStockTransferForBatchDelete(string stockCode, string userCode)
  {
    bool result = await _unitOfWork.StockTransfers.AddStockTransferForBatchDeleteAsync(stockCode, userCode);
    _unitOfWork.Commit();
    return result;
  }

  public async Task<StockTransferFilterDto> GetStockTransfersByStoreId(ProductSearchParameterDto productSearchParameterDto)
  {
    string storeId = null;
    if (productSearchParameterDto.StoreIds != null && productSearchParameterDto.StoreIds.Count() > 0 )
    {
      storeId = productSearchParameterDto.StoreIds.FirstOrDefault();
    }
    int count = await _unitOfWork.StockTransfers.CountStockTransfersByStoreIdAsync(storeId);
    if(productSearchParameterDto.Length == -1) productSearchParameterDto.Length = count;

    IEnumerable<StockTransferDto> stockTransfers = await _unitOfWork.StockTransfers.GetStockTransfersByStoreIdAsync(productSearchParameterDto);

    StockTransferFilterDto stockTransferFilter = new StockTransferFilterDto
    {
      stockTransferDtos = stockTransfers,
      TotalItems = count
    };
    _unitOfWork.Commit();
    return stockTransferFilter;
  }

  public async Task<IEnumerable<string>> GetStoreIds()
  {
    IEnumerable<string> storeIds = await _unitOfWork.StockTransfers.GetStoreIdsAsync();
    _unitOfWork.Commit();
    return storeIds;
  }

  public async Task<IEnumerable<string>> GetRegionIds()
  {
    IEnumerable<string> regionIds = await _unitOfWork.StockTransfers.GetRegionIdsAsync();
    _unitOfWork.Commit();
    return regionIds;
  }

  public async Task<IEnumerable<string>> GetPriceGroupIds()
  {
    IEnumerable<string> priceGroupIds = await _unitOfWork.StockTransfers.GetPriceGroupIdsAsync();
    _unitOfWork.Commit();
    return priceGroupIds;
  }

  public async Task<IEnumerable<string>> GetVendorIds()
  {
    IEnumerable<string> vendorIds = await _unitOfWork.StockTransfers.GetVendorIdsAsync();
    _unitOfWork.Commit();
    return vendorIds;
  }

  public async Task<StockTransferFilterDto> GetStockTransfersByStoreIds(ProductSearchParameterDto productSearchParameterDto)
  {
    IEnumerable<StockTransferDto> stockTransfers = await _unitOfWork.StockTransfers.GetStockTransfersByStoreIdsAsync(productSearchParameterDto);
    int count = await _unitOfWork.StockTransfers.CountStockTransfersByStoreIdsAsync(productSearchParameterDto);
    
    StockTransferFilterDto stockTransferFilter = new StockTransferFilterDto
    {
      stockTransferDtos = stockTransfers,
      TotalItems = count
    };

    _unitOfWork.Commit();
    return stockTransferFilter;
  }

  public async Task<StockTransferFilterDto> GetStockTransfersByPriceGroupIds(ProductSearchParameterDto productSearchParameterDto)
  {
    IEnumerable<StockTransferDto> stockTransfers = await _unitOfWork.StockTransfers.GetStockTransfersByPriceGroupIdsAsync(productSearchParameterDto);
    int count = await _unitOfWork.StockTransfers.CountStockTransfersByPriceGroupIdsAsync(productSearchParameterDto);

    StockTransferFilterDto stockTransferFilter = new StockTransferFilterDto
    {
      stockTransferDtos = stockTransfers,
      TotalItems = count
    };

    _unitOfWork.Commit();
    return stockTransferFilter;
  }

  public async Task<StockTransferFilterDto> GetStockTransfersByRegionIds(ProductSearchParameterDto productSearchParameterDto)
  {
    IEnumerable<StockTransferDto> stockTransfers = await _unitOfWork.StockTransfers.GetStockTransfersByRegionIdsAsync(productSearchParameterDto);
    int count = await _unitOfWork.StockTransfers.CountStockTransfersByRegionIdsAsync(productSearchParameterDto);

    StockTransferFilterDto stockTransferFilter = new StockTransferFilterDto
    {
      stockTransferDtos = stockTransfers,
      TotalItems = count
    };

    _unitOfWork.Commit();
    return stockTransferFilter;
  }

  public async Task<StockTransferFilterDto> GetStockTransfersByVendorIds(ProductSearchParameterDto productSearchParameterDto)
  {
    IEnumerable<StockTransferDto> stockTransfers = await _unitOfWork.StockTransfers.GetStockTransfersByVendorIdsAsync(productSearchParameterDto);
    int count = await _unitOfWork.StockTransfers.CountStockTransfersByVendorIdsAsync(productSearchParameterDto);

    StockTransferFilterDto stockTransferFilter = new StockTransferFilterDto
    {
      stockTransferDtos = stockTransfers,
      TotalItems = count
    };

    _unitOfWork.Commit();
    return stockTransferFilter;
  }
}
