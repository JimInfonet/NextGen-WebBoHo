namespace InfonetCSCR.Core.Dtos;

public class StockTakesAndReceiveDto
{
  public short? RecNum { get; set; }
  public string? Invoice { get; set; }
  public string StockCode { get; set; } = default!;
  public string? Description { get; set; }
  public float? Quantity { get; set; }
  public float? Cost { get; set; }
  public decimal? Amount { get; set; }
  public bool? RecUpdate { get; set; }
  public int? Sequence { get; set; }
  public float? SystemQuantity { get; set; }
  public float? InitialCount { get; set; }
  public int Purpose { get; set; }
}