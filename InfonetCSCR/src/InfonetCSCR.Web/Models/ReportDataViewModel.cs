namespace InfonetCSCR.Web.Models;

public class ReportDataViewModel
{
  public DateTime? SaleDate { get; set; }
  public int? SaleNo { get; set; } = default!;
  public string? TreatyNo { get; set; }
  public string? Product { get; set; }
  public string? Description { get; set; }
  public Decimal? Price { get; set; }
  public Decimal? TaxExemptedPrice { get; set; }
  public Decimal? Quantity { get; set; }
  public Decimal? TotalExemptedTax { get; set; }
  public Decimal? SaleTotal { get; set; }
  public Decimal? PhoneNo { get; set; }
  public string? Reason { get; set; } = default!;
}
