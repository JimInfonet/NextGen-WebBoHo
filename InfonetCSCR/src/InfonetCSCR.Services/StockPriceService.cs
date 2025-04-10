namespace InfonetCSCR.Services;

public class StockPriceService : IStockPriceService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IApplicationConfig _config;

  public StockPriceService(IApplicationConfig config)
  {
    this._unitOfWork = new UnitOfWork(config, config.CSCMasterConnectionString);
    _config = config;
  }

  public async Task<StockPriceFormDto?> GetStockPrice(StockPriceDto model, bool isUseARInvoicePolicy)
  {
    if (model.StockCode == null)
    {
      throw new Exception("Stock Code not Found");
    }

    var vendors = await _unitOfWork.StockPrices.GetVendors(model);
    var item = await _unitOfWork.StockItems.GetStockItemByCode(model.StockCode);
    var costs = await _unitOfWork.StockPrices.GetCosts(model);
    var price = await _unitOfWork.StockPrices.GetPrices(model, model.PricesInUse, $"'{model.StoreID}'");
    var rebates = await _unitOfWork.StockPrices.GetRebate(model.StockCode, model.OrderNo, $"'{model.StoreID}'");
    var promotion = await _unitOfWork.StockPrices.GetPromotion(model.StockCode);

    if (model.PriceType == null)
    {
      model.PriceType = "R";
    }

    IEnumerable<StockSpecialPrice>? specialPrice, specialPriceFromPriceL = null;
    if (isUseARInvoicePolicy)
    {
      specialPrice = await _unitOfWork.StockPrices.GetSpecialPriceFromDiskKL(model);
      specialPriceFromPriceL = await _unitOfWork.StockPrices.GetSpecialPrice(model, $"StoreID");
    }
    else
    {
      specialPrice = await _unitOfWork.StockPrices.GetSpecialPrice(model, $"'{model.StoreID}'");
    }

    var stockPrice = new StockPriceFormDto()
    {
      StockItemCode = model.StockCode,
      StockItemName = item.Description,
      Packing = item.Packing,
      Vendors = vendors,
      StockCosts = costs,
      StockPrices = price,
      StockPriceRebates = rebates,
      SpecialPrice = specialPrice,
      SpecialPriceFromPriceL = specialPriceFromPriceL,
      Promotion = promotion,
    };

    _unitOfWork.Commit();
    return stockPrice;
  }

  public async Task<StockPriceInfo> GetStockPriceInfo(GetStockPriceInfoDto model)
  {
    StockPriceInfo stockPriceInfo = await _unitOfWork.StockPrices.GetStockPriceInfoAsync(model);
    _unitOfWork.Commit();
    return stockPriceInfo;
  }

  public async Task<IEnumerable<StockPriceInfo>> GetStockPriceInfoBatch(List<GetStockPriceInfoDto> models)
  {
    List<Task<IEnumerable<StockPriceInfo>>> tasks = new List<Task<IEnumerable<StockPriceInfo>>>();
    int startIndex = 0;
    int SLICE_SIZE = 1000;
    int total = models.Count();
    while (startIndex < total)
    {
      List<GetStockPriceInfoDto> slice = models.GetRange(startIndex, Math.Min(SLICE_SIZE, total - startIndex));
      tasks.Add(_unitOfWork.StockPrices.GetStockPriceInfoBatchAsync(slice));
      startIndex += SLICE_SIZE;
    }
    IEnumerable<StockPriceInfo>[] result = await Task.WhenAll(tasks);
    _unitOfWork.Commit();
    IEnumerable<StockPriceInfo> stockPrices = result.SelectMany(x => x);
    return stockPrices;
  }

  public async Task<IEnumerable<StockPriceVendors>?> UpdateVendorPartNo(VendorPart model)
  {
    var stockCosts = await _unitOfWork.StockPrices.GetStockCosts(model, $"'{model.StoreID}'");
    int numOfRows = 0;
    if (stockCosts != null)
    {
      var vendorPart = await _unitOfWork.StockPrices.GetVendorPart(model.StockCode, model.VendorID);
      if (vendorPart == null || vendorPart.StockCode == null)
      {
        numOfRows += await _unitOfWork.StockPrices.InsertVendorPart(model.StockCode, model.VendorID, model.PartNo);
      }
      else
      {
        numOfRows += await _unitOfWork.StockPrices.UpdateVendorPart(model.StockCode, model.VendorID, model.PartNo);
      }
    }
    else
    {
      throw new Exception("Vendor must add to stock cost first.");
    }

    if (!_config.IsBackOffice)
    {
      numOfRows += await _unitOfWork.StockPrices.UpdateStockMasterTransferFlag(model.StockCode);
    }

    if (numOfRows == 0)
    {
      throw new Exception("Failed to update vendor part.");
    }

    IEnumerable<StockPriceVendors> stockPriceVendors = await _unitOfWork.StockPrices.GetVendors(model);
    _unitOfWork.Commit();
    return stockPriceVendors;
  }

  public async Task<IEnumerable<StockPriceVendors>?> AddVendorToStockCost(StockCosts model)
  {
    var storeIDs = string.Empty;
    if (!_config.IsBackOffice)
    {
      var ids = new List<string>();
      if (model.StoreIDs != null && model.StoreIDs.Count > 0)
      {
        foreach (var id in model.StoreIDs)
        {
          ids.Add($@"'{id}'");
        }
      }

      storeIDs = ids.Count > 0 ? String.Join(", ", ids) : "";
    }

    var stockCosts = await _unitOfWork.StockPrices.GetStockCosts(model, storeIDs);
    var stockCostsForAllVendor = await _unitOfWork.StockPrices.GetStockCosts(model, storeIDs, $@"'ALL'");
    var defaultCost = stockCostsForAllVendor.FirstOrDefault(x => x.VendorID == "ALL")?.Cost;
    int numOfRows = 0;

    if (_config.IsBackOffice)
    {
      if (!stockCosts.Any())
      {
        numOfRows += await _unitOfWork.StockPrices.InsertStockCosts(model, string.Empty, "", model.ActiveVendor ?? false);
      }
    }
    else
    {
      List<Task<int>> tasks = new List<Task<int>>();
      for (int i = 0; i < model.StoreIDs?.Count; i++)
      {
        var stockCostWithStore = stockCosts.FirstOrDefault(x => x.StoreID == model.StoreIDs[i]);
        if (stockCostWithStore == null)
        {
          tasks.Add(_unitOfWork.StockPrices.InsertStockCosts(model, $"'{model.StoreIDs[i]}'", "", model.ActiveVendor ?? false));
        }
      }
      int[] results = await Task.WhenAll(tasks);
      numOfRows = results.Sum();
    }

    if (numOfRows == 0)
    {
      throw new Exception("Failed to set active vendor.");
    }

    IEnumerable<StockPriceVendors> stockPriceVendors = await _unitOfWork.StockPrices.GetVendors(model);
    _unitOfWork.Commit();
    return stockPriceVendors;
  }

  public async Task<IEnumerable<StockPriceVendors>?> DeleteVendor(StockPriceModel model)
  {
    var storeIDs = string.Empty;
    if (!_config.IsBackOffice)
    {
      var ids = new List<string>();
      if (model.StoreIDs != null && model.StoreIDs.Count > 0)
      {
        foreach (var id in model.StoreIDs)
        {
          ids.Add($@"'{id}'");
        }
      }

      storeIDs = ids.Count > 0 ? String.Join(", ", ids) : "";
    }

    var vendors = await _unitOfWork.StockPrices.GetVendors(model);
    var vendor = vendors.FirstOrDefault(x => x.Code == model.VendorID);

    int numOfRows = 0;
    numOfRows += await _unitOfWork.StockPrices.DeleteVendorpart(model);
    numOfRows += await _unitOfWork.StockPrices.DeleteSpecialPriceForVendor(model, storeIDs);
    numOfRows += await _unitOfWork.StockPrices.DeleteStockPricesForVendor(model, storeIDs);
    numOfRows += await _unitOfWork.StockPrices.DeleteStockRebatesForVendor(model, storeIDs);
    numOfRows += await _unitOfWork.StockPrices.DeleteStockCostsForVendor(model, storeIDs);
    numOfRows += await _unitOfWork.StockPrices.UpdateProductTaxExemptForVendor(model);

    if (vendor.ActiveVendor)
    {
      numOfRows += await _unitOfWork.StockPrices.UpdateStockMasterForVendor(model);
    }

    if (!_config.IsBackOffice)
    {
      numOfRows += await _unitOfWork.StockPrices.UpdateStockMasterTransferFlag(model.StockCode);
    }

    if (numOfRows == 0)
    {
      throw new Exception("Failed to delete vendor.");
    }

    IEnumerable<StockPriceVendors> stockPriceVendors = await _unitOfWork.StockPrices.GetVendors(model);
    _unitOfWork.Commit();
    return stockPriceVendors;
  }

  public async Task<IEnumerable<StockPriceVendors>?> ToggleVendorStatus(StockPriceModel model)
  {
    var storeIDs = string.Empty;
    if (!_config.IsBackOffice)
    {
      var ids = new List<string>();
      if (model.StoreIDs != null && model.StoreIDs.Count > 0)
      {
        foreach (var id in model.StoreIDs)
        {
          ids.Add($@"'{id}'");
        }
      }

      storeIDs = ids.Count > 0 ? String.Join(", ", ids) : "";
    }

    int numOfRows = 0;
    var stockCostList = await _unitOfWork.StockPrices.GetStockCosts(model, storeIDs);
    numOfRows += await _unitOfWork.StockPrices.UpdateAllVendorInActive(model, storeIDs);
    var stockCosts = stockCostList.FirstOrDefault();
    if (!_config.IsBackOffice)
    {
      stockCosts = stockCostList.FirstOrDefault(x => x.StoreID == model.StoreID);
    }

    numOfRows += await _unitOfWork.StockPrices.ToggleVendorStatus(model, storeIDs, stockCosts.ActiveVendor ?? false);

    var vendors = await _unitOfWork.StockPrices.GetVendors(model);
    if (model.OrderNo == 0)
    {
      var activeVendor = vendors.FirstOrDefault(x => x.ActiveVendor);
      if (activeVendor == null)
      {
        model.VendorID = "ALL";
      }
      numOfRows += await _unitOfWork.StockPrices.UpdateStockMasterForActiveVendor(model);
    }

    if (!_config.IsBackOffice)
    {
      numOfRows += await _unitOfWork.StockPrices.UpdateStockMasterTransferFlag(model.StockCode);
    }

    if (numOfRows == 0)
    {
      throw new Exception("Failed to set active vendor.");
    }

    IEnumerable<StockPriceVendors> stockPriceVendors = vendors;
    _unitOfWork.Commit();
    return stockPriceVendors;
  }

  public async Task<IEnumerable<StockCosts>?> UpdateStockCost(StockCosts model)
  {
    string storeIDs = string.Empty, vendorIDs = string.Empty;

    if (model.VendorIDs?.Count == 0)
    {
      model.VendorIDs = new List<string> { model.VendorID ?? "ALL" };
    }

    if (!_config.IsBackOffice)
    {
      var ids = new List<string>();
      if (model.StoreIDs != null && model.StoreIDs.Count > 0)
      {
        foreach (var id in model.StoreIDs)
        {
          ids.Add($@"'{id}'");
        }
      }
      storeIDs = ids.Count > 0 ? String.Join(", ", ids) : "";

      ids = new List<string>();
      if (model.VendorIDs != null && model.VendorIDs.Count > 0)
      {
        foreach (var id in model.VendorIDs)
        {
          ids.Add($@"'{id}'");
        }
      }
      vendorIDs = ids.Count > 0 ? String.Join(", ", ids) : "";

    }

    var stockCosts = await _unitOfWork.StockPrices.GetStockCosts(model, storeIDs, vendorIDs);
    int numOfRows = 0;

    if (model.VendorIDs?.Count == 0)
    {
      model.VendorIDs = new List<string> { model.VendorID ?? "ALL" };
    }

    if (_config.IsBackOffice)
    {
      if (!stockCosts.Any())
      {
        await _unitOfWork.StockPrices.UpdateStockMasterCost(model.StockCode, model.VendorID, model.Cost);
        numOfRows += await _unitOfWork.StockPrices.InsertStockCostsBatch(model, model.VendorIDs, model.StoreIDs);
      }
      else
      {
        List<Task<int>> tasks = new List<Task<int>>();
        for (int i = 0; i < model.VendorIDs?.Count; i++)
        {
          if (stockCosts?.FirstOrDefault()?.ActiveVendor == true || stockCosts?.FirstOrDefault()?.VendorID == "ALL")
          {
            tasks.Add(_unitOfWork.StockPrices.UpdateStockMasterCost(model.StockCode, model.VendorIDs[i], model.Cost));
          }

          tasks.Add(_unitOfWork.StockPrices.UpdateStockCost(model, string.Empty, $"'{model.VendorIDs[i]}'"));
        }

        int[] results = await Task.WhenAll(tasks);
        numOfRows = results.Sum();
      }
    }
    else
    {
      List<Task<int>> tasks = new List<Task<int>>();
      List<StockCostsBatch> toBeUpdated = new List<StockCostsBatch>();

      for (int i = 0; i < model.VendorIDs?.Count; i++)
      {
        for (int j = 0; j < model.StoreIDs?.Count; j++)
        {
          var stockCostWithStore = stockCosts
            .FirstOrDefault(x => x.VendorID == model.VendorIDs[i] && x.StoreID == model.StoreIDs[j]);
          if (stockCostWithStore == null)
          {
            tasks.Add(_unitOfWork.StockPrices.InsertStockCosts(model, $"'{model.StoreIDs[j]}'", model.VendorIDs[i], model.ActiveVendorCode == model.VendorIDs[i] ? true : false));
          }
          else
          {
            if (stockCostWithStore.ActiveVendor ?? false)
            {
              tasks.Add(_unitOfWork.StockPrices.UpdateStockMasterCost(model.StockCode, model.VendorIDs[i], model.Cost));
            }

            toBeUpdated.Add(new StockCostsBatch
            {
              vendorId = model.VendorIDs[i],
              storeId = model.StoreIDs[j],
            });
          }
        }
      }
      tasks.Add(_unitOfWork.StockPrices.UpdateStockCostBatch(model, toBeUpdated));

      int[] results = await Task.WhenAll(tasks);
      numOfRows = results.Sum();
    }

    if (numOfRows == 0)
    {
      throw new Exception("Failed to update stock cost.");
    }

    IEnumerable<StockCosts> resultStockCosts = await _unitOfWork.StockPrices.GetCosts(model);
    _unitOfWork.Commit();
    return resultStockCosts;
  }

  public async Task<IEnumerable<StockCosts>?> InsertFutureStockCosts(StockCostsList models)
  {
    StockCosts CostWithActiveVendor = null;
    int numOfRows = 0;

    List<Task<int>> tasks = new List<Task<int>>();
    if (_config.IsBackOffice)
    {
      foreach (var m in models.Costs)
      {
        tasks.Add(_unitOfWork.StockPrices.InsertStockCosts(m, $"'{m.StoreID}'", m.VendorID, m.ActiveVendor ?? false));

        if (m.VendorID == "ALL" && CostWithActiveVendor == null)
        {
          CostWithActiveVendor = m;
        }
        if (m.ActiveVendor ?? false)
        {
          CostWithActiveVendor = m;
        }
      }
    }
    else
    {
      foreach (var m in models.Costs)
      {
        foreach (var storeId in models.StoreIDs)
        {
          tasks.Add(_unitOfWork.StockPrices.InsertStockCosts(m, $"'{storeId}'", m.VendorID, m.ActiveVendor ?? false));

          if (m.VendorID == "ALL" && CostWithActiveVendor == null)
          {
            CostWithActiveVendor = m;
          }
          if (m.ActiveVendor ?? false)
          {
            CostWithActiveVendor = m;
          }
        }
      }
    }

    int[] results = await Task.WhenAll(tasks);
    numOfRows = results.Sum();

    if (numOfRows == 0)
    {
      throw new Exception("Failed to insert future stock cost.");
    }
    if (CostWithActiveVendor == null) CostWithActiveVendor = models.Costs.FirstOrDefault();
    IEnumerable<StockCosts> resultStockCosts = await _unitOfWork.StockPrices.GetCosts(CostWithActiveVendor);
    _unitOfWork.Commit();
    return resultStockCosts;
  }

  public async Task<IEnumerable<StockPrice>?> UpdateStockPrice(PriceModel model)
  {
    var storeIDs = string.Empty;

    if (model.VendorIDs?.Count == 0)
    {
      model.VendorIDs = new List<string> { model.VendorID ?? "ALL" };
    }

    if (!_config.IsBackOffice)
    {
      var ids = new List<string>();
      if (model.StoreIDs != null && model.StoreIDs.Count > 0)
      {
        foreach (var id in model.StoreIDs)
        {
          ids.Add($@"'{id}'");
        }
      }

      storeIDs = ids.Count > 0 ? String.Join(", ", ids) : "";
    }

    var prices = await _unitOfWork.StockPrices.GetPrices(model, model.PricesInUse, storeIDs);
    int numOfRows = 0;

    if (model.VendorIDs?.Count == 0)
    {
      model.VendorIDs = new List<string> { model.VendorID ?? "ALL" };
    }

    if (_config.IsBackOffice && model.Prices != null)
    {
      List<Task<int>> tasks = new List<Task<int>>();
      foreach (var newPrice in model.Prices)
      {
        for (int i = 0; i < model.VendorIDs?.Count; i++)
        {
          newPrice.VendorID = model.VendorIDs[i];
          var price = prices.FirstOrDefault(x => x.PriceNumber == newPrice.PriceNumber && x.VendorID == newPrice.VendorID);
          if (price == null)
          {
            tasks.Add(_unitOfWork.StockPrices.InsertStockPrice(newPrice));
          }
          else
          {
            tasks.Add(_unitOfWork.StockPrices.UpdateStockPrice(newPrice));
          }
        }
      }
      int[] results = await Task.WhenAll(tasks);
      numOfRows = results.Sum();
    }
    else if (model.Prices != null)
    {
      List<Task<int>> tasks = new List<Task<int>>();

      foreach (var newPrice in model.Prices)
      {
        List<StockPrice> toBeUpdatedPrices = new List<StockPrice>();
        List<string> toBeInsertedStores = new List<string>();
        List<string> toBeInsertedVenors = new List<string>();
        int insertCounter = 0;
        foreach (var storeID in model.StoreIDs ?? new List<string>())
        {
          for (int i = 0; i < model.VendorIDs?.Count; i++)
          {
            newPrice.VendorID = model.VendorIDs[i];
            var price = prices.FirstOrDefault(x => x.PriceNumber == newPrice.PriceNumber && x.VendorID == newPrice.VendorID && x.StoreID == storeID);
            if (price == null)
            {
              newPrice.StoreID = storeID;
              toBeInsertedStores.Add(newPrice.StoreID);
              toBeInsertedVenors.Add(newPrice.VendorID);
              insertCounter++;
              if (insertCounter == 1000)
              {
                tasks.Add(_unitOfWork.StockPrices.InsertStockPriceBatch(newPrice, toBeInsertedStores, toBeInsertedVenors));
                toBeInsertedStores.Clear();
                toBeInsertedVenors.Clear();
                insertCounter = 0;
              }
            }
            else
            {
              newPrice.StoreID = storeID;

              // To prevent query execution error, we kept the data size for a single transaction
              // of a maximum of 1000 rows.
              if (toBeUpdatedPrices.Count == 1000)
              {
                tasks.Add(_unitOfWork.StockPrices.UpdateStockPriceBatch(toBeUpdatedPrices));
                toBeUpdatedPrices.Clear();
              }

              toBeUpdatedPrices.Add(new StockPrice
              {
                PriceNumber = newPrice.PriceNumber,
                VendorID = newPrice.VendorID,
                StoreID = newPrice.StoreID,
                StockCode = newPrice.StockCode,
                OrderNo = newPrice.OrderNo,
                Price = newPrice.Price,
                Margin = newPrice.Margin,
                StartDate = newPrice.StartDate,
                EndDate = newPrice.EndDate
              });
            }
          }
        }
        if (insertCounter > 0)
        {
          tasks.Add(_unitOfWork.StockPrices.InsertStockPriceBatch(newPrice, toBeInsertedStores, toBeInsertedVenors));
        }
        tasks.Add(_unitOfWork.StockPrices.UpdateStockPriceBatch(toBeUpdatedPrices));
      }

      int[] results = await Task.WhenAll(tasks);
      numOfRows = results.Sum();
    }

    if (!_config.IsBackOffice)
    {
      numOfRows += await _unitOfWork.StockPrices.UpdateStockMasterTransferFlag(model.StockCode);
    }

    if (numOfRows == 0)
    {
      throw new Exception("Failed to update stock price.");
    }

    IEnumerable<StockPrice> stockPrices = await _unitOfWork.StockPrices.GetPrices(model, model.PricesInUse, $"'{model.StoreID}'");
    _unitOfWork.Commit();
    return stockPrices;
  }

  public async Task<IEnumerable<StockPrice>?> InsertFuturePrice(PriceModel model)
  {
    int numOfRows = 0;

    if (model.VendorIDs?.Count == 0)
    {
      model.VendorIDs = new List<string> { model.VendorID ?? "ALL" };
    }

    if (_config.IsBackOffice && model.Prices != null)
    {
      List<Task<int>> tasks = new List<Task<int>>();
      foreach (var newPrice in model.Prices)
      {
        for (int i = 0; i < model.VendorIDs?.Count; i++)
        {
          if (newPrice.VendorID != model.VendorIDs[i]) continue;
          newPrice.VendorID = model.VendorIDs[i];
          tasks.Add(_unitOfWork.StockPrices.InsertStockPrice(newPrice));
        }
      }
      int[] results = await Task.WhenAll(tasks);
      numOfRows = results.Sum();
    }
    else if (model.Prices != null)
    {
      List<Task<int>> tasks = new List<Task<int>>();

      foreach (var newPrice in model.Prices)
      {
        List<StockPrice> toBeUpdatedPrices = new List<StockPrice>();
        foreach (var storeID in model.StoreIDs ?? new List<string>())
        {
          for (int i = 0; i < model.VendorIDs?.Count; i++)
          {
            if (newPrice.VendorID != model.VendorIDs[i]) continue;
            newPrice.VendorID = model.VendorIDs[i];
            newPrice.StoreID = storeID;
            tasks.Add(_unitOfWork.StockPrices.InsertStockPrice(newPrice));
          }
        }
        tasks.Add(_unitOfWork.StockPrices.UpdateStockPriceBatch(toBeUpdatedPrices));
      }

      int[] results = await Task.WhenAll(tasks);
      numOfRows = results.Sum();
    }

    if (!_config.IsBackOffice)
    {
      numOfRows += await _unitOfWork.StockPrices.UpdateStockMasterTransferFlag(model.StockCode);
    }

    if (numOfRows == 0)
    {
      throw new Exception("Failed to insert future price.");
    }

    IEnumerable<StockPrice> stockPrices = await _unitOfWork.StockPrices.GetPrices(model, model.PricesInUse, $"'{model.StoreID}'");
    _unitOfWork.Commit();
    return stockPrices;
  }

  public async Task<IEnumerable<StockRebates>?> AddRebate(StockRebates model)
  {
    var storeIDs = string.Empty;
    var rebateByVendorID = new List<StockRebates>();
    var storeToInsertRebate = new List<string>();

    if (!_config.IsBackOffice)
    {
      var ids = new List<string>();
      if (model.StoreIDs != null && model.StoreIDs.Count > 0)
      {
        foreach (var id in model.StoreIDs)
        {
          ids.Add($@"'{id}'");
        }
      }

      storeIDs = ids.Count > 0 ? String.Join(", ", ids) : "";
    }

    var rebates = await _unitOfWork.StockPrices.GetRebate(model.StockCode, model.OrderNo, storeIDs);
    int rebateNo = 1;
    if (rebates.Any())
    {
      rebateByVendorID = rebates.Where(r => r.VendorID == model.VendorID).ToList();
      if (rebateByVendorID.Any())
      {
        rebateNo = rebateByVendorID.Max(x => x.RebateNo) + 1;
      }
    }

    if (!_config.IsBackOffice)
    {
      storeToInsertRebate = model.StoreIDs?.Where(x =>
          !rebateByVendorID.Where(y =>
            y.RebateNo == rebateNo).Select(y => y.StoreID).Contains(x)).ToList();

    }

    model.RebateNo = rebateNo;

    int numOfRows = await _unitOfWork.StockPrices.AddRebate(model, storeToInsertRebate);

    if (!_config.IsBackOffice)
    {
      numOfRows += await _unitOfWork.StockPrices.UpdateStockMasterTransferFlag(model.StockCode);
    }

    if (numOfRows == 0)
    {
      throw new Exception("Failed to insert stock rebate.");
    }

    IEnumerable<StockRebates> stockRebates = await _unitOfWork.StockPrices.GetRebate(model.StockCode, model.OrderNo, @$"'{model.StoreID}'");
    _unitOfWork.Commit();
    return stockRebates;
  }

  public async Task<IEnumerable<StockRebates>?> DeleteRebate(StockRebates model)
  {
    var storeIDs = string.Empty;
    bool useStoreIDs = true;
    if (!_config.IsBackOffice)
    {
      if (model.StoreIDs?.Count == 1 && model.StoreIDs[0] == "ALL")
      {
        useStoreIDs = false;
      }
      else
      {
        var ids = new List<string>();
        if (model.StoreIDs != null && model.StoreIDs.Count > 0)
        {
          foreach (var id in model.StoreIDs)
          {
            ids.Add($@"'{id}'");
          }
        }

        storeIDs = ids.Count > 0 ? String.Join(", ", ids) : "";
      }
    }

    int numOfRows = await _unitOfWork.StockPrices.DeleteRebate(model, storeIDs, useStoreIDs);

    if (!_config.IsBackOffice)
    {
      numOfRows += await _unitOfWork.StockPrices.UpdateStockMasterTransferFlag(model.StockCode);
    }

    if (numOfRows == 0)
    {
      throw new Exception("Failed to delete stock rebate.");
    }

    IEnumerable<StockRebates> stockRebates = await _unitOfWork.StockPrices.GetRebate(model.StockCode, model.OrderNo, $"'{model.StoreID}'");
    _unitOfWork.Commit();
    return stockRebates;
  }

  public async Task<IEnumerable<StockSpecialPrice>?> AddSpecialPrice(StockSpecialPrice model, bool isUseARInvoice, bool? isSetVendor)
  {
    var storeIDs = string.Empty;
    bool useStoreIDs = true;
    if (!_config.IsBackOffice)
    {
      if (model.StoreIDs?.Count == 1 && model.StoreIDs[0] == "ALL")
      {
        useStoreIDs = false;
      }

      var ids = new List<string>();
      if (model.StoreIDs != null && model.StoreIDs.Count > 0)
      {
        foreach (var id in model.StoreIDs)
        {
          ids.Add($@"'{id}'");
        }
      }

      storeIDs = ids.Count > 0 ? String.Join(", ", ids) : "";
    }

    IEnumerable<StockSpecialPrice> stockSpecialPrices;
    if (!isUseARInvoice)
    {
      int numOfRows = await _unitOfWork.StockPrices.RemoveSpecialPrice(model, storeIDs, useStoreIDs);
      var existingSP = await _unitOfWork.StockPrices.GetSpecialPrice(model, storeIDs);

      StockMaster stockMaster = await _unitOfWork.StockItems.GetStockItemByCode(model.StockCode);
      string activeVendor = stockMaster.Vendor;

      if (model.OrderNo == 0)
      {
        if (isSetVendor == true && _config.IsBackOffice && activeVendor == model.VendorID)
        {
          await _unitOfWork.StockItems.UpdatePriceTypeInStockKMasterTableAsync(model);
        }
        else if (isSetVendor == false && _config.IsBackOffice && (activeVendor == model.VendorID || model.VendorID == "ALL"))
        {
          await _unitOfWork.StockItems.UpdatePriceTypeInStockKMasterTableAsync(model);
        }
        else if (isSetVendor == true && !_config.IsBackOffice && activeVendor == model.VendorID && model.StoreID == "ALL")
        {
          await _unitOfWork.StockItems.UpdatePriceTypeInStockKMasterTableAsync(model);
        }
        else if (isSetVendor == false && !_config.IsBackOffice 
          && ((model.VendorID == activeVendor && model.StoreID == "ALL") || (model.VendorID == "ALL" && model.StoreID == "ALL")))
        {
          await _unitOfWork.StockItems.UpdatePriceTypeInStockKMasterTableAsync(model);
        }
      }

      if (model.PriceType == "S")
      {
        if (existingSP == null || existingSP.Count() == 0)
        {
          numOfRows += await _unitOfWork.StockPrices.InsertSpecialPrice(model);
        }
        else
        {
          bool spExists = existingSP.Any(sp => sp.StockCode == model.StockCode && sp.VendorID == model.VendorID && sp.PriceType == model.PriceType);
          if (!spExists)
          {
            numOfRows += await _unitOfWork.StockPrices.InsertSpecialPrice(model);
          }
          else
          {
            numOfRows += await _unitOfWork.StockPrices.UpdateSpecialPrice(model, storeIDs);
          }
        }
      }
      else
      {
        var spSameQty = existingSP.Where(x => x.PriceFromQuantity == model.PriceFromQuantity);
        if (spSameQty == null || spSameQty.Count() == 0)
        {
          numOfRows += await _unitOfWork.StockPrices.InsertSpecialPrice(model);
        }
        else
        {
          bool spExists = existingSP.Any(sp => sp.StockCode == model.StockCode && sp.VendorID == model.VendorID && sp.PriceType == model.PriceType && sp.PriceFromQuantity == model.PriceFromQuantity);
          if (!spExists)
          {
            numOfRows += await _unitOfWork.StockPrices.InsertSpecialPrice(model);
          }
          else
          {
            numOfRows += await _unitOfWork.StockPrices.UpdateSpecialPrice(model, storeIDs, true);
          }
        }
      }


      if (numOfRows == 0)
      {
        throw new Exception("Failed to add special price.");
      }

      stockSpecialPrices = await _unitOfWork.StockPrices.GetSpecialPrice(model, $"'{model.StoreID}'");
    }
    else
    {
      int result = await _unitOfWork.StockPrices.AddSpecialPriceInDiscKLTableAsync(model);
      if (result == 0)
      {
        throw new Exception("Failed to add special price.");
      }

      stockSpecialPrices = await _unitOfWork.StockPrices.GetSpecialPriceFromDiskKL(model);
    }

    _unitOfWork.Commit();
    return stockSpecialPrices;
  }

  public async Task<IEnumerable<StockSpecialPrice>?> UpdateSpecialPriceForUseARInvoicePolicy(StockSpecialPrice model)
  {
    int result = await _unitOfWork.StockPrices.UpdateSpecialPriceForUseARInvoicePolicyAsync(model);
    if (result == 0)
    {
      throw new Exception("Failed to add special price.");
    }

    IEnumerable<StockSpecialPrice> stockSpecialPrices = await _unitOfWork.StockPrices.GetSpecialPriceFromDiskKL(model);

    _unitOfWork.Commit();
    return stockSpecialPrices;
  }

  public async Task<IEnumerable<StockSpecialPrice>?> DeleteSpecialPrice(StockSpecialPrice model, bool isUseARInvoice)
  {
    var storeIDs = string.Empty;
    if (!_config.IsBackOffice)
    {
      var ids = new List<string>();
      if (model.StoreIDs != null && model.StoreIDs.Count > 0)
      {
        foreach (var id in model.StoreIDs)
        {
          ids.Add($@"'{id}'");
        }
      }

      storeIDs = ids.Count > 0 ? String.Join(", ", ids) : "";
    }

    int numOfRows = 0;

    if (isUseARInvoice == true)
    {
      numOfRows = await _unitOfWork.StockPrices.DeleteSpecialPriceFromDiscKLTable(model);
    }
    else
    {
      numOfRows = await _unitOfWork.StockPrices.DeleteSpecialPrice(model, storeIDs);
    }

    if (!_config.IsBackOffice)
    {
      numOfRows += await _unitOfWork.StockPrices.UpdateStockMasterTransferFlag(model.StockCode);
    }

    if (numOfRows == 0)
    {
      throw new Exception("Failed to delete special price.");
    }

    IEnumerable<StockSpecialPrice> stockSpecialPrices = await _unitOfWork.StockPrices.GetSpecialPrice(model, $"'{model.StoreID}'");

    if (isUseARInvoice)
    {
      stockSpecialPrices = await _unitOfWork.StockPrices.GetSpecialPriceFromDiskKL(model);
    }
    else
    {
      stockSpecialPrices = await _unitOfWork.StockPrices.GetSpecialPrice(model, $"'{model.StoreID}'");
    }

    _unitOfWork.Commit();
    return stockSpecialPrices;
  }

  public async Task<IEnumerable<StockPriceVendors>?> GetVendorsNotInStockCost(StockPriceModel model)
  {
    IEnumerable<StockPriceVendors>? stockPriceVendors = await _unitOfWork.StockPrices.GetVendorsNotInStockCost(model);
    _unitOfWork.Commit();
    return stockPriceVendors;
  }

  public async Task<IEnumerable<PriceGroup>?> GetPriceGroup()
  {
    IEnumerable<PriceGroup>? priceGroups = await _unitOfWork.StockPrices.GetPriceGroup();
    _unitOfWork.Commit();
    return priceGroups;
  }

  public async Task<VendorPart?> GetVendorPart(string? stockCode, string? vendorId)
  {
    VendorPart? vendorPart = await _unitOfWork.StockPrices.GetVendorPart(stockCode, vendorId);
    _unitOfWork.Commit();
    return vendorPart;
  }
}
