using System;

namespace InfonetCSCR.Reporting.Models
{
  public class FuelPriceChangeReportDto
  {
    public string? StoreId { get; set; }
    public string? FullName { get; set; }
    public string? ShortName { get; set; }
    public string? TierDescription { get; set; }
    public string? ServiceLevelDescription { get; set; }
    public Decimal? CashPrice { get; set; }
    public Decimal? CreditPrice { get; set; }
    public DateTime? DateTime { get; set; }
  }
}
