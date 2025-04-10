namespace InfonetCSCR.Core.Dtos;

public class PrepareCostChangeTransferDto
{
  public int BatchId { get; set; }
  public List<CostChangeTransfer> CostChangeTransfers { get; set; }
  public List<string> StoreIds { get; set; } 
  public List<StockItemMinimal> Products { get; set; }
  public Dictionary<string, int> PackCountDict { get; set; }
  public string? SelectedOptionId { get; set; }
}
