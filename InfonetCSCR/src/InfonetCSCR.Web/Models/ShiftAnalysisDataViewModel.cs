namespace InfonetCSCR.Web.Models;

public class ShiftAnalysisDataViewModel
{
  public string? EmployeeID { get; set; }
  public int? CashDrawerCount { get; set; }
  public int? SalesCount { get; set; }
  public int? DeletedLinesCount { get; set; }
  public decimal? DeletedLinesTotalAmount { get; set; }
  public decimal? TotalAmount { get; set; }
  public decimal? AvgSaleAmount { get; set; }
  public int? SaleNo { get; set; }
  public DateTime? ShiftDate { get; set; }
  public int? VoidNo { get; set; }
  public string? StockCode { get; set; }
  public string? Description { get; set; }
  public string? Department { get; set; }
  public decimal? Quantity { get; set; }
  public decimal? Amount { get; set; }
  public int? Shift { get; set; }
}
