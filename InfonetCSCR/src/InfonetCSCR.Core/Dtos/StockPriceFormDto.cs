namespace InfonetCSCR.Core.Dtos;

public class StockPriceFormDto
{
  public string? StockItemCode { get; set; }
  public string? StockItemName { get; set; }
  public int? Packing { get; set; }
  public IEnumerable<StockPriceVendors>? Vendors { get; set; }
  public IEnumerable<StockCosts>? StockCosts { get; set; }
  public IEnumerable<StockPrice>? StockPrices { get; set; }
  public IEnumerable<StockRebates>? StockPriceRebates { get; set; }
  public StockPricePromotion? Promotion { get; set; }
  public IEnumerable<StockSpecialPrice>? SpecialPrice { get; set; }
  public IEnumerable<StockSpecialPrice>? SpecialPriceFromPriceL { get; set; }
}
