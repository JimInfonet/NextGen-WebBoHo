namespace InfonetCSCR.Web.Models;

public class StockTakesExportViewModel
{
  public IEnumerable<StockReceivingExportViewModel>? Stocks { get; set; }
  public IEnumerable<PLUMastViewModel>? PluMasts { get; set; }
}
