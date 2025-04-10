namespace InfonetCSCR.Core.Dtos;

public class StockPriceDto : StockPriceModel
{
  public int PricesInUse { get; set; }
  public string? PriceType { get; set; }
}
