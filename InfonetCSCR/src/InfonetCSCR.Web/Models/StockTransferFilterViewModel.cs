namespace InfonetCSCR.Web.Models;

public class StockTransferFilterViewModel
{
  public IEnumerable<StockTransferViewModel>? stockTransferDtos { get; set; }
  public int TotalItems { get; set; }
}
