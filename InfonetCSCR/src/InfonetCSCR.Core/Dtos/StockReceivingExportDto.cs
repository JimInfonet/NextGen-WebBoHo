namespace InfonetCSCR.Core.Dtos;

public class StockReceivingExportDto
{
  public string StockCode { get; set; } = default!;
  public string? Description { get; set; }
  public decimal? SysQuantity { get; set; }
  public decimal? Cost { get; set; }
  public decimal? Price { get; set; }
  public decimal? Assoc { get; set; }
}
