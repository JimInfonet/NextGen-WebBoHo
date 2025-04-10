namespace InfonetCSCR.Core.Models;

public class GradeTierServiceLevelTableData
{
  public string? StoreId { get; set; }
  public int? GradeId { get; set; }
  public string? FullName { get; set; }
  public string? ShortName { get; set; }
  public string? StockCode { get; set; }
  public string? FuelType { get; set; }
  public int? TierId { get; set; }
  public string? TierDescription { get; set; }
  public int? ServiceLevelId { get; set; }
  public string? ServiceLevelDescription { get; set; }
  public double? CashPrice { get; set; }
  public double? CreditPrice { get; set; }
  public bool? LoadRow { get; set; }
  public DateTime? DateTime { get; set; }
}
