namespace InfonetCSCR.Web.Models;

public class CustomerGroupViewModel
{
  [Required(ErrorMessage = "groupIDRequired")]
  [MaxLength(10)]
  public string GroupID { get; set; } = default!;
  public string? GroupName { get; set; }
  public string? DiscountName { get; set; }
  public string? DiscountType { get; set; }
  [Required(ErrorMessage = "discountRateRequired")]
  [Range(-99999, 214748, ErrorMessage = "discountTypeRange")]
  public decimal DiscountRate { get; set; } = default!;
  [Required(ErrorMessage = "loyalityFooterRequired")]
  public string? LoyaltyFooter { get; set; }
}