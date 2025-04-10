namespace InfonetCSCR.Services;

public class StockTakesAndReceivingService : IStockTakesAndReceivingService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IApplicationConfig _config;

  public StockTakesAndReceivingService(IApplicationConfig applicationConfig, IApplicationConfig config)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
    _config = config;
  }

  public async Task<StockTakesAndReceivingResult> GetStockTakesAndReceiveData(StockTakesAndReceiveRequestDto stockTakesAndReceiveRequest)
  {
    int receiveDataCount = await _unitOfWork.StockTakesAndReceiving.GetStockTakesAndReceiveDataCountAsync(stockTakesAndReceiveRequest);
    stockTakesAndReceiveRequest.Length = stockTakesAndReceiveRequest.Length == -1 ? receiveDataCount
                                         : stockTakesAndReceiveRequest.Length;

    IEnumerable<StockTakesAndReceiveDto>? receiveData = await _unitOfWork.StockTakesAndReceiving.GetStockTakesAndReceiveDataAsync(stockTakesAndReceiveRequest);
    _unitOfWork.Commit();

    StockTakesAndReceivingResult stockTakesAndReceivingResult = new()
    {
      receiveData = receiveData,
      totalItems = receiveDataCount
    };

    return stockTakesAndReceivingResult;
  }

  public async Task<IEnumerable<StockDto>?> GetStocks(StockTakesAndRecievingDto stockTakesAndRecievingDto)
  {
    IEnumerable<StockDto>? stocks = await _unitOfWork.StockTakesAndReceiving.GetStocksAsync(stockTakesAndRecievingDto);
    _unitOfWork.Commit();
    return stocks;
  }

  public async Task<bool> AddRecline(ReclineDto reclineDto)
  {
    bool isAdded = false;
    int batchId = await GetBatchId(reclineDto.Purpose);
    string storeId = await GetStore();
    int sequence = await GetSequence(batchId);
    decimal systemQuantity = await GetSystemQuantity(reclineDto.StockCode);

    reclineDto.Sequence = sequence;
    reclineDto.FromText = false;
    reclineDto.InStock = systemQuantity;
    reclineDto.BatchId = batchId;
    reclineDto.UpdateInitial = reclineDto.IsQuantityUpdate;

    var recLineByStockCode = await GetReclineByStockCode(reclineDto.StockCode, reclineDto.Purpose);

    if (recLineByStockCode == null || (reclineDto.IsQuantityUpdate && reclineDto.Purpose == (int)StockCountType.StockReceiving))
    {
      RecLine recLine = new RecLine
      {
        RecNum = (short)batchId,
        Branch = storeId,
        InvNum = reclineDto.Invoice.ToString(),
        StockCode = reclineDto.StockCode,
        Descript = reclineDto.Description,
        Quantity = reclineDto.Quantity,
        Cost = reclineDto.Cost,
        Purpose = reclineDto.Purpose,
        RecUpdate = false,
        Sequence = sequence,
        SysQty = systemQuantity,
        DateCount = DateTime.Now
      };

      isAdded = await AddRecLine(recLine);
    }
    else
    {
      isAdded = true;
    }

    if (reclineDto.IsQuantityUpdate && reclineDto.Purpose == (int)StockCountType.StockTakes)
    {
      await UpdateQuantity(reclineDto);
    }

    isAdded = reclineDto.Purpose == (int)StockCountType.StockReceiving ? await UpdateTotal(reclineDto) : await CalculateSales(reclineDto);

    return isAdded;
  }

  public async Task<bool> UpdateQuantityForStockTakes(string stockCode, decimal quantity, string costType)
  {
    bool isupdated;

    int batchId = await GetBatchId(2);
    decimal systemQuantity = await GetSystemQuantity(stockCode);

    ReclineDto reclineDto = new ReclineDto
    {
      StockCode = stockCode,
      InStock = systemQuantity,
      Quantity = quantity,
      Purpose = 2,
      CostType = costType,
      BatchId = (short)batchId,
      UpdateInitial = true
    };

    isupdated = await UpdateQuantity(reclineDto);
    isupdated = await CalculateSales(reclineDto);

    return isupdated;
  }

  public async Task<bool> DeleteRecLine(StockTakesAndReceiveDto receiveDto)
  {
    bool isDeleted = await _unitOfWork.StockTakesAndReceiving.DeleteRecLineAsync(receiveDto);
    _unitOfWork.Commit();
    return isDeleted;
  }

  public async Task<bool> DeleteAllRecLineAndRecHead(int purpose)
  {
    await DeleteAllRecHead(purpose);
    bool isDeleted = await DeleteAllRecLine(purpose);
    return isDeleted;
  }

  public async Task<bool> AddAllStocksToRecLineForStockTakesAsync(StockTakesAndRecievingDto stockTakesAndRecievingDto)
  {
    bool isUpdated = false;

    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      isUpdated = await uow.StockTakesAndReceiving.AddAllStocksToRecLineForStockTakesAsync(stockTakesAndRecievingDto);
      uow.Commit();
    }

    return isUpdated;
  }

  private async Task<int> GetBatchId(int purpose, string recType = "MA")
  {
    int batchId = await GetMaxRecNumFromRecHead(purpose);

    batchId = Math.Max(batchId, await GetMaxRecNumFromRecHeadSus(purpose));
    batchId = Math.Max(batchId, await GetMaxBatchIdFromStockHist(purpose));

    bool isExists = await IsRecHeadExists(batchId, purpose);
    if (!isExists)
    {
      await AddRecHead(batchId, purpose, recType);
    }

    return batchId;
  }

  private async Task<int> GetMaxRecNumFromRecHead(int purpose)
  {
    int recNum;
    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      recNum = await uow.StockTakesAndReceiving.GetMaxRecNumFromRecHeadAsync(purpose);
      uow.Commit();
    }
    return recNum;
  }

  private async Task<int> GetMaxRecNumFromRecHeadSus(int purpose)
  {
    int recNum;
    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      recNum = await uow.StockTakesAndReceiving.GetMaxRecNumFromRecHeadSusAsync(purpose);
      uow.Commit();
    }
    return recNum;
  }

  private async Task<int> GetMaxBatchIdFromStockHist(int purpose)
  {
    int maxBatchId;
    using (var uow = new UnitOfWork(_config, _config.CSCTransConnectionString))
    {
      maxBatchId = await uow.StockTakesAndReceiving.GetMaxBatchIdFromStockHistAsync(purpose);
      uow.Commit();
    }
    return maxBatchId;
  }

  private async Task<bool> IsRecHeadExists(int batchId, int purpose)
  {
    bool isExists;
    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      isExists = await uow.StockTakesAndReceiving.IsRecHeadExistsAsync(batchId, purpose);
      uow.Commit();
    }
    return isExists;
  }

  private async Task<bool> AddRecHead(int batchId, int purpose, string RecType)
  {
    bool isAdded;
    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      isAdded = await uow.StockTakesAndReceiving.AddRecHeadAsync(batchId, purpose, RecType);
      uow.Commit();
    }
    return isAdded;
  }

  private async Task<string> GetStore()
  {
    string store;
    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      store = await uow.StockTakesAndReceiving.GetStoreAsync();
      uow.Commit();
    }
    return store;
  }

  private async Task<int> GetSequence(int batchId)
  {
    int sequence;
    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      sequence = await uow.StockTakesAndReceiving.GetSequenceAsync(batchId);
      uow.Commit();
    }
    return sequence;
  }

  private async Task<decimal> GetSystemQuantity(string stockCode)
  {
    decimal inStock;
    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      inStock = await uow.StockTakesAndReceiving.GetSystemQuantityAsync(stockCode);
      uow.Commit();
    }
    return inStock;
  }

  private async Task<bool> AddRecLine(RecLine recLine)
  {
    bool isAdded = false;
    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      isAdded = await uow.StockTakesAndReceiving.AddRecLineAsync(recLine);
      uow.Commit();
    }
    return isAdded;
  }

  private async Task<bool> CalculateSales(ReclineDto reclineDto)
  {
    bool isUpdated = true;

    var stockCost = await GetCostForSales(reclineDto.StockCode);

    reclineDto.Cost = stockCost?.Cost ?? 0;
    reclineDto.VendorId = stockCost?.VendorId;

    await UpdateReclineForSales(reclineDto.CostType, reclineDto.Purpose, reclineDto.Cost.GetValueOrDefault(), reclineDto.StockCode);
    var recline = await GetReclineBybatchid(reclineDto.StockCode, reclineDto.Purpose, reclineDto.BatchId.GetValueOrDefault());

    if (recline == null)
    {
      return true;
    }

    var dateCount = recline?.DateCount ?? DateTime.Now;
    reclineDto.SalesQuantity = await CalculateSalesForQuantity(reclineDto.StockCode, dateCount);

    reclineDto.Quantity = recline?.Quantity;
    reclineDto.InitialCount = recline?.InitialCount;

    await UpdateReclineForSalesQuantityAsync(reclineDto);

    return isUpdated;
  }

  public async Task<bool> CalculateAllSales(int purpose, bool isUpdateInitial, bool enableUpdate = false)
  {
    List<Task<int>> tasks = new();
    bool isUpdated = false;

    List<(string stockCode, string costType, int purpose, decimal cost)> listOfStockCodeCostTypePurposeAndCost = new();
    List<RecLine> recLineList = new();
    List<RecLine> recLines = (await GetAllRecLines(purpose)).ToList();

    if (recLines == null || !recLines.Any())
    {
      return true;
    }

    List<(string StockCode, DateTime DateCount)> stockCodeAndDateList = recLines.Select(x => (x.StockCode, x.DateCount ?? DateTime.Now)).ToList();
    List<string> stockCodes = recLines.Select(x => x.StockCode).ToList();

    Dictionary<string, StockCostDto?> stockCostsDictionary = (await GetStockCostsByStockCodes(stockCodes))
                                                             .ToDictionary(stockCost => stockCost.StockCode, stockCost => stockCost);
    Dictionary<string, decimal> stockCodeQuantityDictionary = await GetQuantityByStockCodeDictionary(stockCodeAndDateList);
    Dictionary<string, decimal> inStockDictionary = (await _unitOfWork.StockBranches.GetStockBranchesByStockCodeListAsync(stockCodes))
                                                          .ToDictionary(item => item.StockCode, item => item.InStock ?? 0);

    foreach (RecLine recLine in recLines)
    {
      //enableUpdate = true means CalculateAllSales function has been called from import from file.
      //value of RecUpdate is true or false
      if (enableUpdate && !recLine.RecUpdate.GetValueOrDefault())
      {
        continue;
      }

      StockCostDto? stockCost = stockCostsDictionary.TryGetValue(recLine.StockCode, out var tempStockCost)
        ? tempStockCost : null;

      var dateCount = recLine?.DateCount ?? DateTime.Now;
      recLine.Cost = stockCost?.Cost ?? 0;
      recLine.ActiveVendorCode = stockCost?.VendorId ?? null;
      recLine.SysQty = inStockDictionary.TryGetValue(recLine.StockCode, out var tempInstock) ? tempInstock : 0; //todo get instock from stockbr and assign as sysqty
      if (isUpdateInitial)
      {
        recLine.InitialCount = recLine?.Quantity;
      }

      decimal SalesQuantity = stockCodeQuantityDictionary.TryGetValue(recLine.StockCode, out var tempSalesQuantity) ? tempSalesQuantity : 0;
      recLine.Quantity = recLine?.InitialCount - SalesQuantity;
      recLineList.Add(recLine);
    }

    for (int i = 0; i < recLineList.Count; i += 1000)
    {
      List<RecLine> sublist = recLineList.Skip(i).Take(1000).ToList();
      tasks.Add(_unitOfWork.StockTakesAndReceiving.UpdateReclineListAsync(sublist));
    }

    int[] result = await Task.WhenAll(tasks);
    isUpdated = result.Sum() > 0;

    _unitOfWork.Commit();

    return isUpdated;
  }

  public async Task<decimal> GetCost(string stockCode)
  {
    StockMstDto? costInfo;
    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      costInfo = await uow.StockTakesAndReceiving.GetCostInfoAsync(stockCode);
      uow.Commit();
    }
    return costInfo?.Cost ?? 0;
  }

  public async Task<StockMstDto?> GetCostInfo(string stockCode)
  {
    StockMstDto? costInfo;
    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      costInfo = await uow.StockTakesAndReceiving.GetCostInfoAsync(stockCode);
      uow.Commit();
    }
    return costInfo;
  }

  public async Task<bool> UpdateTotal(ReclineDto reclineDto)
  {
    bool isUpdated = false;
    string? vendorId = "";

    IEnumerable<RecLine?> recLines = await GetRecLines(reclineDto);
    if (recLines == null || recLines.Count() == 0)
    {
      return true;
    }

    IEnumerable<Tax> taxes = await GetTaxes(reclineDto.StockCode, reclineDto.ExceptedTax);
    decimal? assoc = await GetAssoc(reclineDto.StockCode);
    IEnumerable<AssocTaxDto> assocTaxes = await GetAssocTaxes(reclineDto.StockCode, reclineDto.ExceptedTax);

    if (reclineDto.FromText.GetValueOrDefault())
    {
      if (reclineDto.Cost == 0)
      {
        StockMstDto? stockmst = await GetCostInfo(reclineDto.StockCode);
        reclineDto.Cost = stockmst?.Cost ?? 0;
        vendorId = stockmst?.VendorId;
      }
    }

    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      List<Task<int>> task = new List<Task<int>>();
      foreach (var recLine in recLines)
      {
        var oldRecLine = new RecLine
        {
          RecNum = recLine.RecNum,
          StockCode = recLine.StockCode,
          Sequence = recLine.Sequence
        };
        recLine.Quantity = reclineDto.Quantity;
        recLine.Cost = reclineDto.Cost;
        recLine.ActiveVendorCode = vendorId;
        recLine.Amount = reclineDto.Cost * reclineDto.Quantity;
        recLine.Assoc = recLine.Assoc ?? 0;

        if (assoc != null)
        {
          recLine.Assoc = (reclineDto.Quantity * assoc) ?? 0;
        }

        recLine.Taxes = 0;
        foreach (var tax in taxes)
        {
          recLine.Taxes = decimal.Round((recLine.Taxes ?? 0) + reclineDto.Cost.GetValueOrDefault() * reclineDto.Quantity.GetValueOrDefault() * tax.Rate / 100, reclineDto.CostDec.GetValueOrDefault());
        }

        foreach (var assocTax in assocTaxes)
        {
          recLine.Taxes = decimal.Round((recLine.Taxes ?? 0) + (assocTax.Price ?? 0) * reclineDto.Quantity.GetValueOrDefault() * (assocTax.Rate ?? 0) / 100, reclineDto.CostDec.GetValueOrDefault());
        }
        recLine.Total = decimal.Round((recLine.Amount ?? 0) + (recLine.Taxes ?? 0) + (recLine.Assoc ?? 0), 2);
        task.Add(uow.StockTakesAndReceiving.UpdateRecLineAsync(recLine, oldRecLine));
      }

      int[] result = await Task.WhenAll(task);
      isUpdated = result.Sum() > 0;
      uow.Commit();
    }
    return isUpdated;
  }

  public async Task<IEnumerable<RecLine?>> GetRecLines(ReclineDto reclineDto)
  {
    IEnumerable<RecLine?> recLines;

    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      recLines = await uow.StockTakesAndReceiving.GetRecLinesAsync(reclineDto);
      uow.Commit();
    }
    return recLines;
  }

  public async Task<IEnumerable<Tax>> GetTaxes(string stockCode, string exceptedTax)
  {
    IEnumerable<Tax> taxes;

    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      taxes = await uow.StockTakesAndReceiving.GetTaxesAsync(stockCode, exceptedTax);
      uow.Commit();
    }
    return taxes;
  }

  public async Task<decimal?> GetAssoc(string stockCode)
  {
    decimal? assoc;

    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      assoc = await uow.StockTakesAndReceiving.GetAssocAsync(stockCode);
      uow.Commit();
    }
    return assoc;
  }

  public async Task<IEnumerable<AssocTaxDto>> GetAssocTaxes(string stockCode, string exceptedTax)
  {
    IEnumerable<AssocTaxDto> assocTaxes;

    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      assocTaxes = await uow.StockTakesAndReceiving.GetAssocTaxesAsync(stockCode, exceptedTax);
      uow.Commit();
    }
    return assocTaxes;
  }

  public async Task<bool> AddAllStocksToRecline(StockTakesAndRecievingDto stockTakesAndRecievingDto)
  {
    bool isAdded = false;
    int batchId = await GetBatchId(stockTakesAndRecievingDto.Purpose);
    stockTakesAndRecievingDto.BatchId = (short)batchId;

    if (stockTakesAndRecievingDto.Purpose == (int)StockCountType.StockReceiving)
    {
      isAdded = await AddAllStocksToRecLineForStockReceiving(stockTakesAndRecievingDto);
    }
    else
    {
      isAdded = await AddAllStocksToRecLineForStockTakesAsync(stockTakesAndRecievingDto);
    }

    return isAdded;
  }

  public async Task<bool> AddAllStocksToRecLineForStockReceiving(StockTakesAndRecievingDto stockTakesAndRecievingDto)
  {
    bool isAdded = await _unitOfWork.StockTakesAndReceiving.AddAllStocksToRecLineForStockReceivingAsync(stockTakesAndRecievingDto);
    _unitOfWork.Commit();
    return isAdded;
  }

  private async Task<bool> DeleteAllRecHead(int purpose)
  {
    bool isDeleted = false;
    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      isDeleted = await uow.StockTakesAndReceiving.DeleteAllRecHeadAsync(purpose);
      uow.Commit();
    }
    return isDeleted;
  }

  public async Task<bool> DeleteAllRecLine(int purpose, bool deleteFromImport = false)
  {
    bool isDeleted = false;
    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      isDeleted = await uow.StockTakesAndReceiving.DeleteAllRecLineAsync(purpose, deleteFromImport);
      uow.Commit();
    }
    return isDeleted;
  }

  private async Task<int> GetRecLineCount(int purpose)
  {
    int count = 0;
    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      count = await uow.StockTakesAndReceiving.GetRecLineCountAsync(purpose);
      uow.Commit();
    }
    return count;
  }

  public async Task<bool> Suspend(int purpose)
  {
    bool isAllRecHeadInsertedToRecHeadSus = await _unitOfWork.StockTakesAndReceiving.InsertAllRecHeadToRecHeadSusAsync(purpose);
    bool isAllRecLineInsertedToRecLineSus = await _unitOfWork.StockTakesAndReceiving.InsertAllRecLineToRecLineSusAsync(purpose);
    bool isAllRecHeadsDeleted = await _unitOfWork.StockTakesAndReceiving.DeleteAllRecHeadAsync(purpose);
    bool isAllRecLinesDeleted = await _unitOfWork.StockTakesAndReceiving.DeleteAllRecLineAsync(purpose, false);

    _unitOfWork.Commit();
    return isAllRecHeadInsertedToRecHeadSus && isAllRecLineInsertedToRecLineSus && isAllRecLinesDeleted && isAllRecHeadsDeleted;
  }

  public async Task<bool> Unsuspend(int purpose)
  {
    if (await GetRecLineCount(purpose) == 0)
    {
      await _unitOfWork.StockTakesAndReceiving.DeleteAllRecHeadAsync(purpose);
    }

    bool isAllRecHeadSusInsertedToRecHead = await _unitOfWork.StockTakesAndReceiving.InsertAllRecHeadSusToRecHeadAsync(purpose);
    bool isAllRecLineSusInsertedToRecLine = await _unitOfWork.StockTakesAndReceiving.InsertAllRecLineSusToRecLineAsync(purpose);
    bool isAllRecHeadsDeleted = await _unitOfWork.StockTakesAndReceiving.DeleteAllRecHeadSusAsync(purpose);
    bool isAllRecLinesDeleted = await _unitOfWork.StockTakesAndReceiving.DeleteAllRecLineSusAsync(purpose);

    _unitOfWork.Commit();
    return isAllRecHeadSusInsertedToRecHead && isAllRecLineSusInsertedToRecLine && isAllRecLinesDeleted && isAllRecHeadsDeleted;
  }

  public async Task<int> GetRecLineSusCount(int purpose)
  {
    int count = await _unitOfWork.StockTakesAndReceiving.GetRecLineSusCountAsync(purpose);
    _unitOfWork.Commit();
    return count;
  }

  public async Task<IEnumerable<StockTakesAndReceivingReportDto>> GetStockTakesAndReceivingReportData(int purpose)
  {
    IEnumerable<StockTakesAndReceivingReportDto>? reportData = purpose == (int)StockCountType.StockReceiving ?
                                  await _unitOfWork.StockTakesAndReceiving.GetStockReceivingReportDataAsync
                                  ()
                                  : await GetStockTakesReportData();

    await UpdateRecPrint(purpose);

    _unitOfWork.Commit();

    return reportData;
  }

  private async Task<bool> UpdateRecPrint(int purpose)
  {
    bool isUpdated = false;
    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      isUpdated = await uow.StockTakesAndReceiving.UpdateRecPrint(purpose);
      uow.Commit();
    }

    return isUpdated;
  }

  private async Task<IEnumerable<StockTakesAndReceivingReportDto>?> GetStockTakesReportData()
  {
    var stockTakesReportsData = await _unitOfWork.StockTakesAndReceiving.GetStockTakesReportDataAsync();

    foreach (var stockTakesReportData in stockTakesReportsData)
    {
      var isStockTypeF = stockTakesReportData.StockType == "F" ? true : false;

      stockTakesReportData.Quantity = isStockTypeF
                                    ? (stockTakesReportData.IQtyRecv ?? 0) * (stockTakesReportData.Quantity ?? 0)
                                    : stockTakesReportData.IQtyRecv ?? 0;

      stockTakesReportData.Descript = isStockTypeF ? stockTakesReportData.StockCode + " / " + stockTakesReportData.Descript : stockTakesReportData.Descript;
      stockTakesReportData.StockCode = isStockTypeF ? stockTakesReportData.KitItemStockCode : stockTakesReportData.StockCode;
      stockTakesReportData.Cost = isStockTypeF ? stockTakesReportData.StockCost ?? 0 : stockTakesReportData.Cost ?? 0;
      stockTakesReportData.SystemQuantity = !isStockTypeF ? stockTakesReportData.SystemQuantity ?? 0
                                          : (isStockTypeF && stockTakesReportData.GroupCount < 2) ? stockTakesReportData.InStock ?? 0
                                          : stockTakesReportData.SystemQuantity ?? 0;
      stockTakesReportData.QuantityDifference = stockTakesReportData.Quantity - stockTakesReportData.SystemQuantity;
      stockTakesReportData.QuantityDifferenceInDollar = stockTakesReportData.Cost * stockTakesReportData.QuantityDifference;
      stockTakesReportData.InStock ??= 0;
    }

    await CalculateAllSales(2, false);

    return stockTakesReportsData;
  }

  public async Task<bool> Process(StockProcessParametersDto stockProcessParameters)
  {
    IEnumerable<RecHead?> recHeads = await GetAllRecHeads(stockProcessParameters.Purpose);
    IEnumerable<RecLine> recLines = await GetAllRecLines(stockProcessParameters.Purpose);
    if (!recHeads.Any() || !recLines.Any())
    {
      return true;
    }

    int processed = 0;

    IEnumerable<KitItem> allKitItems = await GetAllKitItems();

    List<Task<bool>> tasks = new();
    List<StockHist> stockHists = new();
    List<Tuple<decimal, string>> listOfCostAndStockCodeTuples = new();
    List<(decimal inStock, decimal avail, string stockCode, string branch)> listOfInStockAndAvail = new();
    Dictionary<(string, int), RecLine> recLinesDictionary =
          recLines.ToDictionary(recLine => (recLine.StockCode ?? "", (int)recLine.RecNum), recLine => recLine);

    List<string?> stockCodes = recLines.Select(recLine => recLine.StockCode).ToList();
    List<string?> stockCodesForRecLines = new();

    Dictionary<string, List<KitItem>> kitItemsDictionary = new();
    Dictionary<string, List<int>> stockCodeRecNumberDictionary = new();

    foreach (var recLine in recLines)
    {
      string stockCode = recLine?.StockCode ?? "";
      stockCodesForRecLines.Add(stockCode);

      if (stockCodeRecNumberDictionary.ContainsKey(stockCode))
      {
        stockCodeRecNumberDictionary[stockCode].Add((int)recLine.RecNum);
      }
      else
      {
        List<int> newRecNumberList = new List<int> { (int)recLine.RecNum };
        stockCodeRecNumberDictionary.Add(stockCode, newRecNumberList);
      }

      List<KitItem>? kitItems = allKitItems.Where(item => item.KitCode == stockCode).ToList();
      stockCodes.AddRange(kitItems?.Select(kitItem => kitItem.StockCode) ?? new List<string?>());

      if (!kitItemsDictionary.ContainsKey(stockCode))
      {
        kitItemsDictionary.Add(stockCode, kitItems);
      }
    }

    IEnumerable<StockInfoDto?> stockInfos = await GetStockInfosByStockCodes(stockCodes);
    Dictionary<string, StockInfoDto?> stockInfoDictionary =
      stockInfos.ToDictionary(stockInfo => stockInfo.StockCode, stockInfo => stockInfo);

    List<StockCostDto?> listStockCosts = (await GetStockCostsByStockCodes(stockCodes)).ToList();
    List<StockCostDto?> listStockCostsForRecLines = (await GetStockCostsByStockCodes(stockCodesForRecLines)).ToList();
    List<StockCostAndRecLineDto> listStockCostAndRecLines = new List<StockCostAndRecLineDto>();

    foreach (StockCostDto? stockCostDto in listStockCostsForRecLines)
    {
      List<int> recNumberList = stockCodeRecNumberDictionary[stockCostDto.StockCode];
      StockCostAndRecLineDto tmpStockCostAndRecLineDto = new();

      tmpStockCostAndRecLineDto.StockCode = stockCostDto.StockCode;
      tmpStockCostAndRecLineDto.VendorId = stockCostDto.VendorId;
      tmpStockCostAndRecLineDto.Cost = stockCostDto.Cost;
      tmpStockCostAndRecLineDto.ActiveVendor = stockCostDto.ActiveVendor;
      tmpStockCostAndRecLineDto.RecNumber = recNumberList[0];

      listStockCostAndRecLines.Add(tmpStockCostAndRecLineDto);
      recNumberList.RemoveAt(0);
    }

    listStockCostAndRecLines = listStockCostAndRecLines
    .OrderBy(item => stockCodesForRecLines.IndexOf(item?.StockCode))
    .ToList();

    Dictionary<string, StockCostDto?> stockCostsDictionary =
      listStockCosts.ToDictionary(stockCost => stockCost.StockCode, stockCost => stockCost);

    foreach (var recLine in recLines)
    {
      string stockCode = recLine?.StockCode ?? "";
      List<KitItem>? kitItems = kitItemsDictionary.ContainsKey(stockCode)
        ? kitItemsDictionary[stockCode] : null;
      decimal quantityInKit = 0;

      StockCostDto? stockCost = stockCostsDictionary.TryGetValue(stockCode, out var tempStockCost)
        ? tempStockCost : null;

      if (kitItems.Any())
      {
        foreach (KitItem kitItem in kitItems)
        {
          StockInfoDto? stockInfo = stockInfoDictionary.ContainsKey(kitItem.StockCode) ? stockInfoDictionary[kitItem.StockCode] : null;
          decimal? quantity = (decimal?)kitItem.Quantity;
          quantityInKit = quantity ?? 0;
          decimal newCost = recLine?.Cost ?? 0;

          if (stockInfo != null)
          {
            if (stockProcessParameters.Purpose == (int)StockCountType.StockReceiving)
            {
              decimal inStock = (stockInfo.InStock ?? 0) + ((recLine?.Quantity * quantity) ?? 0);
              decimal avail = (stockInfo.Avail ?? 0) + ((recLine?.Quantity * quantity) ?? 0);

              stockInfo.InStock = inStock;
              stockInfo.Avail = avail;

              // CostType = Standard
              if (quantity != null && quantity != 0 && (recLine?.Quantity + quantityInKit) != 0)
              {
                newCost = ((((recLine?.Cost * recLine?.Quantity) / quantity) ?? 0) + (((stockCost?.Cost * quantityInKit) / (recLine?.Quantity + quantityInKit)) ?? 0));
              }
              else newCost = recLine?.Cost ?? 0;

              stockInfo.StandardCost = newCost;

              var existingInStockAndAvail = listOfInStockAndAvail.FirstOrDefault(item => item.stockCode == kitItem.StockCode);

              if (existingInStockAndAvail != default)
              {
                var updatedItem = (inStock, avail, kitItem.StockCode, stockInfo.Branch);
                int index = listOfInStockAndAvail.IndexOf(existingInStockAndAvail);
                listOfInStockAndAvail[index] = updatedItem;
              }
              else
              {
                listOfInStockAndAvail.Add((inStock, avail, kitItem.StockCode, stockInfo.Branch));
              }

              var existingCostAndStockCode = listOfCostAndStockCodeTuples.FirstOrDefault(item => item.Item2 == kitItem.StockCode);

              if (existingCostAndStockCode != null)
              {
                // Stock code already exists, update its data
                var updatedItem = Tuple.Create(stockInfo.StandardCost ?? 0, kitItem.StockCode);
                int index = listOfCostAndStockCodeTuples.IndexOf(existingCostAndStockCode);
                listOfCostAndStockCodeTuples[index] = updatedItem;
              }
              else
              {
                listOfCostAndStockCodeTuples.Add(Tuple.Create(stockInfo.StandardCost ?? 0, kitItem.StockCode));
              }
            }
            else if (stockProcessParameters.Purpose == (int)StockCountType.StockTakes && recLine?.Quantity != null)
            {
              listOfInStockAndAvail.Add((quantityInKit + (recLine.Quantity * quantity) ?? 0, quantityInKit + (recLine.Quantity * quantity) ?? 0, stockCode, stockInfo.Branch));
            }
          }
        }
      }
      else  // Not a kit
      {
        StockInfoDto? stockInfo = stockInfoDictionary.ContainsKey(stockCode) ? stockInfoDictionary[stockCode] : null;
        if (stockInfo != null)
        {
          if (stockProcessParameters.Purpose == (int)StockCountType.StockReceiving)
          {
            stockInfo.InStock = (stockInfo.InStock ?? 0) + recLine?.Quantity;
            stockInfo.Avail = (stockInfo.Avail ?? 0) + recLine?.Quantity;

            listOfInStockAndAvail.Add((stockInfo.InStock ?? 0, stockInfo.Avail ?? 0, stockCode, stockInfo.Branch));
            listOfCostAndStockCodeTuples.Add(new Tuple<decimal, string>(recLine?.Cost ?? 0, stockCode));
          }
          else if (stockProcessParameters.Purpose == (int)StockCountType.StockTakes && recLine?.Quantity != null)
          {
            listOfInStockAndAvail.Add((recLine?.Quantity ?? 0, recLine?.Quantity ?? 0, stockCode, stockInfo.Branch));
          }
        }
      }

      StockHist stockHist = new()
      {
        UserId = stockProcessParameters.UserCode,
        BatchDate = DateTime.Now,
        Batchid = recLine?.RecNum ?? 0,
        StockCode = recLine?.StockCode ?? "",
        Description = recLine?.Descript,
        Quantity = (int?)recLine?.Quantity,
        BookQty = (int?)recLine?.SysQty,
        Cost = recLine?.Cost,
        Amount = recLine?.Amount,
        Purpose = recLine?.Purpose,
        Sequence = recLine?.Sequence,
      };

      if (stockProcessParameters.Purpose == (int)StockCountType.StockReceiving)
      {
        stockHist.InvNum = recLine?.InvNum;
        stockHist.Assoc = recLine?.Assoc;
        stockHist.Taxes = recLine?.Taxes;
        stockHist.Total = recLine?.Total;

        stockHists.Add(stockHist);
      }
      else if (stockProcessParameters.Purpose == (int)StockCountType.StockTakes && recLine?.Quantity != null)
      {
        stockHist.DateCount = recLine?.DateCount;
        stockHist.InitialCount = recLine?.InitialCount;

        stockHists.Add(stockHist);
      }
    }

    await UpdateCostAndPrice(kitItemsDictionary,
                             listStockCostAndRecLines,
                             recLinesDictionary,
                             stockCostsDictionary,
                             stockProcessParameters.SetVendPolicy,
                             stockProcessParameters.UpdateNetworkPolicy,
                             stockProcessParameters.IsChangePrice,
                             stockProcessParameters.IsCostMarkupPolicy,
                             stockProcessParameters.NumPricePolicy,
                             stockProcessParameters.Purpose);

    for (int i = 0; i < listOfInStockAndAvail.Count; i += 1000)
    {
      List<(decimal inStock, decimal avail, string stockCode, string branch)> sublist =
        listOfInStockAndAvail.Skip(i).Take(1000).ToList();
      tasks.Add(UpdateInStockAndAvailInStockBrList(sublist));
    }

    for (int i = 0; i < listOfCostAndStockCodeTuples.Count; i += 1000)
    {
      List<Tuple<decimal, string>> sublist =
        listOfCostAndStockCodeTuples.Skip(i).Take(1000).ToList();
      tasks.Add(UpdatestockMastersStandardCostBatch(sublist));
    }

    for (int i = 0; i < stockHists.Count; i += 1000)
    {
      List<StockHist> sublist = stockHists.Skip(i).Take(1000).ToList();
      tasks.Add(AddStockHistBatch(sublist));
    }

    bool[] result = await Task.WhenAll(tasks);
    if (result.Count(x => x == true) > 0) processed++;
    processed += (await DeleteAllRecLineAndRecHead(stockProcessParameters.Purpose)) ? 1 : 0;

    _unitOfWork.Commit();
    return processed > 0;
  }

  private async Task<bool> UpdateCostAndPrice(
    Dictionary<string, List<KitItem>> kitItemsDictionary,
    List<StockCostAndRecLineDto?> stockCostsAndRecLines,
    Dictionary<(string, int), RecLine> recLinesDictionary,
    Dictionary<string, StockCostDto?> stockCostsDictionary, bool setVendPolicy, bool updateNetworkPolicy, bool isChangePrice, bool isCostMarkupPolicy, int numPricePolicy, int? purpose = null)
  {
    List<(double cost, string stockCode, string vendorId)> listOfCostStockCodeAndVendorId = new();
    List<(double cost, string stockCode, string vendorId)> listOfCostStockCodeAndVendorIdForPrice = new();
    List<Task<bool>> tasks = new();

    foreach (StockCostAndRecLineDto? stockCostsAndRecLine in stockCostsAndRecLines)
    {
      if (stockCostsAndRecLine == null) continue;

      string stockCode = stockCostsAndRecLine.StockCode;
      int recNumber = (int)stockCostsAndRecLine.RecNumber;

      RecLine? recLine = recLinesDictionary.ContainsKey((stockCode, recNumber)) ? recLinesDictionary[(stockCode, recNumber)] : null;
      decimal cost = recLine?.Cost ?? 0;

      List<KitItem>? kitItems = kitItemsDictionary.ContainsKey(stockCode)
        ? kitItemsDictionary[stockCode] : null;

      if (kitItems.Any())
      {
        kitItems = kitItemsDictionary[stockCode];
        foreach (KitItem kitItem in kitItems)
        {
          if (purpose == (int)StockCountType.StockTakes) continue;
          string? kitItemStockCode = kitItem.StockCode;
          StockCostDto? stockCostKit = stockCostsDictionary.TryGetValue(kitItemStockCode, out var tempStockCost)
            ? tempStockCost : null;

          decimal? quantity = (decimal?)kitItem.Quantity;
          decimal? quantityInKit = (!string.IsNullOrWhiteSpace(kitItemStockCode) &&
            recLinesDictionary.ContainsKey((kitItemStockCode, recNumber))
            ? recLinesDictionary[(kitItemStockCode, recNumber)].Quantity : 0);

          if (quantity != null && quantity != 0 && (recLine?.Quantity + quantityInKit) != 0)
          {
            cost = ((((((recLine?.Cost * recLine?.Quantity) / quantity) ?? 0) +
                                        (stockCostKit?.Cost * quantityInKit)) / (recLine?.Quantity + quantityInKit)) ?? 0).Round(2);
          }
          else
          {
            cost = recLine?.Cost ?? 0;
          }

          stockCostKit.Cost = cost;

          if (stockCostKit.VendorId == "ALL" && !setVendPolicy)
          {
            int existingIndex = listOfCostStockCodeAndVendorId.FindIndex(item => item.stockCode == kitItemStockCode && item.vendorId == "ALL");

            if (existingIndex != -1)
            {
              var existingItem = listOfCostStockCodeAndVendorId[existingIndex];
              listOfCostStockCodeAndVendorId[existingIndex] = ((double)cost, existingItem.stockCode, existingItem.vendorId);
            }
            else
            {
              listOfCostStockCodeAndVendorId.Add(((double)cost, kitItemStockCode, "ALL"));
            }
          }
          else if (stockCostKit.VendorId != "ALL" && stockCostKit.ActiveVendor == 1)
          {
            int existingIndex = listOfCostStockCodeAndVendorId.FindIndex(item => item.stockCode == kitItemStockCode && item.vendorId == stockCostsAndRecLine.VendorId);

            if (existingIndex != -1)
            {
              var existingItem = listOfCostStockCodeAndVendorId[existingIndex];
              listOfCostStockCodeAndVendorId[existingIndex] = ((double)cost, existingItem.stockCode, existingItem.vendorId);
            }
            else
            {
              listOfCostStockCodeAndVendorId.Add(((double)cost, kitItemStockCode, stockCostsAndRecLine.VendorId));
            }
            if (updateNetworkPolicy && !setVendPolicy)
            {
              existingIndex = listOfCostStockCodeAndVendorId.FindIndex(item => item.stockCode == kitItemStockCode && item.vendorId == "ALL");

              if (existingIndex != -1)
              {
                var existingItem = listOfCostStockCodeAndVendorId[existingIndex];
                listOfCostStockCodeAndVendorId[existingIndex] = ((double)cost, existingItem.stockCode, existingItem.vendorId);
              }
              else
              {
                listOfCostStockCodeAndVendorId.Add(((double)cost, kitItemStockCode, "ALL"));
              }
            }
          }

          listOfCostStockCodeAndVendorIdForPrice.Add(((double)cost, kitItemStockCode, stockCostKit.VendorId));
        }
      }
      else
      {
        if (stockCostsAndRecLine.VendorId == "ALL" && !setVendPolicy)
        {
          listOfCostStockCodeAndVendorId.Add(((double)cost, stockCode, "ALL"));
        }
        else if (stockCostsAndRecLine.VendorId != "ALL" && stockCostsAndRecLine.ActiveVendor == 1)
        {
          listOfCostStockCodeAndVendorId.Add(((double)cost, stockCode, stockCostsAndRecLine.VendorId));
          if (updateNetworkPolicy && !setVendPolicy)
          {
            listOfCostStockCodeAndVendorId.Add(((double)cost, stockCode, "ALL"));
          }
        }

        listOfCostStockCodeAndVendorIdForPrice.Add(((double)cost, stockCode, stockCostsAndRecLine.VendorId));
      }
    }

    for (int i = 0; i < listOfCostStockCodeAndVendorId.Count; i += 1000)
    {
      List<(double cost, string stockCode, string vendorId)> sublist
        = listOfCostStockCodeAndVendorId.Skip(i).Take(1000).ToList();
      tasks.Add(UpdateStockCostList(sublist));
    }

    bool[] result = await Task.WhenAll(tasks);
    bool priceUpdated = await UpdatePriceRecords(listOfCostStockCodeAndVendorIdForPrice, isChangePrice, isCostMarkupPolicy, updateNetworkPolicy, setVendPolicy, numPricePolicy);
    return result.Count(x => x == true) > 0 && priceUpdated;
  }

  private async Task<IEnumerable<RecHead?>> GetAllRecHeads(int purpose)
  {
    IEnumerable<RecHead?> recHeads;

    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      recHeads = await uow.StockTakesAndReceiving.GetAllRecHeadsAsync(purpose);
      uow.Commit();
    }
    return recHeads;
  }

  private async Task<IEnumerable<RecLine>> GetAllRecLines(int purpose)
  {
    IEnumerable<RecLine> recLines;

    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      recLines = await uow.StockTakesAndReceiving.GetAllRecLinesAsync(purpose);
      uow.Commit();
    }
    return recLines;
  }

  private async Task<IEnumerable<KitItem>> GetAllKitItems()
  {
    IEnumerable<KitItem> kitItem = new List<KitItem>();

    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      kitItem = await uow.KitMasters.GetAllKitItemsAsync();
      uow.Commit();
    }
    return kitItem;
  }

  private async Task<IEnumerable<StockInfoDto?>> GetStockInfosByStockCodes(List<string?> stockCodes)
  {
    IEnumerable<StockInfoDto?> stockInfos;

    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      stockInfos = await uow.StockTakesAndReceiving.GetStockInfosByStockCodesAsync(stockCodes);
      uow.Commit();
    }
    return stockInfos;
  }

  private async Task<bool> UpdatestockMastersStandardCostBatch(List<Tuple<decimal, string>> listOfCostAndStockCodeTuples)
  {
    bool isUpdated;

    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      isUpdated = await uow.StockTakesAndReceiving.UpdatestockMastersStandardCostBatchAsync(listOfCostAndStockCodeTuples);
      uow.Commit();
    }
    return isUpdated;
  }

  private async Task<IEnumerable<StockCostDto?>> GetStockCostsByStockCodes(List<string> stockCodes)
  {
    IEnumerable<StockCostDto?> stockInfo;

    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      stockInfo = await uow.StockTakesAndReceiving.GetStockCostByStockCodesAsync(stockCodes);
      uow.Commit();
    }
    return stockInfo;
  }

  private async Task<bool> UpdateStockCostList(List<(double cost, string stockCode, string vendorId)> costList)
  {
    bool isUpdated;
    List<StockCostInfoDto> stockCostInfoList = costList.Select(item => new StockCostInfoDto
    {
      Cost = (decimal?)item.cost,
      VendorId = item.vendorId,
      StockCode = item.stockCode,
    }).ToList();

    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      isUpdated = await uow.StockTakesAndReceiving.UpdateStockCostListAsync(stockCostInfoList);
      uow.Commit();
    }
    return isUpdated;
  }

  public async Task<bool> UpdatePriceRecords(List<(double cost, string stockCode, string vendorId)> listOfCostStockCodeAndVendorId, bool isPriceChange, bool isCostMarkupPolicy, bool updateNetworkPolicy, bool setVendPolicy, int numPricePolicy)
  {
    List<Task<bool>> addTasks = new();
    List<Task<int>> updateTasks = new();
    bool isUpdated = false;

    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      List<(string stockCode, string vendorId, int orderNo)> stockPriceParameters
        = listOfCostStockCodeAndVendorId
            .Select(item => (item.stockCode, item.vendorId, orderNo: 0))
            .ToList();
      List<StockPrice> stockPrices = (await uow.StockPrices.GetStockPricesListAsync(stockPriceParameters)).ToList();
      List<StockPrice> stockPricesForVendorAll = new();

      foreach (var stockPrice in stockPrices)
      {
        if (stockPrice == null || stockPrice.StockCode == null) continue;

        string stockCode = stockPrice.StockCode;
        string vendorId = stockPrice.VendorID ?? "";
        double cost = listOfCostStockCodeAndVendorId
                        .Where(item => item.stockCode.Trim() == stockCode.Trim() && item.vendorId == vendorId)
                        .FirstOrDefault()
                        .cost;
        if (cost == 0)
        {
          stockPrice.Margin ??= 0;
        }
        else
        {
          if (isPriceChange)
          {
            stockPrice.Price = CalculateSalePrice((decimal)cost, stockPrice.Margin ?? 0, isCostMarkupPolicy); ;
            stockPrice.Margin ??= 0;
          }
          else
          {
            stockPrice.Margin = CalculateMargin(stockPrice.Price, (decimal)cost, isCostMarkupPolicy);
          }
        }

        if (updateNetworkPolicy && !setVendPolicy)
        {
          stockPricesForVendorAll.Add(new StockPrice
          {
            Price = stockPrice.Price,
            Margin = stockPrice.Margin,
            StockCode = stockCode,
            PriceNumber = stockPrice.PriceNumber,
            VendorID = "ALL",
            OrderNo = 0,
            StartDate = null,
          });
        }
      }

      for (int i = 0; i < stockPrices.Count; i += 1000)
      {
        List<StockPrice> sublist = stockPrices.Skip(i).Take(1000).ToList();
        updateTasks.Add(uow.StockPrices.UpdateStockPriceListAsync(sublist));
      }

      for (int i = 0; i < stockPricesForVendorAll.Count; i += 1000)
      {
        List<StockPrice> sublist = stockPricesForVendorAll.Skip(i).Take(1000).ToList();
        updateTasks.Add(uow.StockPrices.UpdateStockPriceListAsync(sublist));
      }

      List<StockPrice> addStockPriceList = new();

      foreach ((double cost, string stockCode, string vendorId) in listOfCostStockCodeAndVendorId)
      {
        StockPrice? stockPrice =
          stockPrices
            .Where(item => item.StockCode.Trim() == stockCode.Trim() && item.VendorID == vendorId)
            .FirstOrDefault();
        if (stockPrice == null)
        {
          for (int i = 1; i <= numPricePolicy; i++)
          {
            stockPrice = new StockPrice
            {
              StockCode = stockCode,
              PriceNumber = i,
              Price = (decimal)cost,
              VendorID = vendorId == "" ? "ALL" : vendorId,
              OrderNo = 0,
              Margin = 0,
            };

            if (addStockPriceList.Count >= 1000)
            {
              addTasks.Add(uow.StockPrices.AddStockPriceListAsync(addStockPriceList));
              addStockPriceList.Clear();
            }
            addStockPriceList.Add(stockPrice);
          }
        }
      }
      addTasks.Add(uow.StockPrices.AddStockPriceListAsync(addStockPriceList));

      int[] rowsAffected = await Task.WhenAll(updateTasks);
      bool[] addResult = await Task.WhenAll(addTasks);
      isUpdated = addResult.Count(x => x == true) > 0;
      isUpdated = isUpdated || rowsAffected.Sum() > 0;
      uow.Commit();
    }

    return isUpdated;
  }

  private decimal CalculateSalePrice(decimal totalCost, decimal margin, bool isCostMarkup)
  {
    decimal calculatedSalePrice = 0;

    if (isCostMarkup)
    {
      calculatedSalePrice = totalCost * (1 + margin / 100);
    }
    else
    {
      if (margin >= 0 && margin < 100) // If margin > 100, sale price will be negative, instead of that I am making price as 0
      {
        calculatedSalePrice = totalCost / (1 - margin / 100);
      }
    }

    return calculatedSalePrice;
  }

  private decimal CalculateMargin(decimal storePrice, decimal storeCost, bool isCostMarkup)
  {
    decimal calculatedMargin = 0;

    if (isCostMarkup) // Original { Markup % = (SP - Cost) / cost}
    {
      if (storeCost != 0)
      {
        calculatedMargin = (storePrice - storeCost) / storeCost * 100;
      }
    }
    else // New { Margin % = (S.P-Cost)/ S.P}
    {
      if (storePrice != 0 && storeCost != 0)
      {
        calculatedMargin = (storePrice - storeCost) / storePrice * 100;
      }
    }

    return calculatedMargin;
  }

  private async Task<bool> AddStockHistBatch(List<StockHist> stockHists)
  {
    bool isAdded;

    using (var uow = new UnitOfWork(_config, _config.CSCTransConnectionString))
    {
      isAdded = await uow.StockTakesAndReceiving.AddStockHistBatchAsync(stockHists);
      uow.Commit();
    }
    return isAdded;
  }

  private async Task<bool> UpdateInStockAndAvailInStockBrList(List<(decimal inStock, decimal avail, string stockCode, string branch)> parameters)
  {
    bool isUpdated;

    using (var uow = new UnitOfWork(_config, _config.CSCTransConnectionString))
    {
      isUpdated = await uow.StockTakesAndReceiving.UpdateInStockAndAvailInStockBrListAsync(parameters);
      uow.Commit();
    }
    return isUpdated;
  }

  public async Task<IEnumerable<StockReceivingExportDto>?> GetStockReceivingExportData()
  {
    IEnumerable<StockReceivingExportDto>? stockReceivingExportData = await _unitOfWork.StockTakesAndReceiving.GetExportDataAsync((int)StockCountType.StockReceiving);
    List<Task<decimal?>> tasks = new List<Task<decimal?>>();

    foreach (var data in stockReceivingExportData)
    {
      tasks.Add(_unitOfWork.StockTakesAndReceiving.GetAssocAsync(data.StockCode));
    }

    decimal?[] assocs = await Task.WhenAll(tasks);
    int i = 0;
    foreach (var data in stockReceivingExportData)
    {
      data.Assoc = assocs[i];
      i++;
    }

    _unitOfWork.Commit();
    return stockReceivingExportData;
  }

  public async Task<List<string>> ImportFromFile(string text, string exceptedTax, int costDec, int purpose)
  {
    Header header = await GetHeaderByPurpose(purpose);
    short recNum = (short)(await GetBatchId(purpose, "TX"));
    int sequence = await GetSequence(recNum);

    ReclineDto recline = new ReclineDto
    {
      BatchId = recNum,
      Purpose = purpose,
      IsAllReclines = true
    };

    text = Regex.Replace(text, "\r", "");
    string[] lines = text.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

    List<Fields> fields = (await GetAllFieldsForStockTakesAndReceiving(purpose, header.ImportNumber, header.ImportType)).ToList();

    IEnumerable<StockItem> allStockItems = await GetAllStockMaster();
    IEnumerable<KitMast> allKitMasts = await GetAllKitMastsAsync();
    IEnumerable<StockBranch> allStockBranches = await GetAllStockBranchesAsync();
    IEnumerable<RecLine?> allReclines = await GetRecLines(recline);

    List<Task<bool>> addTasks = new List<Task<bool>>();
    List<Task<bool>> updateTasks = new List<Task<bool>>();
    List<Task<bool>> updateReclineTasks = new List<Task<bool>>();
    List<string> badLines = new List<string>();

    for (int i = 0; i < lines.Count(); i++)
    {
      bool isBadLine = false;

      string[] data = lines[i].Split(new[] { header?.Delimiter ?? "" }, StringSplitOptions.None);
      string line = "";

      //ConvertLineF ConvertLineD
      for (int j = 0; j < Math.Min(fields.Count(), data.Count()); j++)
      {
        if (string.IsNullOrEmpty(data[j]))
        {
          continue;
        }
        line += $"{fields[j].Name}={data[j]}|";
      }

      string? stockCode = null;
      string? invoice = "0";
      decimal cost = 0;
      decimal quantity = 0;
      decimal extraQuantity = 0;

      DateTime? endCount = null;
      StockItem? stockItem = null;
      StockBranch? stockBranch = null;

      for (int j = 0; j < fields.Count(); j++)
      {
        string fieldName = fields[j].Name + "=";
        int indexOfField = line.IndexOf(fieldName, StringComparison.OrdinalIgnoreCase);
        switch (fields[j].Name)
        {
          case "SC":
            if (indexOfField == -1)
            {
              badLines.Add(line + "StockCodeMissing");
              isBadLine = true;
              break;
            }
            else
            {
              stockCode = data[j];
              stockItem = allStockItems.FirstOrDefault(stock => string.Equals(stock.StockCode, stockCode, StringComparison.InvariantCultureIgnoreCase));
              stockBranch = allStockBranches.FirstOrDefault(branch => string.Equals(branch.StockCode, stockCode, StringComparison.InvariantCultureIgnoreCase));

              bool isStockCodeExist = stockItem != null;
              bool isStockTypeNotF = stockItem?.StockType != 'F';
              bool isKitExist = allKitMasts.Any(kitMast => string.Equals(kitMast.StockCode, stockCode, StringComparison.InvariantCultureIgnoreCase));
              if (!isStockCodeExist && !isKitExist)
              {
                badLines.Add(line + "StockCodeDoesNotExist");
                isBadLine = true;
                break;
              }
              else
              {
                if (!isStockCodeExist && isKitExist)
                {
                  badLines.Add(line + "StockCodeIsAKit");
                  isBadLine = true;
                  break;
                }
                else
                {
                  if (isKitExist && isStockTypeNotF)
                  {
                    badLines.Add(line + "StockCodeIsAKit");
                    isBadLine = true;
                    break;
                  }
                }

              }
            }
            break;
          case "QTY":
            if (indexOfField == -1)
            {
              badLines.Add(line + "QuantityMissing");
              isBadLine = true;
              break;
            }
            else
            {
              decimal.TryParse(data[j], out quantity);
            }
            break;
          case "EXTQTY":
            if (indexOfField == -1)
            {
              badLines.Add(line + "ExtraQuantityMissing");
              isBadLine = true;
              break;
            }
            else
            {
              decimal.TryParse(data[j], out extraQuantity);
            }
            break;
          case "INVOICE":
            if (indexOfField == -1)
            {
              badLines.Add(line + "InvoiceMissing");
              isBadLine = true;
              break;
            }
            else
            {
              if (decimal.TryParse(data[j], out decimal inv)) invoice = data[j];
            }
            break;
          case "COST":
            if (indexOfField == -1)
            {
              badLines.Add(line + "CostMissing");
              isBadLine = true;
              break;
            }
            else
            {
              decimal.TryParse(data[j], out cost);
            }
            break;
          case "ENDCOUNT":
            if (purpose == (int)StockCountType.StockTakes && header?.ImportType == "D")
            {
              if (indexOfField == -1)
              {
                badLines.Add(line + "DateMissing");
                isBadLine = true;
                break;
              }
              else
              {
                var cleanedDateString = data[j].Replace("'", "");

                if (DateTime.TryParseExact(cleanedDateString, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDateTime))
                {
                  endCount = parsedDateTime;
                }
                else
                {
                  endCount = null;
                }
              }
            }
            break;
        }

        if (isBadLine)
        {
          break;
        }
      }

      if (!isBadLine)
      {
        string storeId = await GetStore();
        RecLine recLine = new RecLine
        {
          RecNum = recNum,
          Branch = storeId,
          InvNum = invoice,
          StockCode = stockCode,
          Descript = stockItem?.Description,
          Cost = purpose == (int)StockCountType.StockTakes ? cost
                 : cost != 0 ? cost : stockItem?.Cost,
          SysQty = purpose == (int)StockCountType.StockReceiving ? 0
                   : stockBranch?.InStock ?? 0,
          Quantity = purpose == (int)StockCountType.StockReceiving ? quantity
                     : (quantity + extraQuantity),
          Purpose = purpose,
          RecUpdate = true,
          Sequence = await GetSequence(recNum),
          DateCount = endCount
        };

        ReclineDto reclineDto = new ReclineDto
        {
          StockCode = stockCode ?? "",
          Cost = recLine.Cost ?? 0,
          Quantity = recLine.Quantity ?? 0,
          Sequence = recLine.Sequence ?? 0,
          FromText = true,
          BatchId = recNum,
          ExceptedTax = exceptedTax,
          CostDec = costDec,
          Purpose = purpose,
          DateCount = endCount
        };

        var reclineData = allReclines.FirstOrDefault(recline => string.Equals(recline.StockCode, stockCode, StringComparison.InvariantCultureIgnoreCase));

        if (purpose == (int)StockCountType.StockTakes && reclineData != null)
        {
          updateReclineTasks.Add(UpdateQuantity(reclineDto));
        }
        else
        {
          addTasks.Add(AddRecLine(recLine));
        }

        updateTasks.Add(UpdateTotal(reclineDto));
      }
    }

    await Task.WhenAll(updateReclineTasks);
    await Task.WhenAll(addTasks);

    if (purpose == (int)StockCountType.StockTakes)
    {
      await CalculateAllSales(purpose, true, true);
      await DeleteAllRecLine(purpose, true);
    }
    else
    {
      await Task.WhenAll(updateTasks);
    }

    _unitOfWork.Commit();
    return badLines;
  }

  private async Task<Header> GetHeaderByPurpose(int purpose)
  {
    Header header;
    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      header = await uow.StockTakesAndReceiving.GetHeaderByPurposeAsync(purpose);
      uow.Commit();
    }
    return header;
  }

  private async Task<IEnumerable<Fields>> GetAllFieldsForStockTakesAndReceiving(int purpose, int importNumber, string? importType)
  {
    IEnumerable<Fields> fields;

    if (purpose == (int)StockCountType.StockTakes && importType == "D")
    {
      var lastItem = await GetLastItem(importNumber);
      fields = await GetFieldsByImportNumberAndImportType(importNumber, lastItem);
    }
    else
    {
      fields = await GetFieldsByImportNumber(importNumber);
    }

    return fields;
  }

  private async Task<IEnumerable<Fields>> GetFieldsByImportNumber(int importNumber)
  {
    IEnumerable<Fields> fields;
    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      fields = await uow.StockTakesAndReceiving.GetFieldsByImportNumberAsync(importNumber);
      uow.Commit();
    }
    return fields;
  }

  private async Task<int> GetLastItem(int importNumber)
  {
    int lastItem;
    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      lastItem = await uow.StockTakesAndReceiving.GetLastItemAsync(importNumber);
      uow.Commit();
    }
    return lastItem;
  }

  private async Task<IEnumerable<Fields>> GetFieldsByImportNumberAndImportType(int importNumber, int lastItem)
  {
    IEnumerable<Fields> fields;
    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      fields = await uow.StockTakesAndReceiving.GetFieldsByImportNumberAndImportTypeAsync(lastItem, importNumber);
      uow.Commit();
    }
    return fields;
  }

  private async Task<IEnumerable<StockItem>> GetAllStockMaster()
  {
    IEnumerable<StockItem> stockItems;
    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      stockItems = await uow.StockItems.GetAllStockMasterAsync();
      uow.Commit();
    }
    return stockItems;
  }

  private async Task<IEnumerable<KitMast>> GetAllKitMastsAsync()
  {
    IEnumerable<KitMast> kitMasts;
    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      kitMasts = await uow.KitMasters.GetAllKitMastsAsync();
      uow.Commit();
    }
    return kitMasts;
  }

  private async Task<IEnumerable<StockBranch>> GetAllStockBranchesAsync()
  {
    IEnumerable<StockBranch> stockBranches;
    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      stockBranches = await uow.StockBranches.GetAllStockBranchAsync();
      uow.Commit();
    }
    return stockBranches;
  }

  public async Task<bool> GetCountOfInventoryDataForMonth(int month)
  {
    var count = await _unitOfWork.StockTakesAndReceiving.GetCountOfInventoryDataForMonth(month);

    var result = count == 0 ? false : true;
    return result;
  }

  private async Task<StockCostDto?> GetCostForSales(string stockCode)
  {
    StockCostDto? costInfo;

    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      costInfo = await uow.StockTakesAndReceiving.GetCostInfoForSalesAsync(stockCode);
      uow.Commit(); ;
    }

    return costInfo;
  }

  private async Task<int> UpdateReclineForSales(string costType, int purpose, decimal cost, string stockCode)
  {
    int isAdded;
    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      isAdded = await uow.StockTakesAndReceiving.UpdateReclineForSalesAsync(stockCode, purpose, cost, costType);
      uow.Commit();
    }

    return isAdded;
  }

  private async Task<RecLine?> GetReclineByStockCode(string stockCode, int purpose)
  {
    RecLine? recLine;

    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      recLine = await uow.StockTakesAndReceiving.GetReclineByStockCOde(stockCode, purpose);
      uow.Commit();
    }

    return recLine;
  }

  private async Task<RecLine?> GetReclineBybatchid(string stockCode, int purpose, int batchId)
  {
    RecLine? recLine;

    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      recLine = await uow.StockTakesAndReceiving.GetReclineByBatchIdAsync(stockCode, purpose, batchId);
      uow.Commit();
    }

    return recLine;
  }

  private async Task<decimal> CalculateSalesForQuantity(string stockCode, DateTime dateCount)
  {
    decimal salesQuantityForTrans;
    decimal salesQuantityFortills;
    decimal salesQuantity;

    using (var uow = new UnitOfWork(_config, _config.CSCTransConnectionString))
    {
      salesQuantityForTrans = await uow.StockTakesAndReceiving.CalculateSalesQuantity(stockCode, dateCount);
      uow.Commit();
    }

    using (var uow = new UnitOfWork(_config, _config.CSCTillsConnectionString))
    {
      salesQuantityFortills = await uow.StockTakesAndReceiving.CalculateSalesQuantity(stockCode, dateCount);
      uow.Commit();
    }

    salesQuantity = salesQuantityForTrans + salesQuantityFortills;

    return salesQuantity;
  }

  private async Task<Dictionary<string, decimal>> GetQuantityByStockCodeDictionary(List<(string StockCode, DateTime DateCount)> stockCodeAndDateList)
  {
    List<(string stockCode, decimal quantity)> quantityListForTrans = new();
    List<(string stockCode, decimal quantity)> quantityListForTills = new();

    using (var uow = new UnitOfWork(_config, _config.CSCTransConnectionString))
    {
      quantityListForTrans = await uow.StockTakesAndReceiving.GetSalesQuantityListAsync(stockCodeAndDateList) ?? quantityListForTrans;
      uow.Commit();
    }

    using (var uow = new UnitOfWork(_config, _config.CSCTillsConnectionString))
    {
      quantityListForTills = await uow.StockTakesAndReceiving.GetSalesQuantityListAsync(stockCodeAndDateList) ?? quantityListForTills;
      uow.Commit();
    }

    var combinedList = quantityListForTrans.Concat(quantityListForTills);

    // Group items by stock code and calculate the sum of quantities for each group
    return combinedList
        .GroupBy(item => item.stockCode)
        .Select(group => (group.Key, quantity: group.Sum(item => item.quantity)))
        .ToDictionary(item => item.Key, item => item.quantity);
  }

  private async Task<int> UpdateReclineForSalesQuantityAsync(ReclineDto reclineDto)
  {
    int rowsAffected;

    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      rowsAffected = await uow.StockTakesAndReceiving.UpdateReclineForSalesQuantityAsync(reclineDto);
      uow.Commit();
    }

    return rowsAffected;
  }

  private async Task<bool> UpdateQuantity(ReclineDto reclineDto)
  {
    int isUpdated;

    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      isUpdated = await uow.StockTakesAndReceiving.UpdateQuantityAsync(reclineDto);
      uow.Commit();
    }

    return isUpdated > 0;
  }

  public async Task<int> GetNotPrintedRecLinesCount(int purpose)
  {
    int result = 0;

    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      result = await uow.StockTakesAndReceiving.GetNotPrintedRecLinesCount(purpose);
      uow.Commit();
    }

    return result;
  }

  public async Task<StockTakesExportDto> GetStockTakesExportData()
  {
    StockTakesExportDto stockTakesExportData = new();
    stockTakesExportData.Stocks = await _unitOfWork.StockTakesAndReceiving.GetExportDataAsync((int)StockCountType.StockTakes);
    stockTakesExportData.PluMasts = await _unitOfWork.PLUMasts.GetPluMastsForStockTakes();

    _unitOfWork.Commit();
    return stockTakesExportData;
  }

  public async Task<bool> UpdateInvoiceNumber(ReclineDto reclineDto)
  {
    bool isUpdated = false;

    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      isUpdated = await uow.StockTakesAndReceiving.UpdateInvoiceNumber(reclineDto);
      uow.Commit();
    }

    return isUpdated;
  }
}
