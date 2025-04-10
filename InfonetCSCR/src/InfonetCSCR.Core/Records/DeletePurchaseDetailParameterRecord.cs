namespace InfonetCSCR.Core.Records;

public record DeletePurchaseDetailParameterRecord
{
  public string? PurchaseNo { get; set; }
  public string? PartNumber { get; set; }
  public string? LineNumber { get; set; }
}
