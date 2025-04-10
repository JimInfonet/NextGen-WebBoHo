namespace InfonetCSCR.Core.Dtos;

public class POFileDto
{
  public PurchaseHeader? PurchaseHeader { get; set; }
  public IEnumerable<PurchaseDetail>? PurchaseDetails { get; set; }
  public string? StoreCode { get; set; }
  public double? CalculatedDiscount { get; set; }
  public string? HeaderFileName { get; set; }
  public string? DetailFileName { get; set; }
  public string? DetailFileContent { get; set; }
  public string? HeaderFileContent { get; set; }
}
