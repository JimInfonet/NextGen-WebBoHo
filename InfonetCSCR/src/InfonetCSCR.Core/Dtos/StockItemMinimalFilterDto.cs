namespace InfonetCSCR.Core.Dtos;

public class StockItemMinimalFilterDto
{
  public int TotalItems { get; set; }
  public IEnumerable<StockItemMinimal> StockItems { get; set; }
}
