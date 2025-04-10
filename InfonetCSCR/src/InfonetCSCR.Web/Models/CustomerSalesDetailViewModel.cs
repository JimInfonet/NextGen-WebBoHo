namespace InfonetCSCR.Web.Models;

public class CustomerSalesDetailViewModel
{
  public int SaleNumber { get; set; }
  public string? ClientCode { get; set; }
  public string? ClientName { get; set; }
  public string? CardNumber { get; set; }
  public DateTime? SaleDate { get; set; }
  public DateTime? SaleTime { get; set; }
  public decimal Overpayment { get; set; }
  public string? StockCode { get; set; }
  public string? StockName { get; set; }
  public int LineNumber { get; set; }
  public int TillNumber { get; set; }
  public decimal Quantity { get; set; }
  public decimal Amount { get; set; }
  public decimal Discount { get; set; }
  public decimal DiscountAdjustment { get; set; }
  public string? SaleType { get; set; }
  public decimal LineTaxAmount { get; set; }
  public decimal ChargeTaxAmount { get; set; }
  public decimal KitTaxAmount { get; set; }
  public decimal SaleAmount { get; set; }
  public decimal KitAmount { get; set; }
}
