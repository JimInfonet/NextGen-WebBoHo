namespace InfonetCSCR.Core.Models;

public class DiscountTable
{
  public int CustomerDiscount { get; set; } = default!;
  public int ProductDiscount { get; set; } = default!;
  public double? DiscountPercentage { get; set; }
}
