namespace InfonetCSCR.Web.Models;

public class FuelPriceChangeTransferRecordViewModel
{
  public string? StoreId { get; set; }
  public int? GradeId { get; set; }
  public int? TierId { get; set; }
  public int? ServiceLevelId { get; set; }
  public Decimal? CashPrice { get; set; }
  public Decimal? CreditPrice { get; set; }
  public string? EmployeeId { get; set; }
  public Decimal? TaxFreePrice { get; set; }
  public Decimal? TaxFreeCreditPrice { get; set; }
}
