namespace InfonetCSCR.Core.Dtos;

public class PriceChangeTransferMinimalDto
{
  public int BatchId { get; set; }
  public string ProductId { get; set; }
  public string? Description { get; set; }
  public string? PriceType { get; set; }
}
