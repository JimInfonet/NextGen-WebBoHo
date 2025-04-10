namespace InfonetCSCR.Core.Dtos;

public class AssocTaxDto
{
  public string StockCode { get; set; } = default!;
  public string TaxName { get; set; } = default!;
  public decimal? Rate { get; set; }
  public decimal? Price { get; set; }
}
