namespace InfonetCSCR.Services;

public class PurchaseDetailService : IPurchaseDetailService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IStockItemService _stockItemService;
  private readonly IStockTaxService _stockTaxService;
  private readonly IApplicationConfig _applicationConfig;
  private static string currentVendorId = null;

  public PurchaseDetailService(
    IApplicationConfig applicationConfig,
    IStockItemService stockItemService,
    IStockTaxService stockTaxService)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
    _stockItemService = stockItemService;
    _stockTaxService = stockTaxService;
    _applicationConfig = applicationConfig;
  }

  #region Constants

  const double minSmallMoneyValue = -214748.3648;
  const double maxSmallMoneyValue = 214748.3647;

  #endregion

  #region Private Methods

  private double CalculateDiscount(double poDiscount, double cost, double? depositQuantity)
  {
    double discount = 0;
    if (poDiscount != 0)
    {
      discount = (((cost - depositQuantity ?? 0) / (1 - poDiscount) * poDiscount).Round(4));
    }
    return discount;
  }

  private double CalculateCost(PurchaseDetail purchaseDetail, double poDiscount, double taxRate, bool isTaxIncluded)
  {
    double cost = 0;
    if (poDiscount != 0)
    {
      cost = (double)(purchaseDetail.PoCost * (1 - poDiscount) + ((purchaseDetail.Charges * purchaseDetail.UnitQuantity * poDiscount) / purchaseDetail.UnitQuantity));
      cost = cost.Round(2);
    }
    else
    {
      if (isTaxIncluded)
      {
        cost = (double)(((purchaseDetail.PoCost - purchaseDetail.Charges) / (1 + taxRate)) + purchaseDetail.Charges);
        cost = cost.Round(2);
      }
      else
      {
        cost = ((double)purchaseDetail.PoCost).Round(2);
      }
    }

    return cost;
  }

  double CalculateGST(IList<double> taxRates, string exptTaxPolicy, double? extCost, int? costDec = 2)
  {
    double calculatedGST = 0;
    foreach (double taxRate in taxRates)
    {
      double tmpTaxRate = taxRate;
      calculatedGST = (double)(calculatedGST + (extCost * tmpTaxRate / 100));
      calculatedGST = calculatedGST.Round(costDec ?? 2);
    }

    return calculatedGST;
  }

  private double CalculateItemsGstRate(IList<double> taxRates)
  {
    double itemsGstRate = 0;
    foreach (double taxRate in taxRates)
    {
      double tmpTaxRate = taxRate;
      itemsGstRate = (double)(itemsGstRate + (tmpTaxRate / 100));
    }

    itemsGstRate = itemsGstRate.Round(2);
    return itemsGstRate;
  }

  private double CalculateItemsHstRate(IList<double> taxRates, int? costDecPolicy)
  {
    double itemsHstRate = 0;
    foreach (double taxRate in taxRates)
    {
      double tmpTaxRate = taxRate;
      itemsHstRate = (double)(itemsHstRate + (tmpTaxRate / 100));
    }

    itemsHstRate = itemsHstRate.Round((int)costDecPolicy);
    return itemsHstRate;
  }

  private double CalculateTax(double itemsTaxRate, double? amount)
  {
    double? calculatedTax = amount - amount / (1 + itemsTaxRate);

    calculatedTax = ((double)calculatedTax).Round(2);
    return (double)calculatedTax;
  }

  private double CalculateTax(double itemsGSTRate, double itemsHSTRate, double? amount)
  {
    double itemsTaxRate = itemsGSTRate + itemsHSTRate;
    double? calculatedTax = amount - amount / (1 + itemsTaxRate);

    calculatedTax = ((double)calculatedTax).Round(2);
    return (double)calculatedTax;
  }

  private double CalculateItemsTaxRate(TaxAmountCalculatorDataRecord taxAmountCalculatorDataRecord)
  {
    string stockCode = taxAmountCalculatorDataRecord.PurchaseDetail.StockCode;
    PurchasingAndReceivingPoliciesDto purchasingAndReceivingPoliciesDto =
      taxAmountCalculatorDataRecord.PurchasingAndReceivingPolicies;

    IDictionary<string, IList<double>>? getTaxRatesByStockCodesForGSTDictionary =
      taxAmountCalculatorDataRecord.GetTaxRatesByStockCodesForGSTDictionary;

    IDictionary<string, IList<double>>? getTaxRatesByStockCodesForHSTDictionary =
        taxAmountCalculatorDataRecord.GetTaxRatesByStockCodesForHSTDictionary;

    IList<double> taxRatesGST = new List<double>();
    if (stockCode != null
      && getTaxRatesByStockCodesForGSTDictionary.TryGetValue(
        stockCode,
        out IList<double> tmpTaxRatesGST))
    {
      taxRatesGST = tmpTaxRatesGST;
    }

    IList<double> taxRatesHST = new List<double>();
    if (stockCode != null
      && getTaxRatesByStockCodesForHSTDictionary.TryGetValue(
        stockCode,
        out IList<double> tmpTaxRatesHST))
    {
      taxRatesHST = tmpTaxRatesHST;
    }

    double itemsGSTRate = CalculateItemsGstRate(taxRatesGST);
    double itemsHSTRate = CalculateItemsHstRate(taxRatesHST, purchasingAndReceivingPoliciesDto.CostDecPolicy);

    return itemsGSTRate + itemsHSTRate;
  }

  private double CalculateTaxAmount(TaxAmountCalculatorDataRecord taxAmountCalculatorDataRecord)
  {
    double poDiscount = taxAmountCalculatorDataRecord.PoDiscount;
    bool isTaxIncluded = taxAmountCalculatorDataRecord.IsTaxIncluded;
    PurchaseDetail purchaseDetail = taxAmountCalculatorDataRecord.PurchaseDetail;
    PurchasingAndReceivingPoliciesDto purchasingAndReceivingPoliciesDto =
      taxAmountCalculatorDataRecord.PurchasingAndReceivingPolicies;

    IDictionary<string, IList<double>>? getTaxRatesByStockCodesForGSTDictionary =
      taxAmountCalculatorDataRecord.GetTaxRatesByStockCodesForGSTDictionary;

    IDictionary<string, IList<double>>? getTaxRatesByStockCodesForHSTDictionary =
        taxAmountCalculatorDataRecord.GetTaxRatesByStockCodesForHSTDictionary;

    IDictionary<string, IList<double>>? getTaxRatesByStockCodesDictionary =
         taxAmountCalculatorDataRecord.GetTaxRatesByStockCodesDictionary;

    double? depositQuantity = (purchaseDetail.UnitQuantity * purchaseDetail.ReceivedQuantity);

    if (poDiscount != 0)
    {
      double? tmpExtCost = (double?)(purchaseDetail.ExtCost
        - (purchaseDetail.CalculateDiscount * purchaseDetail.ReceivedQuantity)
        - (purchaseDetail.Charges * depositQuantity));


      IList<double> taxRates = new List<double>();
      if (purchaseDetail.StockCode != null
        && getTaxRatesByStockCodesDictionary != null
        && getTaxRatesByStockCodesDictionary.TryGetValue(purchaseDetail.StockCode, out IList<double>? tmpTaxRates))
      {
        taxRates = tmpTaxRates;
      }

      purchaseDetail.GST = CalculateGST(
        taxRates,
        purchasingAndReceivingPoliciesDto.ExptTaxPolicy,
        tmpExtCost);
    }
    else
    {
      if (isTaxIncluded)
      {
        double? tmpExtCost = purchaseDetail.ExtCost
          - (purchaseDetail.Charges * depositQuantity);

        double itemsTaxRate = CalculateItemsTaxRate(taxAmountCalculatorDataRecord);
        purchaseDetail.GST = CalculateTax(itemsTaxRate, tmpExtCost);
      }
      else
      {
        IList<double> taxRates = new List<double>();
        if (purchaseDetail.StockCode != null
          && getTaxRatesByStockCodesDictionary.TryGetValue(purchaseDetail.StockCode, out IList<double> tmpTaxRates))
        {
          taxRates = tmpTaxRates;
        }

        purchaseDetail.GST = CalculateGST(
          taxRates,
          purchasingAndReceivingPoliciesDto.ExptTaxPolicy,
          purchaseDetail.ExtCost);
      }
    }

    purchaseDetail.GST = ((double)purchaseDetail.GST).Round(2);

    return (double)purchaseDetail.GST;
  }

  double GetVendorUnitCost(
    string stockCode,
    IDictionary<string, StockCostInfo> stockCostInfoDictionary,
    PurchasingAndReceivingPoliciesDto? purchasingAndReceivingPoliciesDto
    )
  {
    StockCostInfo stockCostInfo = null;

    if (stockCode != null
      && stockCostInfoDictionary.TryGetValue(stockCode,
      out StockCostInfo? tmpStockCostInfo))
    {
      stockCostInfo = tmpStockCostInfo;
    }

    double vendorUnitCost = 0;
    if (stockCostInfo != null)
    {
      vendorUnitCost = (stockCostInfo.Cost == null) ? 0 : (double)stockCostInfo.Cost;
    }

    vendorUnitCost = vendorUnitCost.Round(purchasingAndReceivingPoliciesDto.CostDecPolicy);
    vendorUnitCost = vendorUnitCost.Round(4);

    return vendorUnitCost;
  }

  double CalculatePackCost(
    PurchasingAndReceivingPoliciesDto purchasingAndReceivingPolicies,
    double packing,
    double vendorUnitCost)
  {
    double packCost = 0;
    if (purchasingAndReceivingPolicies.PackCostPolicy)
    {
      if (packing == null || packing == 0)
      {
        packing = 1;
      }

      packCost = vendorUnitCost * packing;
    }
    else
    {
      packCost = vendorUnitCost;
    }

    packCost = packCost.Round(purchasingAndReceivingPolicies.CostDecPolicy);
    return packCost;
  }

  private PurchaseDetail UpdateDataInPurchaseDetailModel(PurchaseDetailUpdateParameterRecord purchaseDetailUpdateParameterRecord)
  {
    PurchaseDetail? purchaseDetail = purchaseDetailUpdateParameterRecord.PurchaseDetail;

    IDictionary<string, IList<double>>? getTaxRatesByStockCodesForGSTDictionary =
      purchaseDetailUpdateParameterRecord.GetTaxRatesByStockCodesForGSTDictionary;

    IDictionary<string, IList<double>>? getTaxRatesByStockCodesForHSTDictionary =
      purchaseDetailUpdateParameterRecord.GetTaxRatesByStockCodesForHSTDictionary;

    IDictionary<string, IList<double>>? getTaxRatesByStockCodesDictionary =
      purchaseDetailUpdateParameterRecord.GetTaxRatesByStockCodesDictionary;

    IDictionary<string, double> getChargeByPartNoDictionary =
      purchaseDetailUpdateParameterRecord.GetChargeByPartNoDictionary;

    IDictionary<string, StockCostInfo> stockCostInfoDictionary =
      purchaseDetailUpdateParameterRecord.StockCostInfoDictionary;

    PurchasingAndReceivingPoliciesDto? purchasingAndReceivingPoliciesDto =
      purchaseDetailUpdateParameterRecord.PurchasingAndReceivingPoliciesDto;

    double poDiscount = purchaseDetailUpdateParameterRecord.PoDiscount;
    bool isTaxIncluded = purchaseDetailUpdateParameterRecord.IsTaxIncluded;

    if (purchaseDetail.PartNumber != null
      && getChargeByPartNoDictionary.TryGetValue(
        purchaseDetail.PartNumber, out double tmpCharge))
    {
      purchaseDetail.Charges = tmpCharge;
    }
    else
    {
      purchaseDetail.Charges = 0;
    }

    double vendorUnitCost = GetVendorUnitCost(purchaseDetail.StockCode, stockCostInfoDictionary, purchasingAndReceivingPoliciesDto);
    double packCost = (double)purchaseDetail.PoCost;

    if (purchaseDetailUpdateParameterRecord.FieldDataUpdated == (int)PurchaseDetailEnum.PackingQuantity)
    {
      packCost = CalculatePackCost(purchasingAndReceivingPoliciesDto, (double)purchaseDetail.UnitQuantity, vendorUnitCost);
    }

    TaxAmountCalculatorDataRecord taxAmountCalculatorDataRecord = new TaxAmountCalculatorDataRecord()
    {
      PoDiscount = poDiscount,
      IsTaxIncluded = isTaxIncluded,
      PurchaseDetail = purchaseDetail,
      PurchasingAndReceivingPolicies = purchasingAndReceivingPoliciesDto,
      GetTaxRatesByStockCodesForGSTDictionary = getTaxRatesByStockCodesForGSTDictionary,
      GetTaxRatesByStockCodesForHSTDictionary = getTaxRatesByStockCodesForHSTDictionary,
      GetTaxRatesByStockCodesDictionary = getTaxRatesByStockCodesDictionary,
    };

    double itemsTaxRate = CalculateItemsTaxRate(taxAmountCalculatorDataRecord);
    double cost = CalculateCost(purchaseDetail, poDiscount, itemsTaxRate, isTaxIncluded);
    double calculateDiscount = CalculateDiscount(poDiscount, cost, purchaseDetail.Charges * purchaseDetail.UnitQuantity);

    if (purchaseDetailUpdateParameterRecord.FieldDataUpdated == (int)PurchaseDetailEnum.OrderQuantity)
    {
      purchaseDetail.ReceivedQuantity = purchaseDetail.OrderedQuantity;
    }

    if (purchaseDetailUpdateParameterRecord.FieldDataUpdated == (int)PurchaseDetailEnum.OrderQuantity
      || purchaseDetailUpdateParameterRecord.FieldDataUpdated == (int)PurchaseDetailEnum.PackingQuantity
      || purchaseDetailUpdateParameterRecord.FieldDataUpdated == (int)PurchaseDetailEnum.ReceivedQuantity
      || purchaseDetailUpdateParameterRecord.FieldDataUpdated == (int)PurchaseDetailEnum.Cost
      || (purchaseDetailUpdateParameterRecord.FieldDataUpdated == (int)PurchaseDetailEnum.ApplyGST && purchaseDetail.ApplyGST == true))
    {
      purchaseDetail.ExtCost =
        ((purchaseDetail.ReceivedQuantity ?? 0) * packCost).Round(purchasingAndReceivingPoliciesDto.CostDecPolicy);
      purchaseDetail.GST = CalculateTaxAmount(taxAmountCalculatorDataRecord);
      purchaseDetail.ApplyGST = purchaseDetail.GST != 0;
    }

    if (purchaseDetailUpdateParameterRecord.FieldDataUpdated == (int)PurchaseDetailEnum.PackingQuantity)
    {
      purchaseDetail.PoCost = packCost.Round(4);
      purchaseDetail.PackQuantityChanged = true;
    }

    if (purchaseDetailUpdateParameterRecord.FieldDataUpdated == (int)PurchaseDetailEnum.Cost)
    {
      purchaseDetail.Cost = cost;
      purchaseDetail.CalculateDiscount = calculateDiscount;
      purchaseDetail.CostChanged = true;
    }

    if (purchaseDetailUpdateParameterRecord.FieldDataUpdated == (int)PurchaseDetailEnum.ApplyGST
      && purchaseDetail.ApplyGST == false)
    {
      purchaseDetail.GST = 0;
    }

    return purchaseDetail;
  }

  private async Task<string> GetVendorSortCondition(long purchaseNo)
  {
    PurchaseHeader? purchaseHeader = await _unitOfWork.PurchaseHeaders.GetPurchaseHeadersByPurchaseNoAsync(purchaseNo);
    if (purchaseHeader == null)
    {
      return null;
    }

    IEnumerable<VendorSort> vendorSorts = await _unitOfWork.VendorSorts.GetVendorSortsAsync(purchaseHeader.AccountNo);
    IList<string> sortItems = new List<string>();

    foreach (VendorSort vendorSort in vendorSorts)
    {
      sortItems.Add(vendorSort.SortItem);
    }

    IEnumerable<VendorSortSetup> vendorSortSetups =
      await _unitOfWork.VendorSorts.GetVendorSortSetupsBySortItemsAsync(sortItems);

    IDictionary<string, VendorSortSetup> getVendorSortSetupsBySortItemsDictionary =
      new Dictionary<string, VendorSortSetup>();

    foreach (VendorSortSetup vendorSortSetup in vendorSortSetups)
    {
      getVendorSortSetupsBySortItemsDictionary.Add(vendorSortSetup.SortItem, vendorSortSetup);
    }

    string vendorSortCondition = "";
    foreach (VendorSort vendorSort in vendorSorts)
    {
      VendorSortSetup vendorSortSetup = null;

      if (getVendorSortSetupsBySortItemsDictionary.TryGetValue(vendorSort.SortItem,
        out VendorSortSetup tmpVendorSortSetup))
      {
        vendorSortSetup = tmpVendorSortSetup;
      }

      if (vendorSortSetup != null)
      {
        if (vendorSortCondition == "")
        {
          vendorSortCondition += $"{vendorSortSetup.LinkTable}.{vendorSortSetup.LinkField} {(vendorSort.Ascending == true ? "ASC" : "DESC")}";
        }
        else
        {
          vendorSortCondition += $", {vendorSortSetup.LinkTable}.{vendorSortSetup.LinkField} {(vendorSort.Ascending == true ? "ASC" : "DESC")}";
        }
      }
    }

    return vendorSortCondition;
  }

  #endregion

  public async Task<double> CalculatePOSubTotal(string purchaseNo, string vendorId, bool useCurrentVendor = false)
  {
    using (var uow = new UnitOfWork(_applicationConfig, _applicationConfig.CSCMasterConnectionString))
    {
      POSubTotalDataDto poSubTotalDataDto = await uow.PurchaseDetails.GetPOSubTotalDataAsync(purchaseNo);
      if (currentVendorId == null || useCurrentVendor == false)
      {
        currentVendorId = vendorId;
      }
      double poDiscount = await uow.Vendors.GetPoDiscountAsync(currentVendorId);
      bool isTaxIncluded = await uow.Vendors.GetIsTaxIncludeInPOAsync(currentVendorId);
      double calculatePOSubTotal = 0.00;

      if (poSubTotalDataDto != null)
      {
        if (poDiscount != 0)
        {
          calculatePOSubTotal = poSubTotalDataDto.SubTotal - poSubTotalDataDto.ExtDiscount;
        }
        else
        {
          if (isTaxIncluded)
          {
            calculatePOSubTotal = poSubTotalDataDto.SubTotal - poSubTotalDataDto.GSTTotal;
          }
          else
          {
            calculatePOSubTotal = poSubTotalDataDto.SubTotal;
          }
        }
      }

      double calculatePOSubTotalFormateNumerator = calculatePOSubTotal * 100;
      double calculatePOSubTotalFormate = calculatePOSubTotalFormateNumerator / 100;
      calculatePOSubTotal = calculatePOSubTotalFormate;
      calculatePOSubTotal = calculatePOSubTotal.Round(2);

      uow.Commit();
      return calculatePOSubTotal;
    }
  }

  public async Task<bool> AddPurchaseDetails(
    long purchanseNo,
    string vendorId,
    PurchasingAndReceivingPoliciesDto purchasingAndReceivingPoliciesDto,
    Vendor vendor)
  {
    await _unitOfWork.PurchaseDetails.DeletePurchaseDetailByPurchaseNoAsync(purchanseNo.ToString());

    if (vendor.PoDiscount == null)
    {
      vendor.PoDiscount = 0.00;
    }
    vendor.PoDiscount = ((double)vendor.PoDiscount).Round(2);
    double poDiscount = (double)vendor.PoDiscount;
    bool isTaxIncluded = (bool)vendor.PoCostTaxInclude;

    int lineNumber = 0;

    // List of Purchase Details to Add in PurchaiseDetails Table
    IList<PurchaseDetail> purchaseDetails = new List<PurchaseDetail>();

    // Prepare Vendor Sort Condition data Start

    IEnumerable<VendorSort> vendorSorts = await _unitOfWork.VendorSorts.GetVendorSortsAsync(vendorId);
    IList<string> sortItems = new List<string>();

    foreach (VendorSort vendorSort in vendorSorts)
    {
      sortItems.Add(vendorSort.SortItem);
    }

    IEnumerable<VendorSortSetup> vendorSortSetups =
      await _unitOfWork.VendorSorts.GetVendorSortSetupsBySortItemsAsync(sortItems);

    IDictionary<string, VendorSortSetup> getVendorSortSetupsBySortItemsDictionary =
      new Dictionary<string, VendorSortSetup>();

    foreach (VendorSortSetup vendorSortSetup in vendorSortSetups)
    {
      getVendorSortSetupsBySortItemsDictionary.Add(vendorSortSetup.SortItem, vendorSortSetup);
    }

    string vendorSortCondition = "";
    foreach (VendorSort vendorSort in vendorSorts)
    {
      VendorSortSetup vendorSortSetup = null;

      if (vendorSort.SortItem != null && getVendorSortSetupsBySortItemsDictionary.TryGetValue(vendorSort.SortItem,
        out VendorSortSetup tmpVendorSortSetup))
      {
        vendorSortSetup = tmpVendorSortSetup;
      }

      if (vendorSortSetup != null)
      {
        if (vendorSortCondition == "")
        {
          vendorSortCondition += $"{vendorSortSetup.LinkTable}.{vendorSortSetup.LinkField} {(vendorSort.Ascending == true ? "ASC" : "DESC")}";
        }
        else
        {
          vendorSortCondition += $", {vendorSortSetup.LinkTable}.{vendorSortSetup.LinkField} {(vendorSort.Ascending == true ? "ASC" : "DESC")}";
        }
      }
    }

    // Prepare Vendor Sort Condition data End

    // Get Vendor Products for Vendor Part Start

    // Get vendor parts for vendorId
    IEnumerable<VendorPart> vendorParts =
      await _unitOfWork.VendorParts.GetVendorPartsFromVendorPartsAndStockMstAsync(vendorSortCondition, vendorId);

    // Prepare Query Parameter by vendor Parts to fetch Vendor Product data
    IList<VendorProductQueryParameterDto> vendorProductQueryParameters = new List<VendorProductQueryParameterDto>();
    foreach (VendorPart vendorPart in vendorParts)
    {
      vendorProductQueryParameters.Add(new VendorProductQueryParameterDto
      {
        StockCode = vendorPart.StockCode,
        VendorId = vendorId,
        PartNo = vendorPart.PartNo
      });
    }

    // Fetch vendor products data using vendor product query parameter prepared from vendor part data
    IEnumerable<VendorProductDto> TempVendorProducts = await _unitOfWork.StockItems.GetVendorProducts(vendorProductQueryParameters);
    IList<VendorProductDto> vendorProducts = new List<VendorProductDto>();

    foreach (VendorProductQueryParameterDto vendorProductQueryParameter in vendorProductQueryParameters)
    {
      VendorProductDto vendorProduct = TempVendorProducts.FirstOrDefault(item => item.StockCode ==  vendorProductQueryParameter.StockCode);
      if (vendorProduct != null)
      {
        vendorProducts.Add(new VendorProductDto
        {
          StockCode = vendorProduct.StockCode,
          ActiveStock = vendorProduct.ActiveStock,
          Description = vendorProduct.Description,
          UnitofMeasure = vendorProduct.UnitofMeasure,
          OrderUnit = vendorProduct.OrderUnit,
          Packing = vendorProduct.Packing,
          Department = vendorProduct.Department,
          SubDepartment = vendorProduct.SubDepartment,
          PartNo = vendorProductQueryParameter.PartNo,
        });
      }
    }

    // Get Vendor Products for Vendor Part End

    // Load StockCodes and PartNos from Vendor Product to fetch different data from tables Start

    HashSet<string> partNosForVendorParts = new HashSet<string>();
    HashSet<string> stockCodes = new HashSet<string>();
    foreach (VendorProductDto vendorProduct in vendorProducts)
    {
      stockCodes.Add(vendorProduct.StockCode);
      partNosForVendorParts.Add(vendorProduct.PartNo);
    }

    // Load StockCodes and PartNos from Vendor Product to fetch different data from tables End


    // Get Tax rates to calculate GST
    IDictionary<string, IList<double>> getTaxRatesByStockCodesForGSTDictionary =
      await _stockTaxService.GetTaxRatesByStockCodes(purchasingAndReceivingPoliciesDto.ExptTaxPolicy, stockCodes, false);

    // Get Tax rates to calculate HST
    IDictionary<string, IList<double>> getTaxRatesByStockCodesForHSTDictionary =
      await _stockTaxService.GetTaxRatesByStockCodes(purchasingAndReceivingPoliciesDto.ExptTaxPolicy, stockCodes, true);

    // Get Tax rates for stock codes for calculating GST or Tax where Tax is not included (isTaxInclude = false)
    IDictionary<string, IList<double>> getTaxRatesByStockCodesDictionary =
      await _stockTaxService.GetTaxRatesByStockCodes(purchasingAndReceivingPoliciesDto.ExptTaxPolicy, stockCodes);

    // Get kit items for stock code and store them in dictionary Start

    Dictionary<string, KitItem> kitItemsDictionary = new Dictionary<string, KitItem>();

    IEnumerable<KitItem> kitItems = await _unitOfWork.KitItems.GetKitItemByStockCodesAsync(stockCodes.ToList());
    foreach (KitItem kitItem in kitItems)
    {
      if (kitItem.StockCode != null && !kitItemsDictionary.TryGetValue(kitItem.StockCode, out KitItem tmpKitItem))
      {
        kitItemsDictionary.Add(kitItem.StockCode, kitItem);
      }
    }

    // Get kit items for stock code and store them in dictionary End

    Dictionary<string, bool> kitOrderDictionary = new Dictionary<string, bool>();

    // Parameter to get Order quantity data for stock codes
    IList<string> stockCodesOrderQuantityParameter = new List<string>();

    // Parameter to get Vendor part data of kit items
    IList<(string VendorId, string StockCode)> vendorIdStockCodePairListForProductAndVendorPart =
      new List<(string VendorId, string StockCode)>();

    // Load kitItemsDictionary, stockCodesOrderQuantityParameter and vendorIdStockCodePairListForProductAndVendorPart
    foreach (string stockCode in stockCodes)
    {
      if (purchasingAndReceivingPoliciesDto.KitsInPoPolicy)
      {
        if (stockCode != null && kitItemsDictionary.TryGetValue(stockCode, out KitItem kitItem))
        {
          kitOrderDictionary.Add(stockCode, true);
          stockCodesOrderQuantityParameter.Add(stockCode);
          vendorIdStockCodePairListForProductAndVendorPart.Add((vendorId, kitItem.KitCode));
        }
        else
        {
          kitOrderDictionary.Add(stockCode, false);
          stockCodesOrderQuantityParameter.Add(stockCode);
        }
      }
      else
      {
        kitOrderDictionary.Add(stockCode, false);
        stockCodesOrderQuantityParameter.Add(stockCode);
      }
    }

    // Get Order quantities
    Dictionary<string, int> orderQuantitiesDictionary = await _stockItemService.GetOrderQuantities(
      stockCodesOrderQuantityParameter,
      kitOrderDictionary,
      purchasingAndReceivingPoliciesDto.KitsInPoPolicy);

    // Get Product And VendorPart Data Of KitItems
    IDictionary<(string VendorId, string StockCode), ProductAndVendorPartDataOfKitItemDto> productAndVendorPartDataOfKitItemDictionary =
      new Dictionary<(string VendorId, string StockCode), ProductAndVendorPartDataOfKitItemDto>();

    IEnumerable<ProductAndVendorPartDataOfKitItemDto> productAndVendorPartDataOfKitItems =
      await _unitOfWork.VendorParts.GetProductAndVendorPartDataOfKitItemsAsync(vendorIdStockCodePairListForProductAndVendorPart);

    foreach (ProductAndVendorPartDataOfKitItemDto productAndVendorPartDataOfKitItem in productAndVendorPartDataOfKitItems)
    {
      string tmpVendorId = productAndVendorPartDataOfKitItem.VendorId;
      string stockCode = productAndVendorPartDataOfKitItem.StockCode;

      productAndVendorPartDataOfKitItemDictionary.Add((tmpVendorId, stockCode), productAndVendorPartDataOfKitItem);
      partNosForVendorParts.Add(productAndVendorPartDataOfKitItem.PartNo);
    }

    // Get Product And VendorPart Data Of KitItems End

    // Get Stock costs data by stock codes Start

    StockCostInfoBatchParameterBackOfficeDto stockCostInfoBatchParameterBackOfficDto = new()
    {
      OrderNo = 0,
      VendorId = vendorId,
      StockCodes = stockCodes.ToList()
    };

    Dictionary<string, StockCostInfo> stockCostInfoDictionary = new Dictionary<string, StockCostInfo>();

    IEnumerable<StockCostInfo?> stockCostInfos =
      await _unitOfWork.StockCosts.GetStockCostInfoBatchForBackOfficeAsync(stockCostInfoBatchParameterBackOfficDto);

    foreach (StockCostInfo stockCostInfo in stockCostInfos)
    {
      stockCostInfoDictionary.Add(stockCostInfo.StockCode, stockCostInfo);
    }

    // Get Stock costs data by stock codes End

    // Generate Dictionary to Get Charge By Part no Start

    // Get vendor parts by part no
    IEnumerable<VendorPart> vendorPartsForPartNos =
      await _unitOfWork.VendorParts.GetVendorPartsByPartNosAsync(partNosForVendorParts.ToList());

    IDictionary<string, string> getPartNoByStockCodeDictionary = new Dictionary<string, string>();
    IList<string> stockCodesForCharges = new List<string>();

    // Generate dictionary to get part o by stock code and genrate parameter to get charges by stock code
    foreach (VendorPart vendorPart in vendorPartsForPartNos)
    {
      if (vendorPart.StockCode != null && !getPartNoByStockCodeDictionary.TryGetValue(vendorPart.StockCode, out string value))
      {
        getPartNoByStockCodeDictionary.Add(vendorPart.StockCode, vendorPart.PartNo);
        stockCodesForCharges.Add(vendorPart.StockCode);
      }
    }

    // Get charges by stock code
    IEnumerable<(string stockCode, double charge)> stockCodeAndChargePairs =
      await _unitOfWork.Associations.GetStockCodeAndAssociationsChargeByStockCodesAsync(stockCodesForCharges);
    IDictionary<string, double> getChargeByPartNoDictionary = new Dictionary<string, double>();

    // Generate dictionary to get charge by part no
    foreach (var stockCodeAndChargePair in stockCodeAndChargePairs)
    {

      if (stockCodeAndChargePair.stockCode != null && getPartNoByStockCodeDictionary.TryGetValue(stockCodeAndChargePair.stockCode, out string tmpPartNo))
      {
        if (tmpPartNo != null && !getChargeByPartNoDictionary.TryGetValue(tmpPartNo, out double value))
        {
          getChargeByPartNoDictionary.Add(tmpPartNo, stockCodeAndChargePair.charge);
        }
      }
    }

    // Generate Dictionary to Get Charge By Part no End

    // Generate Purchase Details list to add in PurchaseDetails Table Start

    foreach (VendorProductDto vendorProduct in vendorProducts)
    {
      string kitProduct = "";
      bool kitOrder = false;
      int orderQuantity = 0;

      if (vendorProduct.StockCode != null && orderQuantitiesDictionary.TryGetValue(vendorProduct.StockCode, out int value))
      {
        orderQuantity = orderQuantitiesDictionary[vendorProduct.StockCode];
      }

      if (purchasingAndReceivingPoliciesDto.KitsInPoPolicy)
      {
        if (vendorProduct.StockCode != null && kitItemsDictionary.TryGetValue(vendorProduct.StockCode, out KitItem kitItem))
        {
          kitOrder = true;
          kitProduct = kitItem.KitCode;
          orderQuantity = orderQuantity / (int)kitItem.Quantity;
        }
      }

      if (orderQuantity != 0)
      {
        PurchaseDetail purchaseDetail = new PurchaseDetail();
        lineNumber++;

        purchaseDetail.PurchaseNo = purchanseNo;
        purchaseDetail.LineNumber = lineNumber;

        if (kitOrder)
        {
          ProductAndVendorPartDataOfKitItemDto productAndVendorPartDataOfKitItem = null;
          if (vendorId != null && productAndVendorPartDataOfKitItemDictionary.TryGetValue((vendorId, kitProduct),
            out ProductAndVendorPartDataOfKitItemDto? tmpProductAndVendorPartDataOfKitItem))
          {
            productAndVendorPartDataOfKitItem = tmpProductAndVendorPartDataOfKitItem;
          }

          if (productAndVendorPartDataOfKitItem != null)
          {
            purchaseDetail.PartNumber = productAndVendorPartDataOfKitItem.PartNo;
            purchaseDetail.Description = productAndVendorPartDataOfKitItem.Description;
            purchaseDetail.UnitOfMeasure = productAndVendorPartDataOfKitItem.OrderUnit ?? "";
            purchaseDetail.UnitQuantity = productAndVendorPartDataOfKitItem.Packing;
            purchaseDetail.OrderedQuantity = orderQuantity;
            purchaseDetail.ReceivedQuantity = orderQuantity;
            purchaseDetail.PartNumber = productAndVendorPartDataOfKitItem.PartNo;

            StockCostInfo stockCostInfo = null;

            if (productAndVendorPartDataOfKitItem.StockCode != null && stockCostInfoDictionary.TryGetValue(productAndVendorPartDataOfKitItem.StockCode,
            out StockCostInfo? tmpStockCostInfo))
            {
              stockCostInfo = tmpStockCostInfo;
            }

            double vendorUnitCost = (stockCostInfo.Cost == null) ? 0 : (double)stockCostInfo.Cost;
            vendorUnitCost = Math.Floor(vendorUnitCost * Math.Pow(10, (double)purchasingAndReceivingPoliciesDto.CostDecPolicy!)) / Math.Pow(10, (double)purchasingAndReceivingPoliciesDto.CostDecPolicy);

            if (purchasingAndReceivingPoliciesDto.PackCostPolicy == true)
            {
              double packing = (double)productAndVendorPartDataOfKitItem.Packing;
              if (packing == null || packing == 0)
              {
                packing = 1;
              }

              purchaseDetail.PoCost = vendorUnitCost * packing;
            }
            else
            {
              purchaseDetail.PoCost = vendorUnitCost;
            }

            purchaseDetail.PoCost = (double)Math.Floor((decimal)(
              purchaseDetail.PoCost * Math.Pow(10, purchasingAndReceivingPoliciesDto.CostDecPolicy)))
              / Math.Pow(10, purchasingAndReceivingPoliciesDto.CostDecPolicy);

            purchaseDetail.StockCode = productAndVendorPartDataOfKitItem.StockCode;
            purchaseDetail.Department = productAndVendorPartDataOfKitItem.Department;
            purchaseDetail.SubDepartment = productAndVendorPartDataOfKitItem.SubDepartment;

            if (purchaseDetail.PartNumber != null && getChargeByPartNoDictionary.TryGetValue(purchaseDetail.PartNumber, out double tmpCharge))
            {
              purchaseDetail.Charges = tmpCharge;
            }
            else
            {
              purchaseDetail.Charges = 0;
            }

            double cost = 0;
            if (poDiscount != 0)
            {
              cost = (double)((double)(purchaseDetail.PoCost * (1 - poDiscount)
                + (purchaseDetail.Charges * purchaseDetail.UnitQuantity * poDiscount)) / purchaseDetail.UnitQuantity);
              cost = cost.Round(2);
            }
            else
            {
              if (isTaxIncluded)
              {
                cost = (double)((purchaseDetail.PoCost * (1 - poDiscount)
                  + (purchaseDetail.Charges * purchaseDetail.UnitQuantity * poDiscount)) / purchaseDetail.UnitQuantity);
                cost = cost.Round(2);
              }
              else
              {
                cost = ((double)purchaseDetail.PoCost).Round(2);
              }
            }

            purchaseDetail.Cost = cost;
            purchaseDetail.CalculateDiscount = (CalculateDiscount(poDiscount, cost, purchaseDetail.Charges)).Round(4);

            purchaseDetail.ExtCost = purchaseDetail.ReceivedQuantity * ((double)purchaseDetail.PoCost).Round(4);

            double? depositQuantity = purchaseDetail.UnitQuantity * purchaseDetail.OrderedQuantity;
            if (poDiscount != 0)
            {
              double? tmpExtCost = (double?)(purchaseDetail.ExtCost
                - (purchaseDetail.CalculateDiscount * purchaseDetail.OrderedQuantity)
                - (purchaseDetail.Charges * depositQuantity));


              IList<double> taxRates = new List<double>();
              if (kitProduct != null && getTaxRatesByStockCodesDictionary.TryGetValue(kitProduct, out IList<double> tmpTaxRates))
              {
                taxRates = tmpTaxRates;
              }

              purchaseDetail.GST = CalculateGST(
                taxRates,
                purchasingAndReceivingPoliciesDto.ExptTaxPolicy,
                tmpExtCost);
            }
            else
            {
              if (isTaxIncluded)
              {
                double? tmpExtCost = purchaseDetail.ExtCost
                  - (purchaseDetail.CalculateDiscount * purchaseDetail.OrderedQuantity)
                  - (purchaseDetail.Charges * depositQuantity);

                IList<double> taxRatesGST = new List<double>();
                if (productAndVendorPartDataOfKitItem.StockCode != null
                  && getTaxRatesByStockCodesForGSTDictionary.TryGetValue(
                    productAndVendorPartDataOfKitItem.StockCode,
                    out IList<double> tmpTaxRatesGST))
                {
                  taxRatesGST = tmpTaxRatesGST;
                }

                IList<double> taxRatesHST = new List<double>();
                if (productAndVendorPartDataOfKitItem.StockCode != null
                  && getTaxRatesByStockCodesForHSTDictionary.TryGetValue(
                    productAndVendorPartDataOfKitItem.StockCode,
                    out IList<double> tmpTaxRatesHST))
                {
                  taxRatesHST = tmpTaxRatesHST;
                }

                double itemsGSTRate = CalculateItemsGstRate(taxRatesGST);
                double itemsHSTRate = CalculateItemsHstRate(taxRatesHST, purchasingAndReceivingPoliciesDto.CostDecPolicy);

                purchaseDetail.GST = CalculateTax(itemsGSTRate, itemsHSTRate, tmpExtCost);
              }
              else
              {
                IList<double> taxRates = new List<double>();
                if (productAndVendorPartDataOfKitItem.StockCode != null
                  && getTaxRatesByStockCodesDictionary.TryGetValue(
                    productAndVendorPartDataOfKitItem.StockCode,
                    out IList<double> tmpTaxRates))
                {
                  taxRates = tmpTaxRates;
                }

                purchaseDetail.GST = CalculateGST(
                  taxRates,
                  purchasingAndReceivingPoliciesDto.ExptTaxPolicy,
                  purchaseDetail.ExtCost);
              }
            }
          }

          purchaseDetail.ApplyGST = (purchaseDetail.GST > 0) ? true : false;
          purchaseDetail.ProductVendor = vendorId;
        }
        else
        {
          purchaseDetail.PartNumber = vendorProduct.PartNo;
          purchaseDetail.Description = vendorProduct.Description;
          purchaseDetail.UnitOfMeasure = vendorProduct.OrderUnit ?? "";
          purchaseDetail.UnitQuantity = vendorProduct.Packing;
          purchaseDetail.OrderedQuantity = orderQuantity;
          purchaseDetail.ReceivedQuantity = orderQuantity;

          StockCostInfoParameterBODto stockCostInfoParameterBackOffic = new()
          {
            VendorId = vendorId,
            StockCode = vendorProduct.StockCode,
            OrderNo = 0
          };

          StockCostInfo? stockCostInfo = null;

          if (stockCostInfoParameterBackOffic.StockCode != null
            && stockCostInfoDictionary.TryGetValue(stockCostInfoParameterBackOffic.StockCode,
            out StockCostInfo? tmpStockCostInfo))
          {
            stockCostInfo = tmpStockCostInfo;
          }

          double vendorUnitCost = (stockCostInfo!.Cost == null) ? 0 : (double)stockCostInfo.Cost;

          vendorUnitCost = vendorUnitCost.Round(purchasingAndReceivingPoliciesDto.CostDecPolicy);

          if (purchasingAndReceivingPoliciesDto.PackCostPolicy)
          {
            double packing = (double)vendorProduct.Packing!;
            if (packing == 0)
            {
              packing = 1;
            }

            purchaseDetail.PoCost = vendorUnitCost * packing;
          }
          else
          {
            purchaseDetail.PoCost = vendorUnitCost;
          }

          purchaseDetail.PoCost = ((double)purchaseDetail.PoCost).Round(purchasingAndReceivingPoliciesDto.CostDecPolicy);

          purchaseDetail.StockCode = vendorProduct.StockCode;
          purchaseDetail.Department = vendorProduct.Department;
          purchaseDetail.SubDepartment = vendorProduct.SubDepartment;
          if (purchaseDetail.PartNumber != null
            && getChargeByPartNoDictionary.TryGetValue(
              purchaseDetail.PartNumber, out double tmpCharge))
          {
            purchaseDetail.Charges = tmpCharge;
          }
          else
          {
            purchaseDetail.Charges = 0;
          }

          double cost = 0;
          if (poDiscount != 0)
          {
            cost = (double)((double)(purchaseDetail.PoCost * (1 - poDiscount) + (purchaseDetail.Charges * purchaseDetail.UnitQuantity * poDiscount)) / purchaseDetail.UnitQuantity);
            cost = cost.Round(2);
          }
          else
          {
            if (isTaxIncluded)
            {
              cost = (double)((purchaseDetail.PoCost * (1 - poDiscount) + (purchaseDetail.Charges * purchaseDetail.UnitQuantity * poDiscount)) / purchaseDetail.UnitQuantity);
              cost = cost.Round(2);
            }
            else
            {
              cost = ((double)purchaseDetail.PoCost).Round(2);
            }
          }

          purchaseDetail.Cost = cost;
          purchaseDetail.CalculateDiscount = (CalculateDiscount(poDiscount, cost, purchaseDetail.Charges)).Round(4);

          purchaseDetail.ExtCost = purchaseDetail.ReceivedQuantity * ((double)purchaseDetail.PoCost).Round(4);

          double? depositQuantity = purchaseDetail.UnitQuantity * purchaseDetail.OrderedQuantity;
          if (poDiscount != 0)
          {
            double? tmpExtCost = (purchaseDetail.ExtCost
              - (purchaseDetail.CalculateDiscount * purchaseDetail.OrderedQuantity)
              - (purchaseDetail.Charges * depositQuantity));

            IList<double> taxRates = new List<double>();
            if (vendorProduct.StockCode != null
              && getTaxRatesByStockCodesDictionary.TryGetValue(vendorProduct.StockCode, out IList<double> tmpTaxRates))
            {
              taxRates = tmpTaxRates;
            }

            purchaseDetail.GST = CalculateGST(
              taxRates,
              purchasingAndReceivingPoliciesDto.ExptTaxPolicy,
              tmpExtCost);
          }
          else
          {
            if (isTaxIncluded)
            {
              double? tmpExtCost = purchaseDetail.ExtCost
                - (purchaseDetail.ReceivedQuantity * purchaseDetail.Charges);

              IList<double> taxRatesGST = new List<double>();
              if (vendorProduct.StockCode != null
                && getTaxRatesByStockCodesForGSTDictionary.TryGetValue(vendorProduct.StockCode, out IList<double> tmpTaxRatesGST))
              {
                taxRatesGST = tmpTaxRatesGST;
              }

              IList<double> taxRatesHST = new List<double>();
              if (vendorProduct.StockCode != null
                && getTaxRatesByStockCodesForHSTDictionary.TryGetValue(vendorProduct.StockCode, out IList<double> tmpTaxRatesHST))
              {
                taxRatesHST = tmpTaxRatesHST;
              }

              double itemsGSTRate = CalculateItemsGstRate(taxRatesGST);
              double itemsHSTRate = CalculateItemsHstRate(taxRatesHST, purchasingAndReceivingPoliciesDto.CostDecPolicy);

              purchaseDetail.GST = CalculateTax(itemsGSTRate, itemsHSTRate, tmpExtCost);
            }
            else
            {
              IList<double> taxRates = new List<double>();
              if (vendorProduct.StockCode != null
                && getTaxRatesByStockCodesDictionary.TryGetValue(vendorProduct.StockCode, out IList<double> tmpTaxRates))
              {
                taxRates = tmpTaxRates;
              }

              purchaseDetail.GST = CalculateGST(
                taxRates,
                purchasingAndReceivingPoliciesDto.ExptTaxPolicy,
                purchaseDetail.ExtCost);
            }
          }
        }

        purchaseDetail.ApplyGST = (purchaseDetail.GST > 0);
        purchaseDetail.ProductVendor = vendorId;
        purchaseDetail.Posted = false;
        purchaseDetail.CostChanged = false;
        purchaseDetail.PackQuantityChanged = false;

        if (!(purchaseDetail.CalculateDiscount >= minSmallMoneyValue && purchaseDetail.CalculateDiscount <= maxSmallMoneyValue))
        {
          purchaseDetail.CalculateDiscount = 0;
        }

        purchaseDetails.Add(purchaseDetail);
      }
    }

    // Generate Purchase Details list to add in PurchaseDetails Table End

    await _unitOfWork.PurchaseDetails.AddPurchaseDetailBatchAsync(purchaseDetails);

    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<POFileDto?> GetPOFileData(long purchaseNo)
  {
    string vendorSortCondition = await GetVendorSortCondition(purchaseNo);
    PurchaseHeader? purchaseHeader = await _unitOfWork.PurchaseHeaders.GetPurchaseHeadersByPurchaseNoAsync(purchaseNo);

    IEnumerable<PurchaseDetail> purchaseDetails;
    if (vendorSortCondition != "")
    {
      purchaseDetails = await _unitOfWork.PurchaseDetails.GetPurchaseDetailsByPurchaseNoAndOrderByVendorsAsync(purchaseNo, vendorSortCondition);
    }
    else
    {
      purchaseDetails = await _unitOfWork.PurchaseDetails.GetPurchaseDetailsByPurchaseNoAsync(purchaseNo);
    }

    POFileDto poFile = new POFileDto
    {
      PurchaseHeader = purchaseHeader,
      PurchaseDetails = purchaseDetails
    };

    _unitOfWork.Commit();
    return poFile;
  }

  public async Task<IEnumerable<PurchaseDetail>> GetPurchaseDetailsByPurchaseNumber(long purchaseNo)
  {
    string vendorSortCondition = await GetVendorSortCondition(purchaseNo);

    IEnumerable<PurchaseDetail> purchaseDetails =
      await _unitOfWork.PurchaseDetails.GetPurchaseDetailsByPurchaseNumberAndVendorSortAsync(
        purchaseNo,
        vendorSortCondition);


    _unitOfWork.Commit();
    return purchaseDetails;
  }

  public async Task<bool> UpdatePurchaseDetail(
    PurchaseDetail purchaseDetail,
    PurchasingAndReceivingPoliciesDto purchasingAndReceivingPoliciesDto,
    string vendorId)
  {
    double poDiscount = await _unitOfWork.Vendors.GetPoDiscountAsync(vendorId);
    bool isTaxIncluded = await _unitOfWork.Vendors.GetIsTaxIncludeInPOAsync(vendorId);

    IList<string> stockCodes = new List<string> { purchaseDetail.StockCode ?? "" };

    IDictionary<string, IList<double>> getTaxRatesByStockCodesForGSTDictionary =
      await _stockTaxService.GetTaxRatesByStockCodes(purchasingAndReceivingPoliciesDto.ExptTaxPolicy, stockCodes, false);

    IDictionary<string, IList<double>> getTaxRatesByStockCodesForHSTDictionary =
      await _stockTaxService.GetTaxRatesByStockCodes(purchasingAndReceivingPoliciesDto.ExptTaxPolicy, stockCodes, true);

    IDictionary<string, IList<double>> getTaxRatesByStockCodesDictionary =
      await _stockTaxService.GetTaxRatesByStockCodes(purchasingAndReceivingPoliciesDto.ExptTaxPolicy, stockCodes);

    IList<(string stockCode, double charge)> stockCodeAndChargePairs =
      (IList<(string stockCode, double charge)>)await _unitOfWork.Associations.GetStockCodeAndAssociationsChargeByStockCodesAsync(stockCodes);


    IDictionary<string, StockCostInfo> stockCostInfoDictionary = new Dictionary<string, StockCostInfo>();

    StockCostInfoBatchParameterBackOfficeDto stockCostInfoBatchParameterBackOfficDto = new()
    {
      OrderNo = 0,
      VendorId = vendorId,
      StockCodes = stockCodes
    };

    IEnumerable<StockCostInfo?> stockCostInfos =
      await _unitOfWork.StockCosts.GetStockCostInfoBatchForBackOfficeAsync(stockCostInfoBatchParameterBackOfficDto);

    foreach (StockCostInfo stockCostInfo in stockCostInfos)
    {
      stockCostInfoDictionary.Add(stockCostInfo.StockCode, stockCostInfo);
    }

    IDictionary<string, double> getChargeByPartNoDictionary = new Dictionary<string, double>();

    if (stockCodeAndChargePairs.Count() > 0)
    {
      getChargeByPartNoDictionary.Add(purchaseDetail.PartNumber, stockCodeAndChargePairs[0].charge);
    }

    PurchaseDetailUpdateParameterRecord purchaseDetailUpdateParameterRecord = new PurchaseDetailUpdateParameterRecord()
    {
      PoDiscount = poDiscount,
      IsTaxIncluded = isTaxIncluded,
      PurchaseDetail = purchaseDetail,
      GetTaxRatesByStockCodesDictionary = getTaxRatesByStockCodesDictionary,
      GetTaxRatesByStockCodesForGSTDictionary = getTaxRatesByStockCodesForGSTDictionary,
      GetTaxRatesByStockCodesForHSTDictionary = getTaxRatesByStockCodesForHSTDictionary,
      GetChargeByPartNoDictionary = getChargeByPartNoDictionary,
      StockCostInfoDictionary = stockCostInfoDictionary,
      PurchasingAndReceivingPoliciesDto = purchasingAndReceivingPoliciesDto
    };

    PurchaseDetail purchaseDetailCurrentData = await _unitOfWork.PurchaseDetails.GetPurchaseDetailByPurchaseNumberAsync(
        purchaseDetail.PurchaseNo,
        purchaseDetail.LineNumber,
        purchaseDetail.PartNumber);


    if (purchaseDetail.OrderedQuantity != null)
    {
      purchaseDetailCurrentData.OrderedQuantity = purchaseDetail.OrderedQuantity;
      purchaseDetailUpdateParameterRecord.PurchaseDetail = purchaseDetailCurrentData;
      purchaseDetailUpdateParameterRecord.FieldDataUpdated = (int)PurchaseDetailEnum.OrderQuantity;
      purchaseDetailCurrentData = UpdateDataInPurchaseDetailModel(purchaseDetailUpdateParameterRecord);
    }
    else if (purchaseDetail.UnitQuantity != null)
    {
      purchaseDetailCurrentData.UnitQuantity = purchaseDetail.UnitQuantity;
      purchaseDetailUpdateParameterRecord.PurchaseDetail = purchaseDetailCurrentData;
      purchaseDetailUpdateParameterRecord.FieldDataUpdated = (int)PurchaseDetailEnum.PackingQuantity;
      purchaseDetailCurrentData = UpdateDataInPurchaseDetailModel(purchaseDetailUpdateParameterRecord);
    }
    else if (purchaseDetail.ReceivedQuantity != null)
    {
      purchaseDetailCurrentData.ReceivedQuantity = purchaseDetail.ReceivedQuantity;
      purchaseDetailUpdateParameterRecord.PurchaseDetail = purchaseDetailCurrentData;
      purchaseDetailUpdateParameterRecord.FieldDataUpdated = (int)PurchaseDetailEnum.ReceivedQuantity;
      purchaseDetailCurrentData = UpdateDataInPurchaseDetailModel(purchaseDetailUpdateParameterRecord);
    }
    else if (purchaseDetail.PoCost != null)
    {
      purchaseDetailCurrentData.PoCost = purchaseDetail.PoCost;
      purchaseDetailUpdateParameterRecord.PurchaseDetail = purchaseDetailCurrentData;
      purchaseDetailUpdateParameterRecord.FieldDataUpdated = (int)PurchaseDetailEnum.Cost;
      purchaseDetailCurrentData = UpdateDataInPurchaseDetailModel(purchaseDetailUpdateParameterRecord);
    }
    else if (purchaseDetail.ApplyGST != null)
    {
      purchaseDetailCurrentData.ApplyGST = purchaseDetail.ApplyGST;
      purchaseDetailUpdateParameterRecord.PurchaseDetail = purchaseDetailCurrentData;
      purchaseDetailUpdateParameterRecord.FieldDataUpdated = (int)PurchaseDetailEnum.ApplyGST;
      purchaseDetailCurrentData = UpdateDataInPurchaseDetailModel(purchaseDetailUpdateParameterRecord);
    }

    bool isUpdated = await _unitOfWork.PurchaseDetails.UpdatePurchaseDetailAsync(purchaseDetailCurrentData);

    _unitOfWork.Commit();
    return isUpdated;
  }

  public async Task<bool> DeletePurchaseDetail(
    string purchaseNo,
    string partNumber,
    string lineNumber)
  {
    bool isDeleted = await _unitOfWork.PurchaseDetails.DeletePurchaseDetailAsync(
        purchaseNo,
        partNumber,
        lineNumber);


    _unitOfWork.Commit();
    return isDeleted;
  }

  public async Task<VendorUnitCostPercentageRecord> GetVendorUnitCostPercentage(
    PurchaseDetail purchaseDetail,
    PurchasingAndReceivingPoliciesDto purchasingAndReceivingPoliciesDto,
    string vendorId)
  {

    IList<string> stockCodes = new List<string> { purchaseDetail.StockCode ?? "" };
    IDictionary<string, StockCostInfo> stockCostInfoDictionary = new Dictionary<string, StockCostInfo>();

    StockCostInfoBatchParameterBackOfficeDto stockCostInfoBatchParameterBackOfficDto = new()
    {
      OrderNo = 0,
      VendorId = vendorId,
      StockCodes = stockCodes
    };

    IEnumerable<StockCostInfo?> stockCostInfos =
      await _unitOfWork.StockCosts.GetStockCostInfoBatchForBackOfficeAsync(stockCostInfoBatchParameterBackOfficDto);

    foreach (StockCostInfo stockCostInfo in stockCostInfos)
    {
      stockCostInfoDictionary.Add(stockCostInfo.StockCode, stockCostInfo);
    }

    double vendorUnitCost = GetVendorUnitCost(purchaseDetail.StockCode, stockCostInfoDictionary, purchasingAndReceivingPoliciesDto);
    double vendorUnitCostPercentage = 0;
    double divisor = ((double)(vendorUnitCost * purchaseDetail.UnitQuantity));

    if (divisor != 0)
    {
      vendorUnitCostPercentage = Math.Abs((double)((vendorUnitCost * purchaseDetail.UnitQuantity - purchaseDetail.PoCost) / divisor) * 100);
    }

    VendorUnitCostPercentageRecord vendorUnitCostPercentageRecord = new()
    {
      VendorUnitCostPercentage = vendorUnitCostPercentage,
      VendorUnitCost = vendorUnitCost
    };

    _unitOfWork.Commit();
    return vendorUnitCostPercentageRecord;
  }

  public async Task<bool> UpdatePurchaseDetailGSTByPurchaseNO(IEnumerable<PurchaseDetail> purchaseDetails)
  {
    bool isUpdated = await _unitOfWork.PurchaseDetails.UpdatePurchaseDetailGSTByPurchaseNOAsync(purchaseDetails);

    _unitOfWork.Commit();
    return isUpdated;
  }

  public async Task<bool> AddTaxToPurchaseDetail(AddTaxToPurchaseDetailParameterDto addTaxToPurchaseDetail)
  {
    IEnumerable<PurchaseDetail>? purchaseDetails = addTaxToPurchaseDetail.PurchaseDetails;
    string? exptTaxPolicy = addTaxToPurchaseDetail.ExptTaxPolicy;
    int? costDec = addTaxToPurchaseDetail.CostDec;
    DateTime? purchaseDate = addTaxToPurchaseDetail.PurchaseDate;

    IList<string> stockCodes = purchaseDetails.Select(pd => pd.StockCode!).Distinct().ToList();

    IDictionary<string, IList<double>> getTaxRatesByStockCodesForGSTDictionary =
      await _stockTaxService.GetTaxRatesByStockCodes(exptTaxPolicy, stockCodes, false, purchaseDate);

    foreach (PurchaseDetail purchaseDetail in purchaseDetails)
    {
      IList<double> taxRates = new List<double>();
      if (purchaseDetail.StockCode != null
        && getTaxRatesByStockCodesForGSTDictionary.TryGetValue(purchaseDetail.StockCode, out IList<double> tmpTaxRates))
      {
        taxRates = tmpTaxRates;
      }

      purchaseDetail.GST = CalculateGST(taxRates, exptTaxPolicy, purchaseDetail.ExtCost, costDec);
      purchaseDetail.ApplyGST = (purchaseDetail.GST != 0);
    }

    bool isUpdated = await _unitOfWork.PurchaseDetails.UpdatePurchaseDetailGSTByPurchaseNOAsync(purchaseDetails);
    _unitOfWork.Commit();
    return isUpdated;
  }
}
