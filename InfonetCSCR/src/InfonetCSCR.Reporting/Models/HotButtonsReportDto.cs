using System;

namespace InfonetCSCR.Reporting.Models
{
  public class HotButtonsReportDto
  {
    public int BatchId { get; set; } = default!;
    public int Button { get; set; } = default!;
    public string ProductId { get; set; } = default!;
    public string? Description { get; set; }
    public string? Caption1 { get; set; }
    public string? Caption2 { get; set; }
    public string StoreId { get; set; } = default!;
    public DateTime? TransDate { get; set; }
    public string? TransUser { get; set; }
    public string? TransFlag { get; set; }
    public string? TransType { get; set; }
    public string? StoreDesc { get; set; }
    public string? Name { get; set; }
  }
}
