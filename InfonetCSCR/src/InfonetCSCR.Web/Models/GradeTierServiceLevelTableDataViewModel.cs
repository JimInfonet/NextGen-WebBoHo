namespace InfonetCSCR.Web.Models;

public class GradeTierServiceLevelTableDataViewModel
{
  public int? GradeId { get; set; }
  public string? FullName { get; set; }
  public string? StockCode { get; set; }
  public string? FuelType { get; set; }
  public int? TierId { get; set; }
  public string? TierDescription { get; set; }
  public int? ServiceLevelId { get; set; }
  public string? ServiceLevelDescription { get; set; }
  public Decimal? CashPrice { get; set; }
  public Decimal? CreditPrice { get; set; }
  public bool? LoadRow { get; set; }
}
