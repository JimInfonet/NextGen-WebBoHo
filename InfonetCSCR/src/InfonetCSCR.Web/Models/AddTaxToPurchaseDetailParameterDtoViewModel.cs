namespace InfonetCSCR.Web.Models;

public class AddTaxToPurchaseDetailParameterDtoViewModel
{
  public string? ExptTaxPolicy { get; set; }
  public IEnumerable<PurchaseDetailViewModel>? PurchaseDetails { get; set; }
  public int? CostDec { get; set; }
  public DateTime? PurchaseDate { get; set; }
}
