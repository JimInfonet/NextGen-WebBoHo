namespace InfonetCSCR.Core.Models;
public class FuelPriceHistory
{
  public DateTime? DateTime { get; set; }
  public double? CashPrice { get; set; }
  public double? CreditPrice { get; set; }
  public int? ReportID { get; set; }
  public int? GradeID { get; set; }
  public int? TierID { get; set; }
  public int? LevelID { get; set; }
  public string? StoreID { get; set; }
}
