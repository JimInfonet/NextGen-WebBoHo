namespace InfonetCSCR.Web.Models;

public class CostChangeTransferViewModel
{
  public int BatchId { get; set; } = default!;
  public string Product { get; set; } = default!;
  public string? Description { get; set; }
}
