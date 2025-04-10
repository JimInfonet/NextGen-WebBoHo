namespace InfonetCSCR.Web.Models;

public class FleetCardSaleViewModel
{
  public DateTime? TransactionDate { get; set; }
  public int? SaleNo { get; set; }
  public int? TillNumber { get; set; }
  public string? CardType { get; set; }
  public string? CardName { get; set; }
  public string? CardNumber { get; set; }
  public DateTime? TransactionTime { get; set; }
  public decimal TendAmount { get; set; }
  public decimal SaleAmount { get; set; }
  public string? StockCode { get; set; }
  public string? Description { get; set; }
  public string? Type { get; set; }
  public decimal Quantity { get; set; }
  public decimal Price { get; set; }
  public decimal Discount { get; set; }
  public decimal Amount { get; set; }
  public int? LineNumber { get; set; }
  public decimal LineTax { get; set; }
  public decimal SaleKitTax { get; set; }
  public decimal ChargeTax { get; set; }
  public decimal SaleCharge { get; set; }
  public decimal SaleKitCharge { get; set; }
}
