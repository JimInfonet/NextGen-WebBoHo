namespace InfonetCSCR.Core.Models;

public class Tax : StockItemDetailsCommonDto
{
  public string? TaxName { get; set; }
  public string? TaxCode { get; set; }
  public decimal Rate { get; set; }
  public bool Included { get; set; }
}
