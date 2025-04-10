namespace InfonetCSCR.Services;

public class PurchasingAndReceivingService : IPurchasingAndReceivingService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IPurchaseDetailService _purchaseDetailService;
  private readonly IPurchaseHeaderService _purchaseHeaderService;

  public PurchasingAndReceivingService(
    IApplicationConfig applicationConfig,
    IPurchaseDetailService purchaseDetailService,
    IPurchaseHeaderService purchaseHeaderService)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCTransConnectionString);
    _purchaseDetailService = purchaseDetailService;
    _purchaseHeaderService = purchaseHeaderService;
  }

  public async Task<bool> AddPurchasingAndReceiving(
    PurchasingAndReceivingDto purchasingAndReceivingDto,
    PurchasingAndReceivingPoliciesDto purchasingAndReceivingPolicies)
  {
    Vendor vendor = await _unitOfWork.Vendors.GetVendorByCodeAsync(purchasingAndReceivingDto.VendorId);
    PurchaseHeaderRecord purchaseHeader = new PurchaseHeaderRecord();
    purchaseHeader.VendorId = purchasingAndReceivingDto.VendorId;
    purchaseHeader.VendorName = purchasingAndReceivingDto.VendorName;
    purchaseHeader.UserCode = purchasingAndReceivingDto.UserCode;
    purchaseHeader.IsAutomated = purchasingAndReceivingDto.IsAutomated;

    long newPurchaseNo = await _unitOfWork.PurchaseHeaders.GetNextPurchaseNumberAsync();
    purchaseHeader.PurchaseNo = newPurchaseNo;

    if (purchaseHeader.IsAutomated)
    {
      await _purchaseDetailService.AddPurchaseDetails(newPurchaseNo, purchaseHeader.VendorId, purchasingAndReceivingPolicies, vendor);
    }

    Company? company = await _unitOfWork.CompanySetups.GetCompanyAsync();
    purchaseHeader.StoreId = company?.StoreCode;

    await _purchaseHeaderService.AddPurchaseHeader(purchaseHeader, vendor);

    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> DeletePurchasingAndReceivingByPurchaseNo(string purchaseNo)
  {
    await _unitOfWork.PurchaseHeaders.DeletePurchaseHeaderByPurchaseNoAsync(purchaseNo);
    await _unitOfWork.PurchaseDetails.DeletePurchaseDetailByPurchaseNoAsync(purchaseNo);

    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<IEnumerable<PurchasingAndReceivingReportDto>> GetPurchasingAndReceivingReportData(int purchaseNo)
  {
    PurchaseHeader? purchaseHeader = await _unitOfWork.PurchaseHeaders.GetPurchaseHeadersByPurchaseNoAsync(purchaseNo);
    // Prepare Vendor Sort Condition data Start

    IEnumerable<VendorSort> vendorSorts = await _unitOfWork.VendorSorts.GetVendorSortsAsync(purchaseHeader?.AccountNo);
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
    if (vendorSortCondition == "")
    {
      vendorSortCondition = " purchasedetail.linenum";
    }

    IEnumerable<PurchasingAndReceivingReportDto> data = await _unitOfWork.PurchaseHeaders.GetPurchasingAndReceivingReportDataAsync(purchaseNo, vendorSortCondition);
    return data;
  }
}
