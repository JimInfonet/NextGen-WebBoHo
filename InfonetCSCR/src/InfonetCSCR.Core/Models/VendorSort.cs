namespace InfonetCSCR.Core.Models;

public class VendorSort
{
  public string VendorId { get; set; } = default!;
  public int? OrderNo { get; set; }
  public string SortItem { get; set; } = default!;
  public bool? Ascending { get; set; }
}
