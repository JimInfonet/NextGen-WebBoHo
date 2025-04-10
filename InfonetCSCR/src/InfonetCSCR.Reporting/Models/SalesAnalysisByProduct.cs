using System;

namespace InfonetCSCR.Reporting.Models
{
  public class SalesAnalysisByProduct
  {
    public string? TransactionType { get; set; }
    public DateTime? ShiftDate { get; set; }
    public int SaleNo { get; set; }
    public string? Client { get; set; }
    public double? Quantity { get; set; }
    public double? Price { get; set; }
    public double? Cost { get; set; }
    public double? TotalAmount { get; set; }
    public double? TotalCost { get; set; }
    public double? GrossProfit { get; set; }
    public double? Margin { get; set; }
    public DateTime? SaleDate { get; set; }
    public string? MonthName { get; set; }
  }
}