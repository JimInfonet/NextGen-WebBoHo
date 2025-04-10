namespace InfonetCSCR.Core.Models;
public class AffdaSaleDetail
{
  public int SaleNo { get; set; }
  public string? Description { get; set; }
  public decimal Quantity { get; set; }
  public decimal Price { get; set; }
  public decimal Rebate { get; set; }
  public DateTime? TransactionDate { get; set; }
  public DateTime? TransactionTime { get; set; }
  public string? StockCode { get; set; }
  public string? CustomerCode { get; set; }
  public string? CustomerName { get; set; }
}
