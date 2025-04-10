namespace InfonetCSCR.Core.Dtos;

public class PriceChangeReportDto
{
  public int BatchId { get; set; } = default!;
  public string ProductId { get; set; } = default!;
  public string? Description { get; set; }
  public string? PriceType { get; set; }
  public string StoreId { get; set; } = default!;
  public string? VendorId { get; set; }
  public Int16? OrderNo { get; set; }
  public DateTime? TransDate { get; set; }
  public string? PrUnits { get; set; }
  public Single FromQty { get; set; } = default!;
  public Single? PrToQty { get; set; }
  public Decimal? PDPrice { get; set; }
  public DateTime? PrFrom { get; set; }
  public DateTime? PrTo { get; set; }
  public Single? Margin { get; set; }
  public string Name { get; set; } = default!;
  public string? StoreDesc { get; set; }
  public Int16 PriceNumber { get; set; } = default!;
  public Decimal SPPrice { get; set; } = default!;
}
