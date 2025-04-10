namespace InfonetCSCR.Web.Models;

public class VendorAddRecordViewModel
{
  public VendorViewModel? Vendor { get; set; }
  public List<VendorSortSetupViewModel>? SortItems { get; set; }
  public string AddedSortItems { get; set; }
  public string RemovedSortItems { get; set; }
}
