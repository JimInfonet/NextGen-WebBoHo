namespace InfonetCSCR.Core.Dtos;

public class ReclineDto
{
  public string StockCode { get; set; } = default!;
  public bool UpdateInitial { get; set; }
  public decimal? InStock { get; set; }
  public int Purpose { get; set; }
  public int? BatchId { get; set; }
  public decimal? SalesQuantity { get; set; }
  public decimal? QuantityRecieved { get; set; }
  public decimal? Cost { get; set; }
  public decimal? InitialCount { get; set; }
  public string? VendorId { get; set; }
  public string? ActiveVendorCode { get; set; }
  public string? CostType { get; set; }
  public int? CostDec { get; set; }
  public decimal? Quantity { get; set; }
  public decimal? Invoice { get; set; }
  public string? ExceptedTax { get; set; }
  public string? Description { get; set; }
  public int? Sequence { get; set; }
  public bool? FromText { get; set; }
  public bool IsQuantityUpdate { get; set; }
  public DateTime? DateCount { get; set; } = default!;
  public bool IsAllReclines { get; set; } = false;
  public decimal? SysQty { get; set; }
}
