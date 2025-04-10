namespace InfonetCSCR.Core.Models;

public class TaxExemptCategory
{
  public int Id { get; set; } = default!;
  public string Name { get; set; } = default!;
  public int TaxFreeLimit { get; set; } = default!;
  public bool LimitProductSale { get; set; } = default!;
}
