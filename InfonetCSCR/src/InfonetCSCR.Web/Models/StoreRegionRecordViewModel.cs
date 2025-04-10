namespace InfonetCSCR.Web.Models;

public class StoreRegionRecordViewModel
{
  public IEnumerable<StoreViewModel> Stores { get; set; }
  public IEnumerable<RegionViewModel> Regions { get; set; }
}
