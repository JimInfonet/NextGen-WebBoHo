namespace InfonetCSCR.Web.Models;

public class KitsViewModel
{
  [MaxLength(16)]
  public string StockCode { get; set; } = default!;
  [MaxLength(35)]
  public string? Description { get; set; }
  public string? Dept { get; set; }
  public char? PriceType { get; set; }
}
