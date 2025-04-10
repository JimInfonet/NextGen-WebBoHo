namespace InfonetCSCR.Core.Dtos;

public class StockInfoDto
{
  public string StockCode { get; set; } = default!;
  public string Branch { get; set; } = default!;
  public decimal? StandardCost { get; set; }
  public decimal? InStock { get; set; }
  public decimal? Avail { get; set; }
}
