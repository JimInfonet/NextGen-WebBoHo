namespace InfonetCSCR.Core.Dtos;

public class PendingHotButtonsTransferDto
{
  public List<string> StoreIds { get; set; }
  public List<HotButtonsWithProductName> HotButtons { get; set; }
  public Dictionary<int, string> TransTypes { get; set; }
}
