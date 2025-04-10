namespace InfonetCSCR.Core.Dtos;

public class HotButtonsTransferDto
{
  public int BatchId { get; set; } = default!;
  public int Button { get; set; } = default!;
  public string ProductId { get; set; } = default!;
  public string? Description { get; set; }
  public string? Caption1 { get; set; }
  public string? Caption2 { get; set; }
  public string StoreID { get; set; } = default!;
  public string? TransType { get; set; }
}