namespace InfonetCSCR.Core.Models;

public class ARCustomerSales
{
  public int? SaleNo { get; set; }
  public string? SerNum { get; set; }
  public DateTime? SaleDate { get; set; }
  public DateTime? SaleTime { get; set; }
  public decimal? Amount { get; set; }
  public string? CardNumber { get; set; }
  public string? SaleType { get; set; }
  public string? StockCode { get; set; }
  public double? Quantity { get; set; }
  public decimal? Discount { get; set; }
  public string? TendClass { get; set; }
  public string? ClientCode { get; set; }
  public string? ClientName { get; set; }
}
