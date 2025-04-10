namespace InfonetCSCR.Core.Models;

public class StockItemHistory
{
  public int SaleNo { get; set; }
  public DateTime? ShiftDate { get; set; }
  public string? TransactionType { get; set; }
  public string? StockCode { get; set; }
  public double? Quantity { get; set; }
  public double? Cost { get; set; }
  public double? TotalCost { get; set; }
  public string? MonthName { get; set; }
}