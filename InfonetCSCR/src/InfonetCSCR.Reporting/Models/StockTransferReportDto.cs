using System;

namespace InfonetCSCR.Reporting.Models
{
  public class StockTransferReportDto
  {
    public int BatchId { get; set; } = default!;
    public string ProductId { get; set; } = default!;
    public string? Description { get; set; }
    public string StoreId { get; set; } = default!;
    public DateTime? TransDate { get; set; }
    public string? TransUser { get; set; }
    public string? TransFlag { get; set; }
    public string? TransType { get; set; }
    public string? UserName { get; set; }
  }
}
