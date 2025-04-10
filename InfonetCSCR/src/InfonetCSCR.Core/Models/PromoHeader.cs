namespace InfonetCSCR.Core.Models;

public class PromoHeader
{
  public string PromoId { get; set; }
  public string? Description { get; set; }
  public decimal? Price { get; set; }
  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set; }
  public string? DiscType { get; set; }
  public int? Items { get; set; }
  public int? Day { get; set; }
  public string? PrType { get; set; }
  public string? PromoDetails { get; set; }
  public IEnumerable<PromoDetail>? PromoDetailViewModels { get; set; }
}
