namespace InfonetCSCR.Services;

public class VendorService : IVendorService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IApplicationConfig _applicationConfig;

  public VendorService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
    _applicationConfig = applicationConfig;
  }

  public async Task<IEnumerable<Vendor>> GetAllVendorsAsync()
  {
    IEnumerable<Vendor> vendors = await _unitOfWork.Vendors.GetAllVendorsAsync();
    _unitOfWork.Commit();

    vendors.ToList().ForEach(v => {
      v.IsTEVendor ??= false;
      v.Active ??= false;
      v.PoCostTaxInclude ??= false;
    });
    return vendors;
  }

  public async Task<IEnumerable<Vendor>> GetVendorByCodeAsync(string code)
  {
    Vendor vendor = await _unitOfWork.Vendors.GetVendorByCodeAsync(code);
    _unitOfWork.Commit();
    return new List<Vendor> { vendor };
  }

  public async Task<IEnumerable<Vendor>> GetVendorsByVendorIds(IEnumerable<string> VendorIds)
  {
    IEnumerable<Vendor> vendors = await _unitOfWork.Vendors.GetVendorsByVendorIdsAsync(VendorIds);
    _unitOfWork.Commit();
    return vendors;
  }

  public async Task<Vendor?> GetVendorByPurchaseNo(long purchaseNo)
  {
    Vendor? vendor = await _unitOfWork.Vendors.GetVendorByPurchaseNoAsync(purchaseNo);
    _unitOfWork.Commit();
    return vendor;
  }

  public async Task<IEnumerable<Vendor>> GetActiveVendorsOrderByColumnNumber(int columnNumber)
  {
    IEnumerable<Vendor> vendors = await _unitOfWork.Vendors.GetActiveVendorsOrderByColumnNumberAsync(columnNumber);
    _unitOfWork.Commit();
    return vendors;
  }

  public async Task<Vendor> GetVendorByVendorCode(string code)
  {
    Vendor vendor = await _unitOfWork.Vendors.GetVendorByVendorCodeAsync(code, _applicationConfig.IsBackOffice);
    _unitOfWork.Commit();
    
    if (vendor != null)
    {
      vendor.IsTEVendor ??= false;
      vendor.Active ??= false;
      vendor.PoCostTaxInclude ??= false;
    }
    return vendor;
  }

  public async Task<IEnumerable<VendorSortSetup>> GetVendorsSortSetup()
  {
    var vendorSortSetup = await _unitOfWork.VendorSorts.GetAllVendorSortSetupsBySortItemsAsync();
    _unitOfWork.Commit();
    return vendorSortSetup;
  }

  public async Task<IEnumerable<VendorSort>> GetVendorsSort(string vendorId)
  {
    var vendorSort = await _unitOfWork.VendorSorts.GetVendorSortsAsync(vendorId);
    _unitOfWork.Commit();
    return vendorSort;
  }

  public async Task<bool> SaveVendor(VendorAddRecord vendorAdd, bool isAdd)
  {
    var addedSortItemList = new List<VendorSort>();
    if (!string.IsNullOrEmpty(vendorAdd.AddedSortItems))
    {
      addedSortItemList = JsonSerializer.Deserialize<List<VendorSort>>(vendorAdd.AddedSortItems);
    }

    var isBackOffice = _applicationConfig.IsBackOffice;

    vendorAdd.Vendor.PoDiscount ??= 0;
    vendorAdd.Vendor.OrderFreq ??= 1;

    vendorAdd.Vendor.Name ??= "";
    vendorAdd.Vendor.Address ??= "";
    vendorAdd.Vendor.City ??= "";
    vendorAdd.Vendor.Postal ??= "";
    vendorAdd.Vendor.Province ??= "";
    vendorAdd.Vendor.Country ??= "";
    vendorAdd.Vendor.Phone ??= "";
    vendorAdd.Vendor.Fax ??= "";
    vendorAdd.Vendor.Email ??= "";
    vendorAdd.Vendor.ContactName1 ??= "";
    vendorAdd.Vendor.ContactName2 ??= "";
    vendorAdd.Vendor.TaxNumber ??= "";
    vendorAdd.Vendor.CustAccount ??= "";
    vendorAdd.Vendor.Active ??= false;
    vendorAdd.Vendor.IsTEVendor ??= false;
    vendorAdd.Vendor.PoCostTaxInclude ??= false;

    addedSortItemList = addedSortItemList.FindAll(item => item.OrderNo == -1).ToList();

    var removedSortItemList = new List<VendorSort>();
    if (!string.IsNullOrEmpty(vendorAdd.RemovedSortItems))
    {
      removedSortItemList = JsonSerializer.Deserialize<List<VendorSort>>(vendorAdd.RemovedSortItems);
    }

    bool result;
    if (isAdd)
    {
      result = await _unitOfWork.Vendors.AddVendorAsync(vendorAdd.Vendor, isBackOffice);
    }
    else
    {
      result = await _unitOfWork.Vendors.UpdateVendorAsync(vendorAdd.Vendor, isBackOffice);
    }

    if(result)
    {
      var newOrder = await _unitOfWork.VendorSorts.GetNextSortOrderAsync(vendorAdd.Vendor.Code);
      addedSortItemList.ForEach(item => {
        item.OrderNo = newOrder++;
        item.VendorId = vendorAdd.Vendor.Code;
      });
      await _unitOfWork.VendorSorts.DeleteVendorSortAsync(removedSortItemList.Select(obj => obj.SortItem), vendorAdd.Vendor.Code);
      await _unitOfWork.VendorSorts.AddVendorSortAsync(addedSortItemList);
    }
    _unitOfWork.Commit();
    return result;
  }
  
  public async Task<bool> DeleteVendor(string vendorId)
  {
    var count = await _unitOfWork.Vendors.GetVendorCountStockMstAsync(vendorId);

    if (count == 0)
    {
      count = await _unitOfWork.Vendors.GetVendorCountStockCostsAsync(vendorId);

      if (count == 0)
      {
        count = await _unitOfWork.Vendors.GetVendorCountStockPricesAsync(vendorId);
      }

      if (count > 0)
      {
        await _unitOfWork.Vendors.VendorDeleteFromStockCostsAsync(vendorId);
        await _unitOfWork.Vendors.VendorDeleteFromStockPricesAsync(vendorId);
        await _unitOfWork.Vendors.SetTEVendorNullProductTaxExemptAsync(vendorId);
      }
      await _unitOfWork.Vendors.VendorDeleteFromVendorAsync(vendorId);
      await _unitOfWork.Vendors.VendorDeleteFromVendorSortAsync(vendorId);
      _unitOfWork.Commit();

      return true;
    }
    _unitOfWork.Commit();

    return false;
  }

  public async Task<VendorDeletionCaseRecord> CheckDeleteValidation(string vendorId)
  {
    var count = await _unitOfWork.Vendors.GetVendorCountStockMstAsync(vendorId);
    var vendorDeletionCase = new VendorDeletionCaseRecord();

    if (count == 0)
    {
      count = await _unitOfWork.Vendors.GetVendorCountStockCostsAsync(vendorId);

      if (count == 0)
      {
        count = await _unitOfWork.Vendors.GetVendorCountStockPricesAsync(vendorId);
      }
      vendorDeletionCase.IsConfirmationAlert = true;
    }
    else
    {
      vendorDeletionCase.IsConfirmationAlert = false;
    }
    vendorDeletionCase.Count = count;
    _unitOfWork.Commit();

    return vendorDeletionCase;
  }

  public async Task<IEnumerable<Vendor>> GetAllVendorsNameAsync()
  {
    var result = await _unitOfWork.Vendors.GetAllVendorNameAsync();
    result.ToList().ForEach(v => {
      v.IsTEVendor ??= false;
      v.Active ??= false;
      v.PoCostTaxInclude ??= false;
    });
    _unitOfWork.Commit();
    return result;
  }

  public async Task<IEnumerable<VendorLink>> GetVendorLinksAsync(string vendorId)
  {
    var result = await _unitOfWork.Vendors.GetVendorLinksAsync(vendorId);
    _unitOfWork.Commit();
    return result;
  }

  public async Task<StoreRegionRecord> GetAllStoresAndRegions()
  {
    var stores = await _unitOfWork.Store.GetAllStoresAsync();
    var regions = await _unitOfWork.Region.GetRegion();
    _unitOfWork.Commit();

    return new StoreRegionRecord()
    {
      Stores = stores,
      Regions = regions
    };
  }

  public async Task<bool> DeleteVendorLink(VendorLink vendorLink)
  {
    bool result;
    if (!string.IsNullOrEmpty(vendorLink.StoreId))
    {
      result = await _unitOfWork.Vendors.DeleteVendorStoreAsync(vendorLink.VendorId, vendorLink.StoreId);
    }
    else
    {
      result = await _unitOfWork.Vendors.DeleteVendorRegionAsync(vendorLink.VendorId, vendorLink.RegionId);
    }
    _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> AddVendorLink(VendorLink vendorLink)
  {
    bool result = await _unitOfWork.Vendors.AddVendorLinkAsync(vendorLink);
    _unitOfWork.Commit();
    return result;
  }

  public async Task<IEnumerable<Vendor>> GetVendorsOrderByColumnNumber(int columnNumber)
  {
    IEnumerable<Vendor> vendors = await _unitOfWork.Vendors.GetVendorsOrderByColumnNumberAsync(columnNumber);
    _unitOfWork.Commit();
    return vendors;
  }
}
