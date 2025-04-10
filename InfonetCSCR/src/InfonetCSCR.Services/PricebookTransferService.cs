namespace InfonetCSCR.Services;

public class PricebookTransferService : IPricebookTransferService
{
  #region Private Properties
  private readonly IUnitOfWork _unitOfWork;
  private readonly IApplicationConfig _config;
  private readonly IStockItemService _stockItemService;
  #endregion

  public PricebookTransferService(
    IApplicationConfig config,
    IStockItemService stockItemService)
  {
    this._unitOfWork = new UnitOfWork(config, config.CSCTransferConnectionString);
    _stockItemService = stockItemService;
    _config = config;
  }

  #region Private Methods
  private async Task<Dictionary<string, int>> GetPackCountBatch(List<string> stockCodes)
  {
    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      var packCountDict = (await uow.StockItems.GetPackCountBatchAsync(stockCodes))
                          .ToDictionary(pc => pc.StockCode, pc => pc.Packing);
      uow.Commit();
      return packCountDict;
    }
  }

  private async Task PrepareCostChangeTransferObjects(PendingTransferDto model, PrepareCostChangeTransferDto dto)
  {
    var storeIds = dto.StoreIds;
    var packCountDict = dto.PackCountDict;
    var products = dto.Products;
    var stockCostInfoTasks = new List<Task<IEnumerable<StockCostInfo>>>();

    if (_config.IsBackOffice)
    {
      StockCostInfoBatchParameterBackOfficeDto stockCostInfoParameterDto = new()
      {
        VendorId = model.VendorId ?? default,
        OrderNo = model.OrderNo,
        StockCodes = model.Products.Select(_ => _.StockCode).ToList()
      };

      using var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString);
      List<StockCostInfo> stockCostInfos = (await uow.StockCosts.GetStockCostInfoBatchForBackOfficeAsync(stockCostInfoParameterDto)).ToList();
      uow.Commit();

      int costInfoIndex = 0;
      foreach (var product in products)
      {
        int packCount = model.Policies.PackCost && packCountDict.ContainsKey(product.StockCode) ? packCountDict[product.StockCode] : 1;
        bool activeVendor = false;
        string activeVendorId = costInfoIndex < stockCostInfos.Count ? stockCostInfos[costInfoIndex].VendorId ?? "ALL" : "ALL";
        decimal oldCost = costInfoIndex < stockCostInfos.Count ? stockCostInfos[costInfoIndex]?.Cost ?? 0 : 0;
        decimal currentPackCost = (oldCost * packCount);
        decimal inputValue = model.PriceDetails[0].Price;
        bool increase = model.IncreaseOrDecrease == "increase";
        decimal? newCost;

        if (model.DollarOrPercentage == "currency-dollar")
        {
          newCost = (currentPackCost / packCount) + (increase ? (inputValue / packCount) : -(inputValue / packCount));
        }
        else if (model.DollarOrPercentage == "currency-percentage")
        {
          newCost = (currentPackCost / packCount) + (increase ? ((currentPackCost * inputValue / 100) / packCount) : -((oldCost * inputValue / 100) / packCount));
        }
        else newCost = inputValue / packCount;

        if (activeVendorId != "ALL")
        {
          activeVendor = costInfoIndex < stockCostInfos.Count && (stockCostInfos[costInfoIndex]?.ActiveVendor ?? false);
          if (model.ShowByOption == "by-vendor" && costInfoIndex < stockCostInfos.Count)
          {
            activeVendor = (activeVendorId == stockCostInfos[costInfoIndex]?.VendorId);
          }
        }

        //For vendor options, active vendor will be first selected vendor for CostChangeTransfer Table
        if (model.ShowByOption == "by-vendor")
        {
          if (model.SelectedOptionIds.Count > 0)
          {
            activeVendorId = model.SelectedOptionIds[0]; // probably not needed
          }
        }

        string storeId = model?.StoreIds?[0]!;
        dto.CostChangeTransfers.Add(new CostChangeTransfer
        {
          BatchId = dto.BatchId,
          ProductId = product.StockCode,
          Description = product.Description,
          StartDate = model.OrderNo == 0 ? null : model.EffectiveDateFrom,
          EndDate = model.OrderNo == 0 || (model.OrderNo > 0 && model.EffectiveDateDoesNotEnd) ? null : model.EffectiveDateTo,
          OldCost = oldCost,
          NewCost = newCost,
          PriceGroupId = (model.ShowByOption == "by-price-group" && storeId != "ALL") ? dto.SelectedOptionId : null,
          RegionId = (model.ShowByOption == "by-region" && storeId != "ALL") ? dto.SelectedOptionId : null,
          StoreId = storeId,
          VendorId = activeVendorId,
          OrderNo = model.OrderNo,
          TransDate = DateTime.Now,
          TransUser = model.TransUser,
          TransFlag = "O",
          ActiveVendor = activeVendor,
          PriceChangeWithCost = model.ChangePriceBasedOnCost,
        });

        ++costInfoIndex;
      }
    }
    else
    {
      if (storeIds != null)
      {
        storeIds = storeIds.Distinct().ToList();
        foreach (var storeId in storeIds)
        {
          List<GetStockCostInfoDto> stockCostInfoDtos = new List<GetStockCostInfoDto>();
          foreach (var product in products)
          {
            stockCostInfoDtos.Add(new GetStockCostInfoDto
            {
              StoreId = storeId,
              StockCode = product.StockCode,
              OrderNo = model.OrderNo,
            });
          }
          stockCostInfoTasks.Add(_unitOfWork.StockCosts.GetStockCostInfoBatchAsync(stockCostInfoDtos));
        }
      }

      IEnumerable<StockCostInfo>[] stockCostInfoResult = await Task.WhenAll(stockCostInfoTasks);
      List<StockCostInfo> stockCostInfos = stockCostInfoResult.SelectMany(x => x).ToList();

      int costInfoIndex = 0;

      if (storeIds != null)
      {
        foreach (var storeId in storeIds)
        {
          foreach (var product in products)
          {
            int packCount = model.Policies.PackCost && packCountDict.ContainsKey(product.StockCode) ? packCountDict[product.StockCode] : 1;
            bool activeVendor = false;
            string activeVendorId = stockCostInfos[costInfoIndex].VendorId ?? "ALL";
            decimal oldCost = stockCostInfos[costInfoIndex]?.Cost ?? 0;
            decimal currentPackCost = (oldCost * packCount);
            decimal inputValue = model.PriceDetails[0].Price;
            bool increase = model.IncreaseOrDecrease == "increase";
            decimal? newCost;

            if (model.DollarOrPercentage == "currency-dollar")
            {
              newCost = (currentPackCost / packCount) + (increase ? (inputValue / packCount) : -(inputValue / packCount));
            }
            else if (model.DollarOrPercentage == "currency-percentage")
            {
              newCost = (currentPackCost / packCount) + (increase ? ((currentPackCost * inputValue / 100) / packCount) : -((oldCost * inputValue / 100) / packCount));
            }
            else newCost = inputValue / packCount;

            if (activeVendorId != "ALL")
            {
              activeVendor = stockCostInfos[costInfoIndex]?.ActiveVendor ?? false;
              if (model.ShowByOption == "by-vendor")
              {
                activeVendor = (activeVendorId == stockCostInfos[costInfoIndex]?.VendorId);
              }
            }

            //For vendor options, active vendor will be first selected vendor for CostChangeTransfer Table
            if (model.ShowByOption == "by-vendor")
            {
              if (model.SelectedOptionIds.Count > 0)
              {
                activeVendorId = model.SelectedOptionIds[0];
              }
            }

            dto.CostChangeTransfers.Add(new CostChangeTransfer
            {
              BatchId = dto.BatchId,
              ProductId = product.StockCode,
              Description = product.Description,
              StartDate = model.OrderNo == 0 ? null : model.EffectiveDateFrom,
              EndDate = model.OrderNo == 0 || (model.OrderNo > 0 && model.EffectiveDateDoesNotEnd) ? null : model.EffectiveDateTo,
              OldCost = oldCost,
              NewCost = newCost,
              PriceGroupId = (model.ShowByOption == "by-price-group" && storeId != "ALL") ? dto.SelectedOptionId : null,
              RegionId = (model.ShowByOption == "by-region" && storeId != "ALL") ? dto.SelectedOptionId : null,
              StoreId = storeId,
              VendorId = activeVendorId,
              OrderNo = model.OrderNo,
              TransDate = DateTime.Now,
              TransUser = model.TransUser,
              TransFlag = "O",
              ActiveVendor = activeVendor,
              PriceChangeWithCost = model.ChangePriceBasedOnCost,
            });

            ++costInfoIndex;
          }
        }
      }
    }
  }

  private async Task GetCostChangeTransfersByOtherOptionsIds(PendingTransferDto model, List<CostChangeTransfer> costChangeTransfers, Dictionary<string, int> packCountDict, int batchId)
  {
    if (_config.IsBackOffice)
    {
      await PrepareCostChangeTransferObjects(model, new PrepareCostChangeTransferDto
      {
        BatchId = batchId,
        CostChangeTransfers = costChangeTransfers,
        Products = model.Products,
        PackCountDict = packCountDict,
      });
      return;
    }

    var processedStoreIds = new HashSet<string>();
    //For vendor options, we will add a transfer for only the first selected vendor
    if (model.ShowByOption == "by-vendor")
    {
      if (model.SelectedOptionIds.Count > 1)
      {
        model.SelectedOptionIds.RemoveRange(1, model.SelectedOptionIds.Count - 1);
      }
    }

    foreach (var selectedOptionId in model.SelectedOptionIds)
    {
      List<string> tmpOptionIds = new List<string>() { selectedOptionId };
      IEnumerable<Store> stores = null;
      List<string> StoreIds = null;
      List<string> unprocessedStoreIds = null;

      if (model.ShowByOption == "by-region")
      {
        using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
        {
          stores = await uow.StoreLinks.GetStoresByRegionIds(tmpOptionIds);
          uow.Commit();
        }
        StoreIds = stores.Select(store => store.Store_ID).ToList();
      }
      else if (model.ShowByOption == "by-price-group")
      {
        using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
        {
          stores = await uow.StoreLinks.GetStoresByPriceGroupIds(tmpOptionIds);
          uow.Commit();
        }
        StoreIds = stores.Select(store => store.Store_ID).ToList();
      }
      else if (model.ShowByOption == "by-vendor")
      {
        using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
        {
          StoreIds = (List<string>)await uow.StoreLinks.GetStoreIdsByVendorIds(tmpOptionIds);
          uow.Commit();
        }
      }

      if (model.IsSelectAllStoreOrOtherOptionChecked ?? false) StoreIds.Add("ALL");
      if (StoreIds is null || StoreIds.Count == 0) continue;

      unprocessedStoreIds = StoreIds.Where(storeId => !processedStoreIds.Contains(storeId)).ToList();
      unprocessedStoreIds.ForEach(storeId => processedStoreIds.Add(storeId));

      await PrepareCostChangeTransferObjects(model, new PrepareCostChangeTransferDto
      {
        BatchId = batchId,
        CostChangeTransfers = costChangeTransfers,
        StoreIds = unprocessedStoreIds,
        Products = model.Products,
        PackCountDict = packCountDict,
        SelectedOptionId = selectedOptionId,
      });
    }
  }

  private async Task<PriceChangeDefinitionAndTransferDto> GetPriceChangeTransfersByStoreIds(PendingTransferDto model, int batchId)
  {
    List<PriceDefinition> priceDefinitions = new List<PriceDefinition>();
    List<PriceChangeTransfer> priceChangeTransfers = new List<PriceChangeTransfer>();
    List<Task<IEnumerable<StockCostInfo>>> stockCostInfoTasks = new List<Task<IEnumerable<StockCostInfo>>>();
    List<Task<IEnumerable<StockPriceInfo>>> stockPriceInfoTasks = new List<Task<IEnumerable<StockPriceInfo>>>();

    foreach (var storeId in model.StoreIds)
    {
      List<GetStockCostInfoDto> stockCostInfoDtos = new List<GetStockCostInfoDto>();
      foreach (var product in model.Products)
      {
        stockCostInfoDtos.Add(new GetStockCostInfoDto
        {
          StoreId = storeId,
          StockCode = product.StockCode,
          OrderNo = model.OrderNo,
        });
      }
      stockCostInfoTasks.Add(_unitOfWork.StockCosts.GetStockCostInfoBatchAsync(stockCostInfoDtos));
    }

    IEnumerable<StockCostInfo>[] stockCostInfoResult = await Task.WhenAll(stockCostInfoTasks);
    List<StockCostInfo> stockCostInfos = stockCostInfoResult.SelectMany(x => x).ToList();

    for (short priceNumber = 0; priceNumber < model.PriceDetails.Count; priceNumber++)
    {
      int stockCostInfoIndex = 0;
      foreach (var storeId in model.StoreIds)
      {
        List<GetStockPriceInfoDto> stockPriceInfoDtos = new List<GetStockPriceInfoDto>();
        foreach (var product in model.Products)
        {
          string activeVendor = stockCostInfos[stockCostInfoIndex++].VendorId ?? "ALL";
          stockPriceInfoDtos.Add(new GetStockPriceInfoDto
          {
            StoreId = storeId,
            StockCode = product.StockCode,
            PriceNumber = (short)(priceNumber + 1),
            VendorId = activeVendor,
            OrderNo = model.OrderNo,
          });
        }
        stockPriceInfoTasks.Add(_unitOfWork.StockPrices.GetStockPriceInfoBatchAsync(stockPriceInfoDtos));
      }
    }

    IEnumerable<StockPriceInfo>[] stockPriceInfoResult = await Task.WhenAll(stockPriceInfoTasks);
    List<StockPriceInfo> stockPriceInfos = stockPriceInfoResult.SelectMany(x => x).ToList();

    int costInfoIndex = 0;
    foreach (var storeId in model.StoreIds)
    {
      foreach (var product in model.Products)
      {
        string activeVendor = stockCostInfos[costInfoIndex++].VendorId ?? "ALL";
        priceChangeTransfers.Add(new PriceChangeTransfer
        {
          BatchId = batchId,
          ProductId = product.StockCode,
          Description = product.Description?.Replace("'", ""),
          PriceType = model.SelectedPriceType.ToUpper()[0].ToString(),
          StoreId = storeId,
          OrderNo = model.OrderNo,
          VendorId = activeVendor,
          TransUser = model.TransUser,
          TransFlag = "O",
        });
      }
    }

    int stockPriceInfoIndex = 0;
    for (short priceNumber = 0; priceNumber < model.PriceDetails.Count; priceNumber++)
    {
      int stockCostInfoIndex = 0;
      foreach (var storeId in model.StoreIds)
      {
        foreach (var product in model.Products)
        {
          string activeVendor = stockCostInfos[stockCostInfoIndex].VendorId ?? "ALL";
          var priceDefinition = PreparePriceDefinitions(model, batchId, storeId, product, (short)(priceNumber + 1),
            model.PriceDetails[priceNumber], activeVendor, stockCostInfos[stockCostInfoIndex++], stockPriceInfos[stockPriceInfoIndex++]);
          priceDefinitions.Add(priceDefinition);
        }
      }
    }

    PriceChangeDefinitionAndTransferDto priceChangeDefinitionAndTransferDto = new()
    {
      PriceDefinitions = priceDefinitions,
      PriceChangeTransfers = priceChangeTransfers
    };

    return priceChangeDefinitionAndTransferDto;
  }

  private async Task<PriceChangeDefinitionAndTransferDto> GetPriceChangeTransfersByOtherOptionsIds(PendingTransferDto model, int batchId)
  {
    List<PriceDefinition> priceDefinitions = new List<PriceDefinition>();
    List<PriceChangeTransfer> priceChangeTransfers = new List<PriceChangeTransfer>();
    HashSet<string> processedStoreIds = new HashSet<string>();

    //For vendor options, we will add a transfer for only the first selected vendor
    if (model.ShowByOption == "by-vendor" && model.SelectedOptionIds.Count > 1)
    {
      model.SelectedOptionIds.RemoveRange(1, model.SelectedOptionIds.Count - 1);
    }

    if (_config.IsBackOffice)
    {
      List<Task<IEnumerable<StockPriceInfo>>> stockPriceInfoTasks = new();

      StockCostInfoBatchParameterBackOfficeDto stockCostInfoParameterDto = new()
      {
        VendorId = model.VendorId ?? default,
        OrderNo = model.OrderNo,
        StockCodes = model.Products.Select(_ => _.StockCode).ToList()
      };

      using var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString);
      List<StockCostInfo> stockCostInfos = (await uow.StockCosts.GetStockCostInfoBatchForBackOfficeAsync(stockCostInfoParameterDto)).ToList();
      uow.Commit();

      for (short priceNumber = 0; priceNumber < model.PriceDetails.Count; priceNumber++)
      {
        int stockCostInfoIndex = 0;

        List<GetStockPriceInfoDto> stockPriceInfoDtos = new List<GetStockPriceInfoDto>();

        foreach (var product in model.Products)
        {
          string activeVendor = stockCostInfos[stockCostInfoIndex++].VendorId ?? "ALL";

          if (model?.ShowByOption == "by-vendor")
          {
            activeVendor = model.VendorId ?? activeVendor;
          }

          stockPriceInfoDtos.Add(new GetStockPriceInfoDto
          {
            StockCode = product.StockCode,
            PriceNumber = (short)(priceNumber + 1),
            VendorId = activeVendor,
            OrderNo = model.OrderNo,
          });
        }
        stockPriceInfoTasks.Add(_unitOfWork.StockPrices.GetStockPriceInfoBatchAsync(stockPriceInfoDtos));
      }

      IEnumerable<StockPriceInfo>[] stockPriceInfoResult = await Task.WhenAll(stockPriceInfoTasks);
      List<StockPriceInfo> stockPriceInfos = stockPriceInfoResult.SelectMany(x => x).ToList();

      int costInfoIndex = 0;
      foreach (var product in model.Products)
      {
        string activeVendor = stockCostInfos[costInfoIndex].VendorId ?? "ALL";
        if (model?.ShowByOption == "by-vendor")
        {
          activeVendor = model.VendorId ?? activeVendor;
        }

        priceChangeTransfers.Add(new PriceChangeTransfer
        {
          BatchId = batchId,
          ProductId = product.StockCode,
          Description = product.Description?.Replace("'", ""),
          PriceType = model.SelectedPriceType.ToUpper()[0].ToString(),
          StoreId = model.StoreIds[0],
          OrderNo = model.OrderNo,
          CurrentPrice = stockPriceInfos[costInfoIndex++].Price,
          VendorId = activeVendor,
          TransUser = model.TransUser,
          TransFlag = "O",
        });
      }

      int stockPriceInfoIndex = 0;
      for (short priceNumber = 0; priceNumber < model.PriceDetails.Count; priceNumber++)
      {
        int stockCostInfoIndex = 0;
        foreach (var product in model.Products)
        {
          string activeVendor = stockCostInfos[stockCostInfoIndex].VendorId ?? "ALL";
          if (model?.ShowByOption == "by-vendor")
          {
            activeVendor = model.VendorId ?? activeVendor;
          }

          var priceDefinition = PreparePriceDefinitions(model, batchId, model.StoreIds[0], product, (short)(priceNumber + 1),
            model.PriceDetails[priceNumber], activeVendor, stockCostInfos[stockCostInfoIndex++], stockPriceInfos[stockPriceInfoIndex++]);
          priceDefinitions.Add(priceDefinition);
        }
      }
    }
    else
    {
      foreach (var selectedOptionId in model.SelectedOptionIds)
      {
        List<string> tmpOptionIds = new List<string>() { selectedOptionId };
        IEnumerable<Store> stores = null;
        List<string> StoreIds = null;

        if (model.ShowByOption == "by-region")
        {
          using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
          {
            stores = await uow.StoreLinks.GetStoresByRegionIds(tmpOptionIds);
            uow.Commit();
          }
          StoreIds = stores.Select(store => store.Store_ID).ToList();
        }
        else if (model.ShowByOption == "by-price-group")
        {
          using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
          {
            stores = await uow.StoreLinks.GetStoresByPriceGroupIds(tmpOptionIds);
            uow.Commit();
          }
          StoreIds = stores.Select(store => store.Store_ID).ToList();
        }
        else if (model.ShowByOption == "by-vendor")
        {
          using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
          {
            StoreIds = (List<string>)await uow.StoreLinks.GetStoreIdsByVendorIds(tmpOptionIds);
            uow.Commit();
          }
        }

        if (model.IsSelectAllStoreOrOtherOptionChecked ?? false) StoreIds?.Add("ALL");

        List<Task<IEnumerable<StockCostInfo>>> stockCostInfoTasks = new List<Task<IEnumerable<StockCostInfo>>>();
        List<Task<IEnumerable<StockPriceInfo>>> stockPriceInfoTasks = new List<Task<IEnumerable<StockPriceInfo>>>();

        if (StoreIds != null)
        {
          StoreIds = StoreIds.Distinct().ToList();
        }

        foreach (var storeId in StoreIds)
        {
          if (processedStoreIds.Contains(storeId))
          {
            continue; // Skip adding duplicate store ID
          }
          List<GetStockCostInfoDto> stockCostInfoDtos = new List<GetStockCostInfoDto>();
          foreach (var product in model.Products)
          {
            stockCostInfoDtos.Add(new GetStockCostInfoDto
            {
              StoreId = storeId,
              StockCode = product.StockCode,
              OrderNo = model.OrderNo,
            });
          }
          stockCostInfoTasks.Add(_unitOfWork.StockCosts.GetStockCostInfoBatchAsync(stockCostInfoDtos));
        }

        IEnumerable<StockCostInfo>[] stockCostInfoResult = await Task.WhenAll(stockCostInfoTasks);
        List<StockCostInfo> stockCostInfos = stockCostInfoResult.SelectMany(x => x).ToList();

        for (short priceNumber = 0; priceNumber < model.PriceDetails.Count; priceNumber++)
        {
          int stockCostInfoIndex = 0;
          foreach (var storeId in StoreIds)
          {
            if (processedStoreIds.Contains(storeId))
            {
              continue; // Skip adding duplicate store ID
            }


            List<GetStockPriceInfoDto> stockPriceInfoDtos = new List<GetStockPriceInfoDto>();

            // gives stockPriceInfoDtos
            foreach (var product in model.Products)
            {
              string activeVendor = stockCostInfos[stockCostInfoIndex++].VendorId ?? "ALL";
              if (model?.ShowByOption == "by-vendor")
              {
                activeVendor = model.VendorId ?? activeVendor;
              }
              stockPriceInfoDtos.Add(new GetStockPriceInfoDto
              {
                StoreId = storeId,
                StockCode = product.StockCode,
                PriceNumber = (short)(priceNumber + 1),
                VendorId = activeVendor,
                OrderNo = model.OrderNo,
              });
            }
            stockPriceInfoTasks.Add(_unitOfWork.StockPrices.GetStockPriceInfoBatchAsync(stockPriceInfoDtos));
          }
        }

        IEnumerable<StockPriceInfo>[] stockPriceInfoResult = await Task.WhenAll(stockPriceInfoTasks);
        List<StockPriceInfo> stockPriceInfos = stockPriceInfoResult.SelectMany(x => x).ToList();

        int costInfoIndex = 0;
        foreach (var storeId in StoreIds)
        {
          if (processedStoreIds.Contains(storeId))
          {
            continue; // Skip adding duplicate store ID
          }
          foreach (var product in model.Products)
          {
            string activeVendor = stockCostInfos[costInfoIndex++].VendorId ?? "ALL";
            if (model?.ShowByOption == "by-vendor")
            {
              activeVendor = model.VendorId ?? activeVendor;
            }

            priceChangeTransfers.Add(new PriceChangeTransfer
            {
              BatchId = batchId,
              ProductId = product.StockCode,
              Description = product.Description?.Replace("'", ""),
              PriceType = model.SelectedPriceType.ToUpper()[0].ToString(),
              PriceGroupId = (model.ShowByOption == "by-price-group" && storeId != "ALL") ? selectedOptionId : null,
              RegionId = (model.ShowByOption == "by-region" && storeId != "ALL") ? selectedOptionId : null,
              StoreId = storeId,
              OrderNo = model.OrderNo,
              VendorId = activeVendor,
              TransUser = model.TransUser,
              TransFlag = "O",
            });
          }
        }

        int stockPriceInfoIndex = 0;
        for (short priceNumber = 0; priceNumber < model.PriceDetails.Count; priceNumber++)
        {
          int stockCostInfoIndex = 0;
          foreach (var storeId in StoreIds)
          {
            if (processedStoreIds.Contains(storeId))
            {
              continue; // Skip adding duplicate store ID
            }
            foreach (var product in model.Products)
            {
              string activeVendor = stockCostInfos[stockCostInfoIndex].VendorId ?? "ALL";
              if (model?.ShowByOption == "by-vendor")
              {
                activeVendor = model.VendorId ?? activeVendor;
              }

              var priceDefinition = PreparePriceDefinitions(model, batchId, storeId, product, (short)(priceNumber + 1),
                model.PriceDetails[priceNumber], activeVendor, stockCostInfos[stockCostInfoIndex++], stockPriceInfos[stockPriceInfoIndex++]);
              priceDefinitions.Add(priceDefinition);
            }
          }
        }

        foreach (var storeId in StoreIds)
        {
          if (processedStoreIds.Contains(storeId))
          {
            continue; // Skip adding duplicate store ID
          }
          processedStoreIds.Add(storeId);
        }
      }
    }
    PriceChangeDefinitionAndTransferDto priceChangeDefinitionAndTransferDto = new()
    {
      PriceDefinitions = priceDefinitions,
      PriceChangeTransfers = priceChangeTransfers
    };

    return priceChangeDefinitionAndTransferDto;
  }
  #endregion

  public async Task<int> InsertPriceDefinition(PriceDefinition model)
  {
    int affectedRows = await _unitOfWork.PricebookTransfer.InsertPriceDefinition(model);
    _unitOfWork.Commit();
    return affectedRows;
  }

  public async Task<int> InsertPriceDefinitionBatch(List<PriceDefinition> models)
  {
    int affectedRows = await _unitOfWork.PricebookTransfer.InsertPriceDefinitionBatch(models);
    _unitOfWork.Commit();
    return affectedRows;
  }

  public async Task<int> InsertPriceChangeTransfer(PriceChangeTransfer model)
  {
    int affectedRows = await _unitOfWork.PricebookTransfer.InsertPriceChangeTransfer(model);
    _unitOfWork.Commit();
    return affectedRows;
  }

  public async Task<int> InsertPriceChangeTransferBatch(List<PriceChangeTransfer> models)
  {
    int affectedRows = await _unitOfWork.PricebookTransfer.InsertPriceChangeTransferBatch(models);
    _unitOfWork.Commit();
    return affectedRows;
  }

  public async Task<int> PendingTransfers(PendingTransferDto model)
  {
    if (_config.IsBackOffice)
    {
      using var uow = new UnitOfWork(_config, _config.CSCAdminConnectionString);
      Company? company = await _unitOfWork.CompanySetups.GetCompanyAsync();
      uow.Commit();

      model.StoreIds = new() { company?.StoreCode };
    }

    if (model.SelectedDataTransferType == DataTransferType.PriceChange)
    {
      int batchId = await GetNewBatchId(DataTransferType.PriceChange);

      PriceChangeDefinitionAndTransferDto? priceChangeDefinitionAndTransferDto;

      model.Products = await GetStockItems(model);

      if (model.IsSelectAllStoreOrOtherOptionChecked ?? false) model.StoreIds.Add("ALL");

      if (model.ShowByOption == "by-store")
      {
        priceChangeDefinitionAndTransferDto = await GetPriceChangeTransfersByStoreIds(model, batchId);
      }
      else
      {
        priceChangeDefinitionAndTransferDto = await GetPriceChangeTransfersByOtherOptionsIds(model, batchId);
      }

      List<PriceDefinition>? priceDefinitions = priceChangeDefinitionAndTransferDto.PriceDefinitions;
      List<PriceChangeTransfer>? priceChangeTransfers = priceChangeDefinitionAndTransferDto.PriceChangeTransfers;

      var tempPriceDefinitions = new List<PriceDefinition>();
      var tempPriceChangeTransfers = new List<PriceChangeTransfer>();
      int SLICE_SIZE = 1000; // We have choosen 1000 as max insertions per call
      int sliceStart = 0;

      List<Task<int>> tasks = new List<Task<int>>();

      foreach (var priceDefinition in priceDefinitions)
      {
        tempPriceDefinitions.Add(priceDefinition);
        if (tempPriceDefinitions.Count % SLICE_SIZE == 0)
        {
          var priceDefinitionSlice = tempPriceDefinitions.GetRange(sliceStart, SLICE_SIZE);
          tasks.Add(_unitOfWork.PricebookTransfer.InsertPriceDefinitionBatch(priceDefinitionSlice));
          sliceStart += SLICE_SIZE;
        }
      }
      var lastPriceDefinitionSlice = tempPriceDefinitions.GetRange(sliceStart, tempPriceDefinitions.Count - sliceStart);
      tasks.Add(_unitOfWork.PricebookTransfer.InsertPriceDefinitionBatch(lastPriceDefinitionSlice));

      sliceStart = 0;
      foreach (var priceChangeTransfer in priceChangeTransfers)
      {
        tempPriceChangeTransfers.Add(priceChangeTransfer);
        if (tempPriceChangeTransfers.Count % SLICE_SIZE == 0)
        {
          var priceChangeSlice = tempPriceChangeTransfers.GetRange(sliceStart, SLICE_SIZE);
          tasks.Add(_unitOfWork.PricebookTransfer.InsertPriceChangeTransferBatch(priceChangeSlice));
          sliceStart += SLICE_SIZE;
        }
      }
      var lastPriceChangeSlice = tempPriceChangeTransfers.GetRange(sliceStart, tempPriceChangeTransfers.Count - sliceStart);
      tasks.Add(_unitOfWork.PricebookTransfer.InsertPriceChangeTransferBatch(lastPriceChangeSlice));
      await Task.WhenAll(tasks);
      _unitOfWork.Commit();
    }
    else if (model.SelectedDataTransferType == DataTransferType.CostChange)
    {
      return await PendingCostChangeTransfers(model);
    }
    else
    {
      return await PendingLabelPrintingTransfers(model);
    }

    _unitOfWork.Commit();
    return 1;
  }

  public async Task<int> PendingCostChangeTransfers(PendingTransferDto model)
  {
    int batchId = await GetNewBatchId(DataTransferType.CostChange);
    var costChangeTransfers = new List<CostChangeTransfer>();
    var packCountDict = new Dictionary<string, int>();

    if (model.SelectedPriceOption == "future-price" && model.EffectiveDateDoesNotEnd)
      model.EffectiveDateTo = null;

    model.Products = await GetStockItems(model);

    if (model.IsSelectAllStoreOrOtherOptionChecked ?? false) model.StoreIds.Add("ALL");

    if (model.Policies.PackCost)
    {
      packCountDict = await GetPackCountBatch(model.Products.Select(product => product.StockCode).ToList());
    }

    if (model.ShowByOption == "by-store")
    {
      await PrepareCostChangeTransferObjects(model, new PrepareCostChangeTransferDto
      {
        BatchId = batchId,
        CostChangeTransfers = costChangeTransfers,
        StoreIds = model.StoreIds,
        Products = model.Products,
        PackCountDict = packCountDict
      });
    }
    else
    {
      await GetCostChangeTransfersByOtherOptionsIds(model, costChangeTransfers, packCountDict, batchId);
    }

    var tempCostChangeTransfers = new List<CostChangeTransfer>();
    int SLICE_SIZE = 1000; // We have choosen 1000 as max insertions per call

    List<Task<int>> costChangeTransferTasks = new List<Task<int>>();

    foreach (var costChangeTransfer in costChangeTransfers)
    {
      tempCostChangeTransfers.Add(costChangeTransfer);
      if (tempCostChangeTransfers.Count == SLICE_SIZE)
      {
        costChangeTransferTasks.Add(_unitOfWork.PricebookTransfer.InsertCostChangeTransferBatch(tempCostChangeTransfers));
        tempCostChangeTransfers = new List<CostChangeTransfer>();
      }
    }
    costChangeTransferTasks.Add(_unitOfWork.PricebookTransfer.InsertCostChangeTransferBatch(tempCostChangeTransfers));

    await Task.WhenAll(costChangeTransferTasks);
    _unitOfWork.Commit();
    return 1;
  }

  private async Task<int> PendingLabelPrintingTransfers(PendingTransferDto model)
  {
    model.Products = await GetStockItems(model);

    List<StockItemMinimal> tempLabelPrintingDelete = new();
    List<StockItemMinimal> tempLabelPrintingInsert = new();
    int SLICE_SIZE = 1000; // We have choosen 1000 as max insertions per call

    List<Task<int>> tempLabelPrintingDeleteTasks = new();
    List<Task<int>> tempLabelPrintingInsertTasks = new();

    foreach (var costChangeTransfer in model.Products)
    {
      tempLabelPrintingDelete.Add(costChangeTransfer);
      tempLabelPrintingInsert.Add(costChangeTransfer);
      if (tempLabelPrintingDelete.Count == SLICE_SIZE)
      {
        tempLabelPrintingDeleteTasks.Add(_unitOfWork.PricebookTransfer.DeleteLabelPrintingTransferBatch(tempLabelPrintingDelete));
        tempLabelPrintingInsertTasks.Add(_unitOfWork.PricebookTransfer.InsertLabelPrintingTransferBatch(tempLabelPrintingInsert));
        tempLabelPrintingDelete = new();
        tempLabelPrintingInsert = new();
      }
    }
    tempLabelPrintingDeleteTasks.Add(_unitOfWork.PricebookTransfer.DeleteLabelPrintingTransferBatch(tempLabelPrintingDelete));
    tempLabelPrintingInsertTasks.Add(_unitOfWork.PricebookTransfer.InsertLabelPrintingTransferBatch(tempLabelPrintingInsert));

    await Task.WhenAll(tempLabelPrintingDeleteTasks);
    await Task.WhenAll(tempLabelPrintingInsertTasks);
    _unitOfWork.Commit();
    return 1;
  }
  public async Task<int> StockPendingTransfers(PendingTransferDto model)
  {
    int batchId = await GetNewBatchIdForStockChange();

    List<StockDefinition> stockDefinitions = new List<StockDefinition>();
    List<StockItemMinimal> tmpProductList = model.Products;
    HashSet<string> processedStoreIds = new HashSet<string>();

    model.Products = await GetStockItems(model);
    if (model.ShowByOption == "by-store" || model.ShowByOption == "by-vendor")
    {
      foreach (var storeId in model.StoreIds)
      {
        foreach (var product in model.Products)
        {
          stockDefinitions.Add(new StockDefinition
          {
            BatchId = batchId,
            ProductId = product.StockCode,
            Description = product.Description?.Replace("'", ""),
            StoreId = storeId,
            PriceGroupId = null,
            RegionId = null,
            VendorId = model.VendorId,
            TransUser = model.UserCode,
            TransType = (model.IsSelectAllProductsModeActivated ?? false) ? model.TransType : product.TransferType,
            TransDate = DateTime.Now
          });
        }
      }
    }
    else
    {
      IEnumerable<Store>? stores = null;
      List<string> StoreIds = new List<string>();

      if (model.ShowByOption == "by-region")
      {
        using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
        {
          stores = await uow.StoreLinks.GetAllStoresByRegionIds(model.SelectedOptionIds);
          uow.Commit();
        }
        StoreIds = stores.Select(store => store.Store_ID).ToList();
      }
      else if (model.ShowByOption == "by-price-group")
      {
        using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
        {
          stores = await uow.StoreLinks.GetStoresByPriceGroupIds(model.SelectedOptionIds);
          uow.Commit();
        }
        StoreIds = stores.Select(store => store.Store_ID).ToList();
      }

      string? selectedOptionId = stores?.First().GroupId ?? null;

      foreach (var storeId in StoreIds)
      {
        if (processedStoreIds.Contains(storeId))
        {
          continue; // Skip adding duplicate store ID
        }

        processedStoreIds.Add(storeId);

        foreach (var product in model.Products)
        {
          stockDefinitions.Add(new StockDefinition
          {
            BatchId = batchId,
            ProductId = product.StockCode,
            Description = product.Description?.Replace("'", ""),
            StoreId = storeId,
            PriceGroupId = (model.ShowByOption == "by-price-group" && storeId != "ALL") ? selectedOptionId : null,
            RegionId = (model.ShowByOption == "by-region" && storeId != "ALL") ? selectedOptionId : null,
            VendorId = model.VendorId,
            TransUser = model.UserCode,
            TransType = (model.IsSelectAllProductsModeActivated ?? false) ? model.TransType : product.TransferType,
            TransDate = DateTime.Now
          });
        }
      }
    }

    if (model.IsSelectAllProductsModeActivated ?? false)
    {
      foreach (var product in tmpProductList)
      {
        var searchedProduct = stockDefinitions.Where(item => item.ProductId == product.StockCode).ToList();
        if (searchedProduct != null && searchedProduct.Count > 0)
        {
          searchedProduct[0].TransType = product.TransferType;
        }
      }
    }

    var tempStockDefinitions = new List<StockDefinition>();
    int SLICE_SIZE = 1000; // We have choosen 1000 as max insertions per call

    List<Task<int>> stockDefinitionTasks = new List<Task<int>>();

    foreach (var stockDefinition in stockDefinitions)
    {
      tempStockDefinitions.Add(stockDefinition);
      if (tempStockDefinitions.Count == SLICE_SIZE)
      {
        stockDefinitionTasks.Add(_unitOfWork.PricebookTransfer.InsertStockDefinitionBatch(tempStockDefinitions));
        tempStockDefinitions = new List<StockDefinition>();
      }
    }
    stockDefinitionTasks.Add(_unitOfWork.PricebookTransfer.InsertStockDefinitionBatch(tempStockDefinitions));


    await Task.WhenAll(stockDefinitionTasks);
    _unitOfWork.Commit();
    return 1;
  }

  public async Task<int> PendingCommodityTransfers(PendingCommodityTransferDto model, string? userCode)
  {
    int batchId = await GetNewBatchId(DataTransferType.Commodity);
    var commodityTransfers = new List<CommodityTransferDto>();
    List<Task<int>> tasks = new List<Task<int>>();
    int SLICE_SIZE = 1000;

    foreach (var store in model.StoreIds)
    {
      foreach (var dept in model.Departments)
      {
        var commodityTransfer = new CommodityTransferDto
        {
          BatchId = batchId,
          Dept = dept.DeptId,
          SubDept = String.Empty,
          SubDetail = String.Empty,
          Description = dept.DeptName,
          StoreID = store,
          TransType = model.TransTypes[GetTransTypeKey(dept.DeptId)]
        };
        commodityTransfers.Add(commodityTransfer);

        if (commodityTransfers.Count >= SLICE_SIZE)
        {
          tasks.Add(_unitOfWork.PricebookTransfer.InsertCommodityTransferBatch(commodityTransfers, userCode));
          commodityTransfers.Clear();
        }
      }

      foreach (var subDept in model.SubDepartments)
      {
        var commodityTransfer = new CommodityTransferDto
        {
          BatchId = batchId,
          Dept = subDept.DeptId,
          SubDept = subDept.SubDeptId,
          SubDetail = String.Empty,
          Description = subDept.SubDeptName,
          StoreID = store,
          TransType = model.TransTypes[GetTransTypeKey(subDept.DeptId, subDept.SubDeptId)]
        };
        commodityTransfers.Add(commodityTransfer);

        if (commodityTransfers.Count >= SLICE_SIZE)
        {
          tasks.Add(_unitOfWork.PricebookTransfer.InsertCommodityTransferBatch(commodityTransfers, userCode));
          commodityTransfers.Clear();
        }
      }

      foreach (var subDetail in model.SubDetails)
      {
        var commodityTransfer = new CommodityTransferDto
        {
          BatchId = batchId,
          Dept = subDetail.DeptId,
          SubDept = subDetail.SubDeptId,
          SubDetail = subDetail.SubDetailId,
          Description = subDetail.SubDetailName,
          StoreID = store,
          TransType = model.TransTypes[GetTransTypeKey(subDetail.DeptId, subDetail.SubDeptId, subDetail.SubDetailId)]
        };
        commodityTransfers.Add(commodityTransfer);

        if (commodityTransfers.Count >= SLICE_SIZE)
        {
          tasks.Add(_unitOfWork.PricebookTransfer.InsertCommodityTransferBatch(commodityTransfers, userCode));
          commodityTransfers.Clear();
        }
      }
    }

    if (commodityTransfers.Count > 0)
    {
      tasks.Add(_unitOfWork.PricebookTransfer.InsertCommodityTransferBatch(commodityTransfers, userCode));
    }
    int[] results = await Task.WhenAll(tasks);
    int numOfRows = results.Sum();
    _unitOfWork.Commit();
    return numOfRows;
  }

  public async Task<int> PendingHotButtonsTransfers(PendingHotButtonsTransferDto model, string? userCode)
  {
    int batchId = await GetNewBatchId(DataTransferType.HotButtons);
    var hotButtonsTranfers = new List<HotButtonsTransferDto>();
    List<Task<int>> tasks = new List<Task<int>>();
    int SLICE_SIZE = 1000;

    foreach (var store in model.StoreIds)
    {
      foreach (var hotButton in model.HotButtons)
      {
        var hotButtonsTransfer = new HotButtonsTransferDto
        {
          BatchId = batchId,
          Button = hotButton.ButtonId,
          ProductId = hotButton.Product,
          Description = hotButton.ProductName,
          Caption1 = hotButton.Caption1,
          Caption2 = hotButton.Caption2,
          StoreID = store,
          TransType = model.TransTypes[hotButton.ButtonId]
        };
        hotButtonsTranfers.Add(hotButtonsTransfer);

        if (hotButtonsTranfers.Count >= SLICE_SIZE)
        {
          tasks.Add(_unitOfWork.PricebookTransfer.InsertHotButtonsTransferBatch(hotButtonsTranfers, userCode));
          hotButtonsTranfers.Clear();
        }
      }
    }

    if (hotButtonsTranfers.Count > 0)
    {
      tasks.Add(_unitOfWork.PricebookTransfer.InsertHotButtonsTransferBatch(hotButtonsTranfers, userCode));
    }
    int[] results = await Task.WhenAll(tasks);
    int numOfRows = results.Sum();
    _unitOfWork.Commit();
    return numOfRows;
  }

  public async Task<int> PendingPromotionTransfers(PendingPromotionTransferDto model, string? userCode)
  {
    var promotions = new List<PromotionTransfer>();
    int batchId = await GetNewBatchId(DataTransferType.Promotions);

    foreach (var storeId in model.StoreIds)
    {
      foreach (var promotion in model.PromotionTransferItems)
      {
        promotions.Add(new PromotionTransfer
        {
          BatchId = batchId,
          PromoId = promotion.PromoId,
          Description = promotion.Description,
          StartDate = promotion.StartDate,
          EndDate = promotion.EndDate,
          StoreId = storeId,
          TransDate = DateTime.Now,
          TransUser = userCode,
          TransFlag = "O",
          TransType = promotion.TransType,
        });
      }
    }

    int affectedRows = await _unitOfWork.PricebookTransfer.InsertPromotionTransferBatch(promotions);
    _unitOfWork.Commit();
    return affectedRows;
  }

  public async Task<int> DeleteFromTransferList(DeleteFromTransferListDto model)
  {
    if (model.DataTransferType == DataTransferType.LabelPrinting)
    {
      await _unitOfWork.PricebookTransfer.DeleteLabelPrintAsync(model.ItemId);
      _unitOfWork.Commit();
      return 1;
    }

    int affectedRows = await _unitOfWork.PricebookTransfer.DeleteFromTransferList(model);
    _unitOfWork.Commit();
    return affectedRows;
  }

  private async Task<int> GetNewBatchId(string dataTransferType)
  {
    int newBatchId = await _unitOfWork.PricebookTransfer.GetNewBatchId(dataTransferType);
    return newBatchId;
  }

  private async Task<int> GetNewBatchIdForStockChange()
  {
    int newBatchId = await _unitOfWork.StockTransfers.GetBatchId();
    _unitOfWork.Commit();
    return newBatchId;
  }

  private PriceDefinition PreparePriceDefinitions(PendingTransferDto model, int batchId, string storeId,
    StockItemMinimal product, short priceNumber, PriceChangeInfoDto priceDetails,
     string activeVendor, StockCostInfo stockCostInfo, StockPriceInfo stockPriceInfo)
  {
    PriceDefinition priceDefinition = null;
    decimal newPrice = 0;
    decimal newMargin = 0;

    if (model.SelectedPriceType == "r-regular-price")
    {
      newPrice = stockPriceInfo?.Price ?? 0;
      newMargin = stockPriceInfo?.Margin ?? 0;

      if (priceDetails.Price != 0)
      {
        var temp = CalculatePriceAndMargin(stockCostInfo.Cost ?? 0, stockPriceInfo?.Price ?? 0, stockPriceInfo?.Margin ?? 0, priceDetails.Price, model);
        newPrice = temp.Price;
        newMargin = temp.Margin ?? 0;
      }
    }
    else
    {
      newPrice = priceDetails.Price;
    }

    priceDefinition = (ChangeFormFieldsBasedOnPriceType(model, new PriceDefinition
    {
      BatchId = batchId,
      ProductId = product.StockCode,
      PR_Type = model.SelectedPriceType.ToUpper()[0].ToString(),
      StoreId = storeId,
      PR_Units = model.PR_Units,
      PriceNumber = priceNumber,
      Price = newPrice,
      Margin = newMargin,
      PR_From_Qty = priceDetails.From ?? 0,
      PR_To_Qty = priceDetails.To ?? 0,
      PR_FROM = model.EffectiveDateFrom,
      PR_TO = model.EffectiveDateTo,
      OrderNo = model.OrderNo,
      CurrentPrice = _config.IsBackOffice ? stockPriceInfo.Price : null,
      VendorId = activeVendor,
      Trans_User = model.TransUser,
      Trans_Flag = "O",
    }));

    return priceDefinition;
  }

  private decimal CalculatePriceBasedOnPolicy(bool policyMarginCalcCostMkup, decimal cost, decimal margin)
  {
    decimal price = 0;

    if (policyMarginCalcCostMkup)
    {
      price = cost * (1 + margin / 100);
    }
    else
    {
      if (margin >= 0 && margin < 100)
      {
        price = cost / (1 - margin / 100);
      }
    }
    return price;
  }

  private decimal CalculateMarginBasedOnPolicy(bool policyMarginCalcCostMkup, decimal cost, decimal price)
  {
    decimal margin = 0;

    if (policyMarginCalcCostMkup)
    {
      if (cost != 0)
      {
        margin = (price - cost) / cost * 100;
      }
    }
    else
    {
      if (price != 0 && cost != 0)
      {
        margin = (price - cost) / price * 100;
      }
    }
    return margin;
  }

  private StockPriceInfo CalculatePriceAndMargin(decimal cost, decimal oldPrice, decimal oldMargin, decimal inputValue, PendingTransferDto model)
  {
    decimal newPrice;
    decimal newMargin;
    bool increase = model.IncreaseOrDecrease == "increase";

    if (model.MarginOrPrice == "change-by-margin")
    {
      if (model.IncreaseOrDecrease != null)
      {
        newMargin = oldMargin + (increase ? inputValue : -inputValue);
      }
      else newMargin = inputValue;
      newPrice = CalculatePriceBasedOnPolicy(model.Policies.MarginCalcCostMkup, cost, newMargin);
    }
    else
    {
      if (model.DollarOrPercentage == "currency-dollar")
      {
        newPrice = oldPrice + (increase ? inputValue : -inputValue);
      }
      else if (model.DollarOrPercentage == "currency-percentage")
      {
        newPrice = oldPrice + (increase ? (oldPrice * inputValue / 100) : -(oldPrice * inputValue / 100));
      }
      else newPrice = inputValue;

      newMargin = CalculateMarginBasedOnPolicy(model.Policies.MarginCalcCostMkup, cost, newPrice);
    }

    return new StockPriceInfo { Price = newPrice, Margin = newMargin };
  }

  private PriceDefinition ChangeFormFieldsBasedOnPriceType(PendingTransferDto model, PriceDefinition form)
  {
    bool futurePrice = model.SelectedPriceOption == "future-price";

    if (model.SelectedPriceType == "r-regular-price")
    {
      form.PR_Units = null;
      form.PR_From_Qty = 0;
      form.PR_To_Qty = 0;
      form.PR_FROM = futurePrice ? form.PR_FROM : null; ;
      form.PR_TO = futurePrice && !model.EffectiveDateDoesNotEnd ? form.PR_TO : null;
    }
    else
    {
      form.PR_Units = model.DollarOrPercentage == "currency-dollar" ? "$" : "%";
      form.PR_FROM = model.EffectiveDateFrom;
      form.PR_TO = model.EffectiveDateTo;
      form.PriceNumber = 0;
    }
    if (model.SelectedPriceType == "s-sale-pricing")
    {
      form.PR_From_Qty = 0;
      form.PR_To_Qty = 0;
    }
    if (model.SelectedPriceType == "f-first-unit-pricing" || model.SelectedPriceType == "x-x-for-pricing")
    {
      form.PR_To_Qty = 0;
    }
    return form;
  }

  public async Task<IEnumerable<Store>> GetPendingTransferStores(string dataTransferType)
  {
    IEnumerable<Store> stores = await _unitOfWork.PricebookTransfer.GetPendingTransferStoresAsync(dataTransferType);
    _unitOfWork.Commit();
    return stores;
  }

  public async Task<IEnumerable<PriceChangeTransferMinimalDto>> GetPriceChangeTransfers(string? storeId)
  {
    IEnumerable<PriceChangeTransferMinimalDto> priceChangeTransfers = await _unitOfWork.PricebookTransfer.GetPriceChangeTransfersAsync(storeId);
    _unitOfWork.Commit();
    return priceChangeTransfers;
  }

  public async Task<IEnumerable<CostChangeTransferDto>> GetCostChangeTransfers(string? storeId)
  {
    IEnumerable<CostChangeTransferDto> costChangeTransfers = await _unitOfWork.PricebookTransfer.GetCostChangeTransfersAsync(storeId);
    _unitOfWork.Commit();
    return costChangeTransfers;
  }

  public async Task<IEnumerable<CommodityTransferDto>> GetCommodityTransfers(string? storeId)
  {
    IEnumerable<CommodityTransferDto> commodityTransfers = await _unitOfWork.PricebookTransfer.GetCommodityTransfersAsync(storeId);
    _unitOfWork.Commit();
    return commodityTransfers;
  }

  public async Task<IEnumerable<HotButtonsTransferDto>> GetHotButtonsTransfers(string? storeId)
  {
    IEnumerable<HotButtonsTransferDto> hotButtonsTransfers = await _unitOfWork.PricebookTransfer.GetHotButtonsTransfersAsync(storeId);
    _unitOfWork.Commit();
    return hotButtonsTransfers;
  }

  public async Task<IEnumerable<PromotionTransfer>> GetPromotionTransfers(string? storeId)
  {
    IEnumerable<PromotionTransfer> promotionTransfers = await _unitOfWork.PricebookTransfer.GetPromotionTransfersAsync(storeId);
    _unitOfWork.Commit();
    return promotionTransfers;
  }

  public async Task<int> PerformTransfer(string dataTransferType, PricebookTransferPoliciesDto policies, string? userCode)
  {
    if (dataTransferType == DataTransferType.PriceChange)
    {
      await PerformPriceChangeTransfer(policies, userCode);
    }
    else if (dataTransferType == DataTransferType.Commodity)
    {
      var affectedRows = await _unitOfWork.PricebookTransfer.PerformCommodityTransfer();
      _unitOfWork.Commit();
      return affectedRows;
    }
    else if (dataTransferType == DataTransferType.Promotions)
    {
      var affectedRows = await _unitOfWork.PricebookTransfer.PerformPromotionTransfer();
      _unitOfWork.Commit();
      return affectedRows;
    }
    else if (dataTransferType == DataTransferType.CostChange)
    {
      await PerformCostChangeTransfer(policies, userCode);
    }
    else if (dataTransferType == DataTransferType.HotButtons)
    {
      var affectedRows = await _unitOfWork.PricebookTransfer.PerformHotButtonsTransfer();
      _unitOfWork.Commit();
      return affectedRows;
    }

    return 0;
  }

  public async Task<int> StockTransfer()
  {
    IEnumerable<DataDefinition> dataDefinitions = await _unitOfWork.DataDefinitions.GetDataDefinitionsAsync();
    await _unitOfWork.StockItems.UpdateStockMasterFromStockTransferAsync();
    await _unitOfWork.StockTransfers.UpdateStockTransferFlagAsync();
    await _unitOfWork.DataDefinitions.UpdateDataDefinitionTransFlagAsync();

    Dictionary<Tuple<string, string, string>, bool> isAddedTaxes = new Dictionary<Tuple<string, string, string>, bool>();
    Dictionary<Tuple<string, string, string>, bool> isAddedCharges = new Dictionary<Tuple<string, string, string>, bool>();
    Dictionary<Tuple<string, string, string>, bool> isDeleted = new Dictionary<Tuple<string, string, string>, bool>();
    List<Task> tasks = new List<Task>();

    foreach (DataDefinition dataDefinition in dataDefinitions)
    {
      if (dataDefinition.DefCode != "REMOVE")
      {
        if (dataDefinition.DefType == "TAXES")
        {
          if (!isDeleted.ContainsKey(new Tuple<string, string, string>(dataDefinition.ProductID, dataDefinition.StoreId, dataDefinition.DefType)))
          {
            await _unitOfWork.StockTaxes.DeleteStockTaxByStockCodeAndStoreIdAsync(dataDefinition.ProductID, dataDefinition.StoreId);
            isDeleted[new Tuple<string, string, string>(dataDefinition.ProductID, dataDefinition.StoreId, dataDefinition.DefType)] = true;
          }

          string[] parts = dataDefinition.DefCode.Split('-');
          if (!isAddedTaxes.ContainsKey(new Tuple<string, string, string>(dataDefinition.ProductID, parts[0], dataDefinition.StoreId)))
          {
            tasks.Add(_unitOfWork.StockTaxes.AddStockTaxAsync(dataDefinition.ProductID, parts[0], parts[2], dataDefinition.StoreId));
            isAddedTaxes[new Tuple<string, string, string>(dataDefinition.ProductID, parts[0], dataDefinition.StoreId)] = true;
          }
        }
        else if (dataDefinition.DefType == "CHARGES")
        {
          if (!isDeleted.ContainsKey(new Tuple<string, string, string>(dataDefinition.ProductID, dataDefinition.StoreId, dataDefinition.DefType)))
          {
            await _unitOfWork.StockAcs.DeleteStockAcByStockCodeAndStoreIdAsync(dataDefinition.ProductID, dataDefinition.StoreId);
            isDeleted[new Tuple<string, string, string>(dataDefinition.ProductID, dataDefinition.StoreId, dataDefinition.DefType)] = true;
          }

          if (!isAddedCharges.ContainsKey(new Tuple<string, string, string>(dataDefinition.ProductID, dataDefinition.DefType, dataDefinition.StoreId)))
          {
            tasks.Add(_unitOfWork.StockAcs.AddStockAcAsync(dataDefinition.ProductID, dataDefinition.DefCode, dataDefinition.StoreId));
            isAddedCharges[new Tuple<string, string, string>(dataDefinition.ProductID, dataDefinition.DefCode, dataDefinition.StoreId)] = true;
          }
        }
      }
    }

    await Task.WhenAll(tasks);
    _unitOfWork.Commit();

    return 1;
  }

  private async Task<bool> PerformPriceChangeTransfer(PricebookTransferPoliciesDto policies, string? userCode)
  {
    #region OldCode
    //string tmpProductId = "";
    //int tmpOrderNo = 0;
    //string tmpStoreId = "";
    //bool deleteExistingPrices = false;
    //IEnumerable<PriceDefinition> priceDefinitions = await _unitOfWork.PricebookTransfer.GetPriceDefinitions();

    //foreach (var priceDef in priceDefinitions)
    //{
    //  using (var _unitOfWorkMaster = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    //  {
    //    if (priceDef.PR_Type == "R")
    //    {
    //      bool updateForVendorAll = !policies.SetVend && policies.UpdateAllVendor;
    //      await _unitOfWorkMaster.StockPrices.CreateOrUpdateStockPriceAsync(priceDef, updateForVendorAll);
    //      await CreateCostRecords(new CreateCostRecordDto
    //      {
    //        StoreId = priceDef.StoreId,
    //        ProductId = priceDef.ProductId,
    //        OrderNo = priceDef.OrderNo,
    //        VendorId = priceDef.VendorId,
    //        StartDate = priceDef.PR_FROM,
    //        EndDate = priceDef.PR_TO
    //      });
    //    }
    //    else
    //    {
    //      if (tmpProductId == "")
    //      {
    //        tmpProductId = priceDef.ProductId;
    //        tmpOrderNo = priceDef.OrderNo;
    //        tmpStoreId = priceDef.StoreId;
    //        deleteExistingPrices = true;
    //      }
    //      else
    //      {
    //        if (tmpProductId != priceDef.ProductId
    //         || tmpOrderNo != priceDef.OrderNo
    //         || tmpStoreId != priceDef.StoreId)
    //        {
    //          tmpProductId = priceDef.ProductId;
    //          tmpOrderNo = priceDef.OrderNo;
    //          tmpStoreId = priceDef.StoreId;
    //          deleteExistingPrices = true;
    //        }
    //        else deleteExistingPrices = false;
    //      }

    //      if (deleteExistingPrices)
    //      {
    //        await _unitOfWorkMaster.StockPrices.DeleteExistingSpecialPricesAsync(priceDef.ProductId, priceDef.StoreId, priceDef.OrderNo, priceDef.VendorId);

    //        deleteExistingPrices = false;

    //        await CreateCostRecords(new CreateCostRecordDto
    //        {
    //          ProductId = priceDef.ProductId,
    //          StoreId = priceDef.StoreId,
    //          OrderNo = priceDef.OrderNo,
    //          VendorId = priceDef.VendorId,
    //          StartDate = priceDef.PR_FROM,
    //          EndDate = priceDef.PR_TO,
    //        });

    //        int stockPriceCount = await _unitOfWorkMaster.StockPrices.GetStockPriceRecordCountAsync(new GetStockPriceRecordCountDto
    //        {
    //          StockCode = priceDef.ProductId,
    //          StoreId = priceDef.StoreId,
    //          OrderNo = priceDef.OrderNo,
    //          VendorId = priceDef.VendorId,
    //        });

    //        if (stockPriceCount == 0)
    //        {
    //          await CreatePriceRecords(new CreatePriceRecordDto
    //          {
    //            ProductId = priceDef.ProductId,
    //            StoreId = priceDef.StoreId,
    //            OrderNo = priceDef.OrderNo,
    //            VendorId = priceDef.VendorId,
    //            StartDate = priceDef.OrderNo != 0 ? priceDef.PR_FROM : null,
    //            EndDate = priceDef.OrderNo != 0 ? priceDef.PR_TO : null,
    //          }, "price-change", policies);

    //          await CreatePriceChangeRecords(priceDef.StoreId, priceDef.VendorId, priceDef.ProductId, priceDef.OrderNo, userCode);
    //        }
    //      }

    //      await _unitOfWorkMaster.StockPrices.AddSpecialPriceAsync(new StockSpecialPrice
    //      {
    //        StockCode = priceDef?.ProductId,
    //        PriceFromQuantity = priceDef?.PR_From_Qty,
    //        PriceToQuantity = priceDef?.PR_To_Qty,
    //        Price = priceDef?.Price,
    //        PriceType = priceDef?.PR_Type,
    //        PriceUnits = priceDef?.PR_Units,
    //        StoreID = priceDef?.StoreId,
    //        VendorID = priceDef?.VendorId,
    //        StartDate = priceDef?.PR_FROM,
    //        EndDate = priceDef?.PR_TO,
    //        OrderNo = priceDef?.OrderNo ?? 0,
    //      });
    //    }
    //    await _unitOfWorkMaster.StockItems.UpdateStockItemUpdateDateField(priceDef.ProductId);
    //    _unitOfWorkMaster.Commit();
    //  }
    //}

    //await _unitOfWork.PricebookTransfer.PerformPriceChangeTransfer();
    //_unitOfWork.Commit();
    #endregion
    await _unitOfWork.PricebookTransfer.PerformPriceChangeTransfer(policies, userCode);
    _unitOfWork.Commit();
    return true;
  }

  private async Task<int> PerformCostChangeTransfer(PricebookTransferPoliciesDto policies, string? userCode)
  {
    #region oldCode
    //IEnumerable<CostChangeTransferDto> allCostChangeTransfers = await GetAllCostChangeTransfers();

    //int count = 0;
    //int zeroCount = 0;

    //foreach (var costChangeTransfer in allCostChangeTransfers)
    //{
    //  StockPriceModel model = new StockPriceModel
    //  {
    //    StockCode = costChangeTransfer.Product,
    //    StoreID = costChangeTransfer.StoreId,
    //    VendorID = costChangeTransfer.VendorId,
    //    OrderNo = costChangeTransfer.OrderNo,
    //  };
    //  IEnumerable<StockCosts>? stockCosts = null;
    //  using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    //  {
    //    stockCosts = await uow.StockPrices.GetStockCosts(model, $@"'{costChangeTransfer.StoreId}'", "");
    //    List<Task> tasks = new List<Task>();
    //    foreach (var cost in stockCosts)
    //    {
    //      cost.Cost = costChangeTransfer.NewCost;
    //      cost.StartDate = costChangeTransfer.OrderNo != 0 ? costChangeTransfer.StartDate : cost.StartDate;
    //      cost.EndDate = costChangeTransfer.OrderNo != 0 ? costChangeTransfer.EndDate : cost.EndDate;

    //      tasks.Add(uow.StockPrices.UpdateStockCost(cost, $"'{cost.StoreID}'", $"'{cost.VendorID}'"));

    //    }

    //    count += stockCosts.Count();


    //    if (!stockCosts.Any())
    //    {
    //      zeroCount++;
    //      StockCosts stockCost = new StockCosts
    //      {
    //        StockCode = costChangeTransfer.Product,
    //        Cost = costChangeTransfer.NewCost,
    //        StoreID = costChangeTransfer.StoreId ?? "ALL",
    //        VendorID = costChangeTransfer.VendorId ?? "ALL",
    //        StartDate = costChangeTransfer.OrderNo != 0 ? costChangeTransfer.StartDate : null,
    //        EndDate = costChangeTransfer.OrderNo != 0 ? costChangeTransfer.EndDate : null,
    //        OrderNo = costChangeTransfer.OrderNo,
    //        ActiveVendor = costChangeTransfer.ActiveVendor,
    //      };

    //      tasks.Add(uow.StockPrices.InsertStockCosts(stockCost, $"'{stockCost.StoreID}'", stockCost.VendorID));
    //    }
    //    await Task.WhenAll(tasks);
    //    uow.Commit();
    //  }

    //  await UpdatePriceForCostTransfer(costChangeTransfer.StoreId, costChangeTransfer.VendorId, costChangeTransfer.Product, costChangeTransfer.OrderNo, costChangeTransfer.NewCost ?? 0, policies.NumberOfPrice, costChangeTransfer.PriceChangeWithCost ?? false, policies.MarginCalcCostMkup, costChangeTransfer.StartDate, costChangeTransfer.EndDate, userCode);

    //  if (!policies.SetVend && policies.UpdateAllVendor)
    //  {
    //    model = new StockPriceModel
    //    {
    //      StockCode = costChangeTransfer.Product,
    //      StoreID = costChangeTransfer.StoreId,
    //      VendorID = "ALL",
    //      OrderNo = costChangeTransfer.OrderNo,
    //    };

    //    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    //    {
    //      stockCosts = await uow.StockPrices.GetStockCosts(model, $@"'{costChangeTransfer.StoreId}'", "");
    //      List<Task> tasks = new List<Task>();
    //      if (stockCosts.Any())
    //      {
    //        foreach (var cost in stockCosts)
    //        {
    //          cost.Cost = costChangeTransfer.NewCost;
    //          cost.StartDate = costChangeTransfer.OrderNo != 0 ? costChangeTransfer.StartDate : cost.StartDate;
    //          cost.EndDate = costChangeTransfer.OrderNo != 0 ? costChangeTransfer.EndDate : cost.EndDate;
    //          tasks.Add(uow.StockPrices.UpdateStockCost(cost, $"'{cost.StoreID}'", $"'{cost.VendorID}'"));
    //        }
    //      }
    //      else
    //      {
    //        StockCosts stockCost = new StockCosts
    //        {
    //          StockCode = costChangeTransfer.Product,
    //          Cost = costChangeTransfer.NewCost,
    //          StoreID = costChangeTransfer.StoreId ?? "ALL",
    //          VendorID = "ALL",
    //          StartDate = costChangeTransfer.OrderNo != 0 ? costChangeTransfer.StartDate : null,
    //          EndDate = costChangeTransfer.OrderNo != 0 ? costChangeTransfer.EndDate : null,
    //          OrderNo = costChangeTransfer.OrderNo,
    //          ActiveVendor = false,
    //        };
    //        tasks.Add(uow.StockPrices.InsertStockCosts(stockCost, $"'{stockCost.StoreID}'", stockCost.VendorID));
    //      }
    //      await Task.WhenAll(tasks);
    //      uow.Commit();
    //    }
    //    await UpdatePriceForCostTransfer(costChangeTransfer.StoreId, "ALL", costChangeTransfer.Product, costChangeTransfer.OrderNo, costChangeTransfer.NewCost ?? 0, policies.NumberOfPrice, costChangeTransfer.PriceChangeWithCost ?? false, policies.MarginCalcCostMkup, costChangeTransfer.StartDate, costChangeTransfer.EndDate, userCode);
    //  }
    //}
    #endregion

    var affectedRows = await _unitOfWork.PricebookTransfer.PerformCostChangeTransfer(policies, userCode);
    _unitOfWork.Commit();
    return affectedRows;
  }

  private string GetTransTypeKey(string deptId, string? subDeptId = null, string? subDetailId = null)
  {
    string result = deptId.Trim();
    if (subDeptId != null)
    {
      result += $"-{subDeptId.Trim()}";
    }
    if (subDetailId != null)
    {
      result += $"-{subDetailId.Trim()}";
    }
    return result;
  }

  private async Task<List<StockItemMinimal>> GetStockItems(PendingTransferDto model)
  {
    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      if (model.IsSelectAllProductsModeActivated ?? false)
      {
        if (model.VendorId == "ALL" || model.VendorId == "" || model.IsShowVendorProducts == false)
        {
          model.Products = (await _stockItemService.GetStockItemsByDeptSubDeptAndSubDetail(new GetStockItemsByDeptSubDeptAndSubDetailDto
          {
            Depts = model.ShowAllProducts ? new List<Department>() : model.Departments,
            SubDepts = model.ShowAllProducts ? new List<SubDepartment>() : model.SubDepartments,
            SubDetails = model.ShowAllProducts ? new List<SubDetail>() : model.SubDetails,
          })).StockItems.ToList();

        }
        else
        {
          model.Products = (await _stockItemService.GetStockItemsByVendor(new GetStockItemsByVendorDto
          {
            VendorId = model.VendorId,
          })).StockItems.ToList();
        }

        model.Products = model.Products
                        .Where(prod => !model.DeselectedProducts.Any(p => p.StockCode == prod.StockCode))
                        .ToList();
      }
      uow.Commit();
    }

    return model.Products;
  }

  public async Task<IEnumerable<StockItemMinimal>?> GetLabelPrints()
  {
    IEnumerable<StockItemMinimal>? labelPrints = await _unitOfWork.PricebookTransfer.GetLabelPrintsAsync();
    return labelPrints;
  }

  public async Task<bool> DeleteLabelPrint(string? stockCode)
  {
    bool isDeleted = await _unitOfWork.PricebookTransfer.DeleteLabelPrintAsync(stockCode);
    return isDeleted;
  }
}
