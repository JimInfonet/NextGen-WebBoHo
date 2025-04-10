namespace InfonetCSCR.Core.Dtos;

public class KitsDto
{
  public string StockCode { get; set; } = default!;
  public string? Description { get; set; }
  public string? Dept { get; set; }
  public char? PriceType { get; set; }
}
