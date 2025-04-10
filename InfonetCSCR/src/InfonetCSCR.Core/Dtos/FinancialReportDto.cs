namespace InfonetCSCR.Core.Dtos
{
  public class FinancialReportDto
  {
    public string? Tender { get; set; }
    public int? TotQty { get; set; }
    public decimal? Amount { get; set; }
    public string? Action { get; set; }
  }
}
