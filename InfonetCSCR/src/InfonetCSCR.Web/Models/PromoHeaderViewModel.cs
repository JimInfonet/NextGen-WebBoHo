namespace InfonetCSCR.Web.Models;

public class PromoHeaderViewModel
{
  [Range(1, 999, ErrorMessage = "promotionIdRangeError")]
  [Required(ErrorMessage = "thisFieldIsRequired")]
  public string PromoId { get; set; }
  [MaxLength(30, ErrorMessage = "maxLengthDescriptionError")]
  [Required(ErrorMessage = "thisFieldIsRequired")]
  public string? Description { get; set; }
  [Range(0, 9999.99, ErrorMessage = "maximumRangePriceError")]
  [Required(ErrorMessage = "thisFieldIsRequired")]
  public decimal? Price { get; set; }
  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set; }
  public string? DiscType { get; set; }
  [Range(1, 99, ErrorMessage = "maximumRangeItemError")]
  [Required(ErrorMessage = "thisFieldIsRequired")]
  public int? Items { get; set; }
  public int? Day { get; set; }
  public string? PrType { get; set; }
  public string? PromoDetails { get; set; }
  public IEnumerable<PromoDetailViewModel>? PromoDetailViewModels { get; set; }
}
