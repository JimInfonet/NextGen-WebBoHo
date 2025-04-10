namespace InfonetCSCR.Core.Records;

public class FuelPriceChangeTransferRecord
{
  public string? StoreId { get; set; }
  public int? GradeId { get; set; }
  public int? TierId { get; set; }
  public int? ServiceLevelId { get; set; }
  public Decimal? CashPrice { get; set; }
  public Decimal? CreditPrice { get; set; }
  public string? EmployeeId { get; set; }
  public int? ReportId { get; set; }
  public DateTime? ReportDateTime { get; set; }
  public Decimal? TaxFreePrice { get; set; }
  public Decimal? TaxFreeCreditPrice { get; set; }
}
