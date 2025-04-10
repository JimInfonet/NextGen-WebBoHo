using System;

namespace InfonetCSCR.Reporting.Models
{
  public class PromotionalDiscountTransferReportDto
  {
    public int BatchId { get; set; } = default!;
    public string PromoId { get; set; } = default!;
    public string? Description { get; set; }
    public string StoreId { get; set; } = default!;
    public DateTime? TransDate { get; set; }
    public string? TransUser { get; set; }
    public string? TransType { get; set; }
    public decimal? Price { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? DiscType { get; set; }
    public byte? Items { get; set; }
    public byte? Day { get; set; }
    public string? StoreDesc { get; set; }
    public string? UserName { get; set; }
  }
}
