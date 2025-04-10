namespace InfonetCSCR.Web.Models;

public class PendingHotButtonsTransferDtoViewModel
{
  public List<string> StoreIds { get; set; }
  public List<HotButtonsWithProductNameViewModel> HotButtons { get; set; }
  public Dictionary<int, string> TransTypes { get; set; }
}
