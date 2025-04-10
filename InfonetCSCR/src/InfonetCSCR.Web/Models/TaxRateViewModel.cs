namespace InfonetCSCR.Web.Models;

public class TaxRateViewModel
{
  public string Name { get; set; } = default!;
  public string Code { get; set; } = default!;
  public decimal Rate { get; set; }
  public bool? Included { get; set; }
  public string? Description { get; set; }
  public decimal? TaxRebate { get; set; }
}
