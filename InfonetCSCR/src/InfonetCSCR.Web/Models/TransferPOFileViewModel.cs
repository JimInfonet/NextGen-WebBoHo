namespace InfonetCSCR.Web.Models;

public class TransferPOFileViewModel
{
  public List<PurchaseHeaderViewModel>? PurchaseHeaders { get; set; }
  public List<PurchaseDetailViewModel>? PurchaseDetails { get; set; }
  public string? StoreCode { get; set; }
  public double? CalculatedDiscount { get; set; }
  public string? HeaderFileName { get; set; }
  public string? DetailFileName { get; set; }
  public string? DetailFileContent { get; set; }
  public string? HeaderFileContent { get; set; }
}
