namespace InfonetCSCR.Web.Models;

public class SpecialPriceViewModel : StockPriceModel
{
  public decimal? PriceFromQuantity { get; set; }
  public decimal? PriceToQuantity { get; set; }
  public decimal? Price { get; set; }
  public string? PriceType { get; set; }
  public string? PriceUnits { get; set; }
  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set; }
  public bool UseArInvoice { get; set; }
}
