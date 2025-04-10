namespace InfonetCSCR.Core.Models;

public class PriceDefinition
{
  public int BatchId { get; set; }
  public string ProductId { get; set; }
  public string? PR_Type { get; set; }
  public string StoreId { get; set; }
  public string? PR_Units { get; set; }
  public decimal PR_From_Qty { get; set; }
  public decimal? PR_To_Qty { get; set; }
  public short PriceNumber { get; set; }
  public decimal? Price { get; set; }
  public DateTime? PR_FROM { get; set; }
  public DateTime? PR_TO { get; set; }
  public DateTime? Trans_Date { get; set; }
  public string? Trans_User { get; set; }
  public string? Trans_Flag { get; set; }
  public short OrderNo { get; set; }
  public string VendorId { get; set; }
  public decimal? Margin { get; set; }
  public decimal? CurrentPrice { get; set; }
}
