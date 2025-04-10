namespace InfonetCSCR.Services;

public class VendorSortService : IVendorSortService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IApplicationConfig _applicationConfig;

  public VendorSortService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
    _applicationConfig = applicationConfig;
  }

  public async Task<Dictionary<string, string>> GetVendorSortConditionDictionaryByAccountNoList(List<string> accountNoList)
  {
    Dictionary<string, string> getVendorSortConditionsByAccountNo = new();
    using (var uow = new UnitOfWork(_applicationConfig, _applicationConfig.CSCMasterConnectionString))
    {
      IEnumerable<VendorSort> allVendorSorts = await _unitOfWork.VendorSorts.GetAllVendorSortsByVendorListAsync();

      Dictionary<string, List<string>> GetSortItemsByAccountNo = allVendorSorts.GroupBy(item => item.VendorId)
                                                                           .ToDictionary(group => group.Key, group => group.Select(item => item.SortItem).ToList());
      IEnumerable<VendorSortSetup> vendorSortSetups =
        await _unitOfWork.VendorSorts.GetAllVendorSortSetupsBySortItemsAsync();

      IDictionary<string, VendorSortSetup> getVendorSortSetupsBySortItemsDictionary = vendorSortSetups.ToDictionary(item => item.SortItem, item => item);

      foreach (string accountNo in accountNoList)
      {
        if (getVendorSortConditionsByAccountNo.ContainsKey(accountNo)) continue;
        List<string>? sortItems = GetSortItemsByAccountNo.TryGetValue(accountNo, out var tempSortItems) ? tempSortItems : null;
        if (sortItems == null) continue;

        StringBuilder vendorSortCondition = new();
        List<VendorSort> vendorSorts = allVendorSorts.Where(vs => sortItems.Contains(vs.SortItem) && vs.VendorId == accountNo).ToList();
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
            if (vendorSortCondition.Equals(""))
            {
              vendorSortCondition.Append($"{vendorSortSetup.SortItem.Replace(" ", "")} {(vendorSort.Ascending == true ? "ASC" : "DESC")}");
            }
            else
            {
              vendorSortCondition.Append($", {vendorSortSetup.SortItem.Replace(" ", "")} {(vendorSort.Ascending == true ? "ASC" : "DESC")}");
            }
          }
        }
        getVendorSortConditionsByAccountNo.Add(accountNo, vendorSortCondition.ToString());
      }

      uow.Commit();
      return getVendorSortConditionsByAccountNo;
    }
  }
}
