namespace InfonetCSCR.Core.Records;

public class VendorAddRecord
{
  public Vendor? Vendor { get; set; }
  public List<VendorSortSetup>? SortItems { get; set; }
  public string AddedSortItems { get; set; }
  public string RemovedSortItems { get; set; }
}
