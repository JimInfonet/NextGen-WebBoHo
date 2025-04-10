using System;

namespace InfonetCSCR.Reporting.Models
{
  public class ShortageDto
  {
    public long? ReportId { get; set; }
    public string? UserCode { get; set; }
    public DateTime? ShiftDate { get; set; }
    public double? Shortage { get; set; }
  }
}