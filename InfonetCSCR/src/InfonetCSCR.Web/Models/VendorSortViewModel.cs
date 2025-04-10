namespace InfonetCSCR.Web.Models;

public class VendorSortViewModel
{
  public string VendorId { get; set; } = default!;
  public int? OrderNo { get; set; }
  public string SortItem { get; set; } = default!;
  public bool? Ascending { get; set; }
}
