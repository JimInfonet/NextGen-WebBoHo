namespace InfonetCSCR.Core.Dtos;

public class StockTakesExportDto
{
  public IEnumerable<StockReceivingExportDto>? Stocks { get; set; }
  public IEnumerable<PLUMast>? PluMasts { get; set; }
}
