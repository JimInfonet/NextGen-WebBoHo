namespace InfonetCSCR.Core.Models;

public class Category
{
  public int? Id { get; set; }
  public string? Name { get; set; }
  public int? TaxFreeLimit { get; set; }
  public Boolean LimitProductSale { get; set; } = default!;
}
