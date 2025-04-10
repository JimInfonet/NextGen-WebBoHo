namespace InfonetCSCR.Core.Dtos;

public class CommissionProduct
{
  public string? StockCode { get; set; }
  public DateTime? ShiftDate { get; set; }
  public double? Quantity { get; set; }
  public double? Amount { get; set; }
  public string? Dept { get; set; }
  public string? SubDept { get; set; }
  public string? SubDetail { get; set; }
}
