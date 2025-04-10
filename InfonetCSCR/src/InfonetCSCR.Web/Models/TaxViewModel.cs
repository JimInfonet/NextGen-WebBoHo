namespace InfonetCSCR.Web.Models;

public class TaxViewModel : StockItemDetailsCommonViewModel
{
  public string? TaxName { get; set; }
  public string? TaxCode { get; set; }
  public decimal Rate { get; set; }
  public bool Included { get; set; }
}
