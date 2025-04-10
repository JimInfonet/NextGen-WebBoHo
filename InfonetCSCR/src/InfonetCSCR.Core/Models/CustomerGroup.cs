namespace InfonetCSCR.Core.Models;

public class CustomerGroup
{
  public string GroupID { get; set; } = default!;
  public string? GroupName { get; set; }
  public string? DiscountName { get; set; }
  public string? DiscountType { get; set; }
  public decimal DiscountRate { get; set; } = default!;
  public string? LoyaltyFooter { get; set; }
}
