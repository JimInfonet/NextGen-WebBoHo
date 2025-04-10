namespace InfonetCSCR.Web.Models;

public class FleetCardFuelViewModel
{
  public DateTime TransactionDate { get; set; }
  public int SaleNo { get; set; }
  public decimal Amount { get; set; }
  public string? CardType { get; set; }
  public string? SaleType { get; set; }
  public string? CardName { get; set; }
  public string? CardNumber { get; set; }
  public string? StockCode { get; set; }
  public decimal Quantity { get; set; }
  public decimal Discount { get; set; }
  public DateTime TransactionTime { get; set; }
}
