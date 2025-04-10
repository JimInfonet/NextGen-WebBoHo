namespace InfonetCSCR.Core.Dtos;

public class TransferPOFileDto
{
  public List<PurchaseHeader>? PurchaseHeaders { get; set; }
  public List<PurchaseDetail>? PurchaseDetails { get; set; }
  public string? StoreCode { get; set; }
  public double? CalculatedDiscount { get; set; }
  public string? HeaderFileName { get; set; }
  public string? DetailFileName { get; set; }
  public string? DetailFileContent { get; set; }
  public string? HeaderFileContent { get; set; }
}
