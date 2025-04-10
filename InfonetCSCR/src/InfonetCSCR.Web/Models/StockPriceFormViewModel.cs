namespace InfonetCSCR.Web.Models;

public class StockPriceFormViewModel
{
  public string? StockItemCode { get; set; }
  public string? StockItemName { get; set; }
  public int? Packing { get; set; }
  public IEnumerable<StockPriceVendorsViewModel>? Vendors { get; set; }
  public IEnumerable<StockCostsViewModel>? StockCosts { get; set; }
  public IEnumerable<StockPriceViewModel>? StockPrices { get; set; }
  public IEnumerable<StockRebatesViewModel>? StockPriceRebates { get; set; }
  public StockPricePromotionViewModel? Promotion { get; set; }
  public IEnumerable<SpecialPriceViewModel>? SpecialPrice { get; set; }
  public IEnumerable<SpecialPriceViewModel>? SpecialPriceFromPriceL { get; set; }
}
