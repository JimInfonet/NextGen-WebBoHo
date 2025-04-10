namespace InfonetCSCR.Core.Dtos;

public class AddTaxToPurchaseDetailParameterDto
{
  public string? ExptTaxPolicy { get; set; }
  public IEnumerable<PurchaseDetail>? PurchaseDetails { get; set; }
  public int? CostDec { get; set; }
  public DateTime? PurchaseDate { get; set; }
}
