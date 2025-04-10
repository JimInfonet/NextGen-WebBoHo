namespace InfonetCSCR.Core.Models;

public class PriceDefinitionBatch
{
  public int BatchId { get; set; }
  public string[] Products { get; set; }
  public string[] StoreIds { get; set; }
  public string? PR_Type { get; set; }
  public double PR_From_Qty { get; set; }
  public double? PR_To_Qty { get; set; }
  public DateTime? PR_FROM { get; set; }
  public DateTime? PR_TO { get; set; }
  public short OrderNo { get; set; }
}
