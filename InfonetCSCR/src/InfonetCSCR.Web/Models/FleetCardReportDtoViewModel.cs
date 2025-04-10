namespace InfonetCSCR.Web.Models;

public class FleetCardReportDtoViewModel
{
  public string? CardNumber { get; set; }
  public int SaleNo { get; set; } = default!;
  public DateTime? SaleDate { get; set; }
  public Decimal? Quantity { get; set; }
  public Decimal? Price { get; set; }
  public string? Descript { get; set; }
  public Decimal? SaleLineAmount { get; set; }
  public Decimal? Discount { get; set; }
  public Decimal? NetAmount { get; set; }
  public string Misc { get; set; } = default!;
  public Decimal? SaleHeadAmount { get; set; }
  public Decimal? FinalAmount { get; set; }
}
