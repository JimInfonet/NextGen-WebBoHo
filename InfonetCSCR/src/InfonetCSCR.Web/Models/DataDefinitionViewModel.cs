namespace InfonetCSCR.Web.Models;

public class DataDefinitionViewModel
{
  public string ProductID { get; set; } = default!;
  public string? Description { get; set; }
  public string StoreId { get; set; } = default!;
  public string DefType { get; set; } = default!;
  public string DefCode { get; set; } = default!;
  public bool? Transfer { get; set; }
  public DateTime? TransferDate { get; set; }
  public string? TransferUser { get; set; }
  public string? VendorId { get; set; }
  public string? Trans_Flag { get; set; }
}
