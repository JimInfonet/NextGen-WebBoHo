namespace InfonetCSCR.Web.Models;

public class ShortageDtoViewModel
{
  public int? ShiftRecId { get; set; }
  public int? ReportId { get; set; }
  public string? UserCode { get; set; }
  public DateTime? ShiftDate { get; set; }
  public double? Shortage { get; set; }
  public int? Shift { get; set; }
}
