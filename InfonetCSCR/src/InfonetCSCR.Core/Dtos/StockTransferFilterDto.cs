namespace InfonetCSCR.Core.Dtos;

public class StockTransferFilterDto
{
  public IEnumerable<StockTransferDto>? stockTransferDtos { get; set; }
  public int TotalItems { get; set; }
}
