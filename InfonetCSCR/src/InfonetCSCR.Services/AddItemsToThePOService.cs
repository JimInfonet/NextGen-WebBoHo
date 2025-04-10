namespace InfonetCSCR.Services;

public class AddItemsToThePOService : IAddItemsToThePOService
{
  #region Private Properties
  private readonly IUnitOfWork _unitOfWork;
  private readonly IApplicationConfig _config;
  private readonly IStockTaxService _stockTaxService;
  #endregion

  public AddItemsToThePOService(IApplicationConfig config, IStockTaxService stockTaxService)
  {
    _config = config;
    _stockTaxService = stockTaxService;
    _unitOfWork = new UnitOfWork(config, config.CSCMasterConnectionString);
  }

  public async Task<List<VendorPartDetailsDto>> GetVendorParts(string vendorId, AddItemsToThePOSelectedCriteria selectedCriteria)
  {
    var vendorParts = (await _unitOfWork.AddItemsToThePO.GetVendorPartsAsync(vendorId, selectedCriteria)).ToList();
    _unitOfWork.Commit();

    return vendorParts;
  }

  public async Task<List<string>> GetExistingPartNos(string purchaseNo, List<string> partNos)
  {
    var _unitOfWorkTrans = new UnitOfWork(_config, _config.CSCTransConnectionString);
    var existingPartNos = (await _unitOfWorkTrans.AddItemsToThePO.GetExistingPartNosAsync(purchaseNo, partNos)).ToList();

    _unitOfWorkTrans.Commit();
    return existingPartNos;
  }

  public async Task AddItemsToThePO(AddVendorPartsDto model)
  {
    var policies = model.Policies;
    var vendorParts = model.VendorParts;
    var vendorId = model.VendorId;
    var purchaseNo = model.PurchaseNo;

    List<string> stockCodes = vendorParts.Select(part => part.StockCode).Distinct().ToList();
    List<string> vendorIds = vendorParts.Select(part => part.VendorId).Distinct().ToList();
    List<(string stockCode, string vendorId)> stockCodesAndVendorIds =
      vendorParts.Select(part => (part.StockCode, part.VendorId)).ToList();

    // Get vendor products
    Dictionary<string, VendorProductDto> vendorProductsDict =
      (await _unitOfWork.StockItems.GetVendorProductsByStockCodes(stockCodes))
      .ToDictionary(product => product.StockCode!, product => product);

    // Get stock costs
    var getStockCostDto = new GetStockCostInfoByMultipleVendorBatchBackOfficeDto
    { OrderNo = 0, StockCodesAndVendorIds = stockCodesAndVendorIds, };

    Dictionary<(string stockCode, string vendorId), StockCostInfo> stockCostInfoDict =
      (await _unitOfWork.StockCosts.GetStockCostInfoByMultipleVendorBatchForBackOfficeAsync(getStockCostDto))
      .ToDictionary(info => (info!.StockCode!, info.VendorId!), info => info);

    // Get charges by stock code
    Dictionary<string, double> chargeDict =
      (await _unitOfWork.Associations.GetStockCodeAndAssociationsChargeByStockCodesAsync(stockCodes))
      .GroupBy(tuple => tuple.stockCode)
      .Select(group => group.First())
      .ToDictionary(tuple => tuple.stockCode, tuple => tuple.charge);

    // Get Tax rates to calculate GST
    IDictionary<string, IList<double>> taxRatesByStockCodesForGSTDict =
      await _stockTaxService.GetTaxRatesByStockCodes(policies.ExptTaxPolicy, stockCodes, false);

    // Get Tax rates to calculate HST
    IDictionary<string, IList<double>> taxRatesByStockCodesForHSTDict =
      await _stockTaxService.GetTaxRatesByStockCodes(policies.ExptTaxPolicy, stockCodes, true);

    // Other general props
    PoDiscountAndTaxIncludeDto poDiscountAndTaxInclude = await _unitOfWork.AddItemsToThePO.GetPoDiscountAndTaxIncludeAsync(vendorId);
    double poDiscount = poDiscountAndTaxInclude.PoDiscount;
    bool isTaxIncluded = poDiscountAndTaxInclude.PoCostTaxInclude;
    int lineNo = await _unitOfWork.PurchaseDetails.GetMaxLineNumAsync(purchaseNo);

    List<PurchaseDetail> newPurchaseDetails = new List<PurchaseDetail>();

    foreach (var vendorPart in vendorParts)
    {
      VendorProductDto vendorProduct = vendorProductsDict[vendorPart.StockCode];

      IList<double> taxRatesForGST = taxRatesByStockCodesForGSTDict.TryGetValue(vendorProduct.StockCode!, out var gstList) ? gstList : new List<double>();
      IList<double> taxRatesForHST = taxRatesByStockCodesForHSTDict.TryGetValue(vendorProduct.StockCode!, out var hstList) ? hstList : new List<double>();

      // tax rates
      double itemsGSTRate = PurchaseDetailHelper.CalculateItemsGstRate(taxRatesForGST);
      double itemsHSTRate = PurchaseDetailHelper.CalculateItemsHstRate(taxRatesForHST, policies.CostDecPolicy);
      double itemTaxRate = itemsGSTRate + itemsHSTRate;

      int unitQuantity = vendorProduct.Packing ?? 1;
      int orderedQuantity = 1;

      // vendor stock cost
      double vendorUnitCost = 0;
      if (stockCostInfoDict.TryGetValue((vendorProduct.StockCode!, vendorPart.VendorId), out StockCostInfo stockCostInfo))
      {
        vendorUnitCost = (double)(stockCostInfo.Cost ?? 0);
      }

      // Cost
      double cost = 0;
      if (policies.PackCostPolicy)
      {
        int packing = (vendorProduct.Packing == null || vendorProduct.Packing == 0) ? 1 : (int)vendorProduct.Packing;
        cost = (vendorUnitCost * packing).Round(2);
      }
      else cost = vendorUnitCost.Round(2);

      // Charge
      double charges = chargeDict.TryGetValue(vendorProduct.StockCode!, out double chargeValue) ? chargeValue : 0.0;

      // Po Cost
      double poCost = 0;
      if (poDiscount != 0)
      {
        poCost = ((cost - (charges * unitQuantity)) / (1 - poDiscount) + charges * unitQuantity).Round(2);
      }
      else
      {
        if (isTaxIncluded)
        {
          poCost = ((cost - (charges * unitQuantity)) * (1 + itemTaxRate) + charges * unitQuantity).Round(2);
        }
        else poCost = cost;
      }

      // others
      double calculatedDiscount = PurchaseDetailHelper.CalculateDiscount(poDiscount, cost, charges);
      double extCost = poCost.Round(policies.CostDecPolicy);
      double qty = unitQuantity * orderedQuantity;
      double ndExtCost = 0;
      double gst = 0;

      // gst
      if (poDiscount != 0)
      {
        ndExtCost = extCost - (calculatedDiscount * orderedQuantity) - (charges * qty);
        gst = PurchaseDetailHelper.CalculateGST(taxRatesForGST, policies.ExptTaxPolicy, ndExtCost);
      }
      else
      {
        if (isTaxIncluded)
        {
          ndExtCost = extCost - (charges * qty);
          gst = PurchaseDetailHelper.CalculateTax(itemTaxRate, ndExtCost);
        }
        else gst = PurchaseDetailHelper.CalculateGST(taxRatesForGST, policies.ExptTaxPolicy, extCost);
      }

      newPurchaseDetails.Add(new PurchaseDetail
      {
        PurchaseNo = int.Parse(purchaseNo),
        LineNumber = lineNo++,
        PartNumber = vendorPart.PartNo,
        Description = vendorProduct.Description,
        UnitOfMeasure = vendorProduct.UnitofMeasure,
        UnitQuantity = unitQuantity,
        OrderedQuantity = orderedQuantity,
        ReceivedQuantity = orderedQuantity,
        ProductVendor = vendorPart.VendorId,
        Cost = cost,
        StockCode = vendorProduct.StockCode,
        Charges = charges,
        CalculateDiscount = calculatedDiscount,
        PoCost = poCost,
        ExtCost = extCost,
        GST = gst,
        ApplyGST = gst != 0,
        Department = vendorProduct.Department,
        SubDepartment = vendorProduct.SubDepartment,
        SubDetail = vendorProduct.SubDetail,
        Posted = false,
        CostChanged = false,
      });
    }

    await _unitOfWork.PurchaseDetails.AddPurchaseDetailBatchAsync(newPurchaseDetails);
    await _unitOfWork.PurchaseDetails.UpdatePurchaseHeaderLineCount(purchaseNo);
    _unitOfWork.Commit();
    return;
  }
}
