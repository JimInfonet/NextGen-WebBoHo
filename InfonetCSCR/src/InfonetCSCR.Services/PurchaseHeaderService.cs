namespace InfonetCSCR.Services;

public class PurchaseHeaderService : IPurchaseHeaderService
{
  private readonly IStockTakesAndReceivingService _stockTakesAndReceivingService;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IPurchaseDetailService _purchaseDetailService;
  private readonly IVendorSortService _vendorSortService;
  private readonly IApplicationConfig _applicationConfig;

  private bool _blSupportKits = false;
  private bool _packCost = false;
  private bool _updateAllVendor = false;
  private bool _setVend = false;
  private bool _blPChange_Cost = false;
  private bool _PChangeInPO = false;
  #region Constants

  const double minSmallMoneyValue = -214748.3648;
  const double maxSmallMoneyValue = 214748.3647;

  #endregion

  #region Private Methods

  private double CalculatePOSurcharge(double subTotal, double surChargeTotalDiscount, string surChargeTotalDiscountType)
  {
    double poSurcharge = 0;
    if (surChargeTotalDiscount != 0)
    {
      if (surChargeTotalDiscountType == "$")
      {
        poSurcharge = surChargeTotalDiscount;
      }
      else
      {
        poSurcharge = subTotal * surChargeTotalDiscount / 100;
      }
    }

    poSurcharge = poSurcharge.Round(2);

    return poSurcharge;
  }

  #endregion

  public PurchaseHeaderService(
    IApplicationConfig applicationConfig, 
    IPurchaseDetailService purchaseDetailService,
    IStockTakesAndReceivingService stockTakesAndReceivingService,
    IVendorSortService vendorSortService)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCTransConnectionString);
    _purchaseDetailService = purchaseDetailService;
    _stockTakesAndReceivingService = stockTakesAndReceivingService;
    _applicationConfig = applicationConfig;
    _vendorSortService = vendorSortService;
  }

  public async Task<IEnumerable<PurchaseHeader>> GetPurchaseHeadersByPOStatus(string poStatus, int columnIndex)
  {
    IEnumerable<PurchaseHeader> purchaseHeaders = await _unitOfWork.PurchaseHeaders.GetPurchaseHeadersByPOStatusAsync(poStatus, columnIndex);
    _unitOfWork.Commit();
    return purchaseHeaders;
  }

  public async Task<bool> AddPurchaseHeader(PurchaseHeaderRecord purchaseHeaderRecord, Vendor vendor)
  {
    PurchaseHeader purchaseHeader = new PurchaseHeader();
    long newPurchaseNo = purchaseHeaderRecord.PurchaseNo;

    purchaseHeader.PurchaseNo = newPurchaseNo;
    purchaseHeader.PurchaseDate = DateTime.Today;
    purchaseHeader.Paid = false;
    purchaseHeader.Emailed = false;
    purchaseHeader.AccountNo = purchaseHeaderRecord.VendorId;
    purchaseHeader.VendorName = purchaseHeaderRecord.VendorName;
    purchaseHeader.CreatedBy = purchaseHeaderRecord.UserCode;
    purchaseHeader.POStatus = "O"; // Hard coded in VB code
    purchaseHeader.StoreID = purchaseHeaderRecord.StoreId;

    purchaseHeader.DeliveryDate = DateTime.Today;
    purchaseHeader.ReceivedDate = DateTime.Today;
    purchaseHeader.Terms = "";
    purchaseHeader.Buyer = "";
    purchaseHeader.ShipVia = "";
    purchaseHeader.FOB = "";
    purchaseHeader.Freight = "";

    purchaseHeader.ShiftID = 0;
    purchaseHeader.ShiftNum = 0;
    purchaseHeader.SurChargeTotalDiscount = 0;
    purchaseHeader.SurChargeTotalDiscountType ??= "$"; // Default data in database
    purchaseHeader.Reposted = false;

    purchaseHeader.CalculateDiscount = await _unitOfWork.PurchaseDetails.CalculatPODiscount(newPurchaseNo.ToString());
    if (!(purchaseHeader.CalculateDiscount >= minSmallMoneyValue && purchaseHeader.CalculateDiscount <= maxSmallMoneyValue))
    {
      purchaseHeader.CalculateDiscount = 0;
    }

    purchaseHeader.SubTotal = await _purchaseDetailService.CalculatePOSubTotal(newPurchaseNo.ToString(), purchaseHeaderRecord.VendorId);
    purchaseHeader.TotalGST = await _unitOfWork.PurchaseDetails.CalculatePOGSTTotal(newPurchaseNo.ToString());
    purchaseHeader.SurChargeTotal = 0;
    purchaseHeader.Lines = await _unitOfWork.PurchaseDetails.GetLinesCountAsync(newPurchaseNo.ToString());
    purchaseHeader.PurchaseTotal = (purchaseHeader.SubTotal + purchaseHeader.TotalGST + purchaseHeader.SurChargeTotal);
    purchaseHeader.Notes = "";
    purchaseHeader.CustAccount = vendor.CustAccount;

    bool isAdded = await _unitOfWork.PurchaseHeaders.AddPurchaseHeaderAsync(purchaseHeader);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> UpdatePurchaseHeaderEmailedStatus(PurchaseHeader purchaseheader)
  {
    bool isUpdated = await _unitOfWork.PurchaseHeaders.UpdatePurchaseHeaderEmailedStatusAsync(purchaseheader);
    _unitOfWork.Commit();
    return isUpdated;
  }

  public async Task<PurchaseHeader?> GetPurchaseHeadersByPurchaseNo(string purchaseNo)
  {
    PurchaseHeader? purchaseHeader = await _unitOfWork.PurchaseHeaders.GetPurchaseHeadersByPurchaseNoAsync(long.Parse(purchaseNo));
    purchaseHeader.SubTotal =
      await _purchaseDetailService.CalculatePOSubTotal(purchaseHeader.PurchaseNo.ToString(), purchaseHeader.AccountNo, true);

    purchaseHeader.TotalGST = await _unitOfWork.PurchaseDetails.CalculatePOGSTTotal(purchaseHeader.PurchaseNo.ToString());
    purchaseHeader.SurChargeTotal = CalculatePOSurcharge((double)purchaseHeader.SubTotal, (double)purchaseHeader.SurChargeTotalDiscount, purchaseHeader.SurChargeTotalDiscountType);
    purchaseHeader.PurchaseTotal = purchaseHeader.SubTotal + purchaseHeader.TotalGST + purchaseHeader.SurChargeTotal;

    _unitOfWork.Commit();
    return purchaseHeader;
  }

  public async Task<bool> UpdatePurchaseHeader(PurchaseHeaderUpdateRecord purchaseHeaderUpdateRecord)
  {
    PurchaseHeader purchaseheader = purchaseHeaderUpdateRecord.PurchaseHeader;
    bool isEditTotals = (bool)purchaseHeaderUpdateRecord.IsEditTotals;

    PurchaseHeader? currentPurchaseHeader =
      await _unitOfWork.PurchaseHeaders.GetPurchaseHeadersByPurchaseNoAsync(purchaseheader.PurchaseNo);

    if (purchaseheader.Buyer != null)
    {
      currentPurchaseHeader.Buyer = purchaseheader.Buyer;
    }

    if (purchaseheader.ShipVia != null)
    {
      currentPurchaseHeader.ShipVia = purchaseheader.ShipVia;
    }

    if (purchaseheader.Freight != null)
    {
      currentPurchaseHeader.Freight = purchaseheader.Freight;
    }

    if (purchaseheader.FOB != null)
    {
      currentPurchaseHeader.FOB = purchaseheader.FOB;
    }

    if (purchaseheader.CustAccount != null)
    {
      currentPurchaseHeader.CustAccount = purchaseheader.CustAccount;
    }

    if (purchaseheader.PurchaseDate != null)
    {
      currentPurchaseHeader.PurchaseDate = purchaseheader.PurchaseDate;
    }

    if (purchaseheader.DeliveryDate != null)
    {
      currentPurchaseHeader.DeliveryDate = purchaseheader.DeliveryDate;
    }

    if (purchaseheader.ReceivedDate != null)
    {
      currentPurchaseHeader.ReceivedDate = purchaseheader.ReceivedDate;
    }

    if (purchaseheader.InvoiceNo != null)
    {
      currentPurchaseHeader.InvoiceNo = purchaseheader.InvoiceNo;
    }

    if (purchaseheader.Terms != null)
    {
      currentPurchaseHeader.Terms = purchaseheader.Terms;
    }

    if (purchaseheader.Notes != null)
    {
      currentPurchaseHeader.Notes = purchaseheader.Notes;
    }

    if (purchaseheader.SubTotal != null)
    {
      currentPurchaseHeader.SubTotal = purchaseheader.SubTotal;
    }

    if (purchaseheader.SurChargeTotal != null)
    {
      currentPurchaseHeader.SurChargeTotal = purchaseheader.SurChargeTotal;
    }

    if (purchaseheader.TotalGST != null)
    {
      currentPurchaseHeader.TotalGST = purchaseheader.TotalGST;
    }

    if (purchaseheader.PurchaseTotal != null)
    {
      currentPurchaseHeader.PurchaseTotal = purchaseheader.PurchaseTotal;
    }

    currentPurchaseHeader.SubTotal =
      await _purchaseDetailService.CalculatePOSubTotal(currentPurchaseHeader.PurchaseNo.ToString(), purchaseHeaderUpdateRecord.VendorId, true);

    currentPurchaseHeader.TotalGST = await _unitOfWork.PurchaseDetails.CalculatePOGSTTotal(currentPurchaseHeader.PurchaseNo.ToString());
    currentPurchaseHeader.SurChargeTotal = CalculatePOSurcharge((double)currentPurchaseHeader.SubTotal, (double)currentPurchaseHeader.SurChargeTotalDiscount, currentPurchaseHeader.SurChargeTotalDiscountType);
    currentPurchaseHeader.PurchaseTotal = currentPurchaseHeader.SubTotal + currentPurchaseHeader.TotalGST + currentPurchaseHeader.SurChargeTotal;

    currentPurchaseHeader.Lines = await _unitOfWork.PurchaseDetails.GetLinesCountAsync(purchaseheader.PurchaseNo.ToString());

    bool isUpdated = await _unitOfWork.PurchaseHeaders.UpdatePurchaseHeaderAsync(currentPurchaseHeader);
    _unitOfWork.Commit();
    return isUpdated;
  }

  public async Task<bool> UpdatePurchaseHeaderSurChargeDiscount(PurchaseHeader purchaseheader)
  {
    purchaseheader.SurChargeTotal = CalculatePOSurcharge((double)purchaseheader.SubTotal, (double)purchaseheader.SurChargeTotalDiscount, purchaseheader.SurChargeTotalDiscountType);
    purchaseheader.PurchaseTotal = purchaseheader.SubTotal + purchaseheader.TotalGST + purchaseheader.SurChargeTotal;

    bool isUpdated = await _unitOfWork.PurchaseHeaders.UpdatePurchaseHeaderSurChargeDiscountAsync(purchaseheader);
    _unitOfWork.Commit();
    return isUpdated;
  }

  public async Task<PurchaseHeader?> GetPurchaseHeadersByPurchaseNo(long? selectedPO)
  {
    PurchaseHeader? purchaseHeaders = await _unitOfWork.PurchaseHeaders.GetPurchaseHeadersByPurchaseNoAsync((long)selectedPO!);
    _unitOfWork.Commit();
    return purchaseHeaders;
  }

  public async Task<string> POPayment(PurchaseAndReceivePostDto purchaseAndReceivePostDto)
  {
    _blPChange_Cost = purchaseAndReceivePostDto.CostChange! == true;
    _blSupportKits = purchaseAndReceivePostDto.PurchasingAndReceivingPolicies?.KitsInPoPolicy! == true;
    _packCost = purchaseAndReceivePostDto.PurchasingAndReceivingPolicies?.PackCostPolicy! == true;
    _updateAllVendor = purchaseAndReceivePostDto.PurchasingAndReceivingPolicies?.UpdateAllVendor! == true;
    _setVend = purchaseAndReceivePostDto.PurchasingAndReceivingPolicies?.SetVend! == true;
    _PChangeInPO = purchaseAndReceivePostDto.PurchasingAndReceivingPolicies?.PChangeInPO! == true;

    PurchaseHeader? purchaseHeader = await _unitOfWork.PurchaseHeaders.GetPurchaseHeadersByPurchaseNoAsync((long)purchaseAndReceivePostDto.PurchaseNo!);
    if (purchaseHeader != null)
    {
      var vendorID = purchaseHeader.AccountNo;
      if (purchaseAndReceivePostDto.IsPaid == true && (purchaseHeader.POStatus == "S" || purchaseHeader.POStatus == "O" || purchaseHeader.POStatus == "C"))
      {
        // if reposting PO mark the same in PO . Also mark in shift reconcilliation ( if changing the reportid need to mark the old and new)
        if (purchaseHeader.Paid == true)
        {
          //reposting of PO
          purchaseHeader.Reposted = true;
          purchaseHeader.RepostedBy = purchaseAndReceivePostDto.UserCode;
          purchaseHeader.RepostedDate = DateTime.Now;
          await UpdateShiftRecRepost(purchaseHeader.ShiftNum, purchaseHeader.ShiftID, purchaseAndReceivePostDto.ShiftNumber, purchaseAndReceivePostDto.ShiftID);
        }
        else
        {
          // if first time coming to post the PO
          purchaseHeader.Paid = true;
          purchaseHeader.PostedBy = purchaseAndReceivePostDto.UserCode;
          purchaseHeader.PostedDate = DateTime.Now;
          purchaseHeader.POStatus = "C"; //Closed
        }
        purchaseHeader.ShiftNum = purchaseAndReceivePostDto.ShiftNumber;
        purchaseHeader.ShiftID = (int)purchaseAndReceivePostDto.ShiftID!;
        purchaseHeader.PayType = purchaseAndReceivePostDto.PaymentType;
        _ = await _unitOfWork.PurchaseHeaders.UpdatePurchaseHeaderAsync(purchaseHeader);

        // when posting the PO update the posted flag for all existing detail lines(this is to handle detail lines in repost )
        await UpdateMasterTables(purchaseAndReceivePostDto.PurchaseNo!, vendorID, purchaseAndReceivePostDto.UserCode);
        _ = await _unitOfWork.PurchaseDetails.UpdatePurchaseDetailAsync((long)purchaseAndReceivePostDto.PurchaseNo!);
        _unitOfWork.Commit();
        return purchaseAndReceivePostDto.PurchaseNo.ToString()!;
      }
    }
    _unitOfWork.Commit();
    return "";
  }

  public async Task<TransferPOFileDto?> GetPOTransferFilesData(long purchaseNo, string headerFile, string detailFile)
  {
    Company? company = await _unitOfWork.CompanySetups.GetCompanyAsync();

    using (var uow = new UnitOfWork(_applicationConfig, _applicationConfig.CSCTransConnectionString))
    {
      await uow.PurchaseHeaders.UpdateHeaderFileNameAsync(headerFile, purchaseNo);
      uow.Commit();
    }

    IEnumerable<PurchaseHeader> purchaseHeaders = await _unitOfWork.PurchaseHeaders.GetPurchaseHeaderByHeaderFileAsync(headerFile);
    if (purchaseHeaders == null)
    {
      throw new Exception("No PurchaseHeader Found");
    }

    List<PurchaseDetail> purchaseDetails = new();
    List<long> purchaseNoList = purchaseHeaders.Select(x => x.PurchaseNo).ToList();
    Dictionary<long, List<PurchaseDetail>> purchaseDetailsDictionary = (await _unitOfWork.PurchaseDetails.GetPurchaseDetailsByPurchaseNoListOrderByLinenumAsync(purchaseNoList))
                                                                 .GroupBy(item => item.PurchaseNo)
                                                                 .ToDictionary(group => group.Key, group => group.ToList());

    List<string> accountNoList = purchaseHeaders?.Select(x => x.AccountNo).ToList()!;
    Dictionary<string, string> getVendorSortConditionsByAccountNo = await _vendorSortService.GetVendorSortConditionDictionaryByAccountNoList(accountNoList);

    foreach (PurchaseHeader purchaseHeader in purchaseHeaders)
    {
      string vendorSortCondition = getVendorSortConditionsByAccountNo.TryGetValue(purchaseHeader.AccountNo, out var tempConditions) ? tempConditions : "LineNumber";

      List<PurchaseDetail>? tempPurchaseDetailsList = purchaseDetailsDictionary.TryGetValue((int)purchaseHeader.PurchaseNo, out tempPurchaseDetailsList) ? tempPurchaseDetailsList : null;
      if (tempPurchaseDetailsList == null) continue;
      tempPurchaseDetailsList = tempPurchaseDetailsList?.AsQueryable().OrderBy(vendorSortCondition).ToList();
      purchaseDetails.AddRange(tempPurchaseDetailsList!);
    }

    List<POSubTotalDataDto> poSubTotalDataDto = (await _unitOfWork.PurchaseDetails.GetCalculatedDiscountsAsync(purchaseNoList)).ToList();

    purchaseHeaders.ToList().ForEach(purchaseHeader =>
    {
      var subTotalDataDto = poSubTotalDataDto?.FirstOrDefault(x => x.PurchaseNo == purchaseHeader.PurchaseNo);
      purchaseHeader.CalculateDiscount = ((subTotalDataDto?.ExtDiscount) ?? 0).Round(2);
    });

    TransferPOFileDto? poFile = new()
    {
      PurchaseHeaders = purchaseHeaders.ToList(),
      PurchaseDetails = purchaseDetails,
      StoreCode = company?.StoreCode,
      HeaderFileName = headerFile,
      DetailFileName = detailFile,
    };

    _unitOfWork.Commit();
    return poFile;
  }

  private async Task UpdateShiftRecRepost(int? orgShiftNumber, int? orgShiftID, int? newShiftNumber, int? newShiftID)
  {
    using var uow = new UnitOfWork(_applicationConfig, _applicationConfig.CSCMasterConnectionString);
    _ = await _unitOfWork.Shift.UpdateRepostedByShiftNumberAndShiftIDAsync(shiftID: orgShiftID, shiftNumber: orgShiftNumber);
    if (newShiftNumber != orgShiftNumber || newShiftID != orgShiftID)
    {
      _ = await _unitOfWork.Shift.UpdateRepostedByShiftNumberAndShiftIDAsync(shiftID: newShiftID, shiftNumber: newShiftNumber);
    }
    uow.Commit();
  }

  private async Task UpdateMasterTables(long? purchaseNo, string? vendorID, string? postedBy)
  {
    using var uow = new UnitOfWork(_applicationConfig, _applicationConfig.CSCMasterConnectionString);
    using var uowTrans = new UnitOfWork(_applicationConfig, _applicationConfig.CSCTransConnectionString);

    List<PurchaseDetail>? purchaseDetails = _blSupportKits ?
      (await _unitOfWork.PurchaseDetails.GetPurchaseDetailsWithStockAsync((long)purchaseNo!)).ToList() :
      (await _unitOfWork.PurchaseDetails.GetPurchaseDetailsByPurchaseNoAsync((long)purchaseNo!)).ToList();

    List<Func<Task<Tuple<string, bool>>>> getKitCodeDelegates = new();
    foreach (PurchaseDetail purchaseDetail in purchaseDetails)
    {
      if (purchaseDetail.Posted != true)
        getKitCodeDelegates.Add(async () => await uow.KitMasters.GetKitCodeByStockCodeAsync(purchaseDetail.StockCode!));
    }
    Tuple<string, bool>[] kitProducts = await Task.WhenAll(getKitCodeDelegates.Select(getKitCode => getKitCode()).ToArray());

    List<Func<Task<bool>>> updateInStockDelegates = new();
    List<Func<Task<bool>>> updOrInsCostChangeDelegates = new();
    List<Func<Task<List<KitItem>>>> kitItemDelegates = new();
    List<Func<Task<List<KitItem>>>> kitItemCostChangedDelegates = new();
    List<Func<Task>> updateStockCostDelegates = new();
    List<Func<Task>> updCurStockDelegates = new();

    foreach (PurchaseDetail purchaseDetail in purchaseDetails)
    {
      Tuple<string, bool>? kitItem = kitProducts.FirstOrDefault(kit => kit.Item1 == purchaseDetail.StockCode);

      if (kitItem != null)
      {
        (string stockCode, bool isKitProduct) = kitItem;

        if (!isKitProduct)
        {
          updCurStockDelegates.Add(async () => await UpdateCurrentStock((long)purchaseNo, purchaseDetail.StockCode!, purchaseDetail.LineNumber));
          double newQty = purchaseDetail.ReceivedQuantity ?? 0;
          newQty *= (purchaseDetail.UnitQuantity ?? 1);
          if (newQty != 0)
          {
            updateInStockDelegates.Add(async () => await uow.StockBranches.UpdateInStockPOAsync(purchaseDetail.StockCode!, Math.Abs(newQty), newQty > 0 ? "+" : "-"));
          }
        }
        else
        {
          kitItemDelegates.Add(async () => await uow.KitMasters.GetKitItemsByKitCodeAsync(purchaseDetail.StockCode!));
        }

        double newUnitcost = 0;
        if (purchaseDetail.CostChanged == true)
        {
          if (!isKitProduct && purchaseDetail.PackQuantityChanged != true)
          {
            newUnitcost = purchaseDetail.Cost ?? 0;
            if (!_packCost)
              newUnitcost /= (purchaseDetail.UnitQuantity ?? 1);
          }

          if (isKitProduct)
          {
            newUnitcost = purchaseDetail.Cost ?? 0;
            kitItemCostChangedDelegates.Add(async () => await uow.KitMasters.GetKitItemsByKitCodeAsync(purchaseDetail.StockCode!));
          }
        }

        if (newUnitcost != 0)
        {
          PurchaseCostChange purchaseCostChange = new()
          {
            PurchaseNo = (long)purchaseNo!,
            StockCode = purchaseDetail.StockCode!,
            LineNum = purchaseDetail.LineNumber,
            VendorID = vendorID!,
            TransDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            PostedDate = DateTime.Today,
            NewCost = (decimal?)newUnitcost,
            PostedBy = postedBy
          };
          updOrInsCostChangeDelegates.Add(async () => await uowTrans.PurchaseHeaders.UpdateOrInsertPurchaseCostChangeAsync(purchaseCostChange));
          updateStockCostDelegates.Add(async () => await UpdateStockCost(purchaseDetail.StockCode!, (decimal)newUnitcost, vendorID!, true));
        }
      }
    }

    await Task.WhenAll(updCurStockDelegates.Select(updCurStock => updCurStock()).ToArray());
    List<KitItem>[] kitItemsArray = await Task.WhenAll(kitItemDelegates.Select(kit => kit()).ToArray());

    Dictionary<string, decimal> kitStockCodeAndQtyPair = new Dictionary<string, decimal>();

    foreach (PurchaseDetail purchaseDetail in purchaseDetails)
    {
      foreach (var kitItemList in kitItemsArray)
      {
        List<KitItem> matchingKitItems = kitItemList
            .Where(kit => kit.KitCode == purchaseDetail.StockCode)
            .ToList();

        foreach (KitItem kitItem in matchingKitItems)
        {
          if (kitItem != null)
          {
            decimal newQty = purchaseDetail.ReceivedQuantity == null ? 0 : (decimal)purchaseDetail.ReceivedQuantity * (decimal)kitItem.Quantity!;
            if (kitStockCodeAndQtyPair.ContainsKey(kitItem.StockCode))
            {
              kitStockCodeAndQtyPair[kitItem.StockCode] += newQty;
            }
            else
            {
              kitStockCodeAndQtyPair.Add(kitItem.StockCode, newQty);
            }
          }
        }
      }
    }

    foreach (var kitStockCodeAndQty in kitStockCodeAndQtyPair)
    {
      updateInStockDelegates.Add(async () => await uow.StockBranches.UpdateInStockPOAsync(kitStockCodeAndQty.Key, (double)Math.Abs(kitStockCodeAndQty.Value), kitStockCodeAndQty.Value > 0 ? "+" : "-"));
    }

    List<Task<PurchaseDetail>> receivedQtyCostTasks = new();
    List<KitItem>[] kitItemsForCostChangeArray = await Task.WhenAll(kitItemCostChangedDelegates.Select(kit => kit()).ToArray());

    foreach (PurchaseDetail purchaseDetail in purchaseDetails)
    {
      foreach (var kitItemList in kitItemsForCostChangeArray)
      {
        KitItem? kitItem = kitItemList.FirstOrDefault(kit => kit.KitCode == purchaseDetail.StockCode);
        if (kitItem != null)
        {
          receivedQtyCostTasks.Add(uow.PurchaseDetails.GetPurchaseDetailsByPurchaseNoAndStockCodeAsync(purchaseNo, kitItem.StockCode));
        }
      }
    }

    PurchaseDetail[] purchaseDetailsListByPurchaseNoAndStockCode = await Task.WhenAll(receivedQtyCostTasks);
    purchaseDetailsListByPurchaseNoAndStockCode = purchaseDetailsListByPurchaseNoAndStockCode.Where(purchaseDetail => purchaseDetail != null).ToArray();

    foreach (PurchaseDetail purchaseDetail in purchaseDetails)
    {
      foreach (var kitItemList in kitItemsForCostChangeArray)
      {
        KitItem? kitItem = kitItemList.FirstOrDefault(kit => kit.KitCode == purchaseDetail.StockCode);

        if (kitItem != null)
        {
          decimal costInPurchaseDetails = 0;
          decimal quantityInPurchaseDetails = 0;
          var qtyInKit = kitItem.Quantity;

          PurchaseDetail? purchaseDetailsByPurchaseNoAndStockCode = purchaseDetailsListByPurchaseNoAndStockCode.FirstOrDefault(rsIndpurch => rsIndpurch.StockCode == purchaseDetail.StockCode);

          if (purchaseDetailsByPurchaseNoAndStockCode != null)
          {
            quantityInPurchaseDetails = (decimal)(purchaseDetailsByPurchaseNoAndStockCode.ReceivedQuantity ?? 0);
            costInPurchaseDetails = (decimal)(purchaseDetailsByPurchaseNoAndStockCode.Cost ?? 0);
          }

          decimal NewUnitcost = 0;
          decimal divisor = ((decimal)purchaseDetail.ReceivedQuantity! * (decimal)qtyInKit! + quantityInPurchaseDetails)!;

          if (divisor != 0)
          {
            NewUnitcost = (((decimal)purchaseDetail.Cost! * (decimal)purchaseDetail.ReceivedQuantity! + quantityInPurchaseDetails * costInPurchaseDetails) / divisor).Round(2);
          }
          else
          {
            NewUnitcost = (decimal)(purchaseDetail.Cost ?? 0);
          }

          updateStockCostDelegates.Add(async () => await UpdateStockCost(kitItem.StockCode, NewUnitcost, vendorID!));
        }
      }
    }

    await Task.WhenAll(updateInStockDelegates.Select(updInStock => updInStock()).ToArray());
    await Task.WhenAll(updateStockCostDelegates.Select(updStockCost => updStockCost()).ToArray());
    await Task.WhenAll(updOrInsCostChangeDelegates.Select(updOrInsCostChange => updOrInsCostChange()).ToArray());
    uowTrans.Commit();
    uow.Commit();
  }

  private async Task UpdateCurrentStock(long purchaseNo, string stockCode, int lineNum)
  {

    var uowMaster = new UnitOfWork(_applicationConfig, _applicationConfig.CSCMasterConnectionString);
    decimal? inStock = await uowMaster.StockBranches.GetInStockByStockCodeAsync(stockCode);
    uowMaster.Commit();

    var uowTrans = new UnitOfWork(_applicationConfig, _applicationConfig.CSCTransConnectionString);
    _ = await uowTrans.PurchaseHeaders.UpdateCurrentStockByPurchaseNoAndLineNumAsync(purchaseNo: purchaseNo, lineNum: lineNum, currentStock: inStock ?? 0);
    uowTrans.Commit();
  }

  private async Task UpdateStockCost(string stockCode, decimal stockCost, string vendorID, bool all = false)
  {
    string activeVendorID = await _unitOfWork.StockCosts.GetVendorIDByStockCodeAsync(stockCode: stockCode, activeVendor: "1", orderNo: "0");
    string activeVendor = string.IsNullOrEmpty(activeVendorID) ? "NONE" : activeVendorID;

    _ = await _unitOfWork.StockCosts.UpdateOrInsertStockCostAsync(stockCost: (double)stockCost, stockCode: stockCode, vendorID: vendorID, orderNo: "0");
    if (all)
    {
      _ = await _unitOfWork.StockCosts.UpdateOrInsertStockCostAsync(stockCost: (double)stockCost, stockCode: stockCode, vendorID: "ALL", orderNo: "0");
    }

    if (vendorID == activeVendor)
    {
      if (_updateAllVendor)
      {
        _ = await _unitOfWork.StockCosts.UpdateCostAsync(stockCost: (double)stockCost, stockCode: stockCode, vendorID: "ALL", orderNo: "0");
      }

      _ = await _unitOfWork.StockItems.UpdateStockMasterAsync(stockCost: stockCost, stockCode: stockCode);
    }

    if (_PChangeInPO)
    {
      List<(double, string, string)> costStockCodeAndVendorIdList = new()
      {
        ((double)stockCost, stockCode, vendorID)
      };

      _ = await _stockTakesAndReceivingService.UpdatePriceRecords(costStockCodeAndVendorIdList, isPriceChange: _blPChange_Cost,
        isCostMarkupPolicy: false, updateNetworkPolicy: _updateAllVendor, setVendPolicy: _setVend, numPricePolicy: 0);
    }
  }
}
