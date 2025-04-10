namespace InfonetCSCR.Core.Models;

public class TimeSheetHeader
{
  public int ReportId { get; set; }
  public string UserCode { get; set; }
  public DateTime PayDate { get; set; }
  public DateTime PeriodStarting { get; set; }
  public DateTime PeriodEnding { get; set; }
  public double UniformPurchases { get; set; }
  public string Status { get; set; }
  public bool SplitShift { get; set; }
  public bool? TransferFlag { get; set; }
  public byte[]? ManagerSign { get; set; }
  public byte[]? EmployeeSign { get; set; }
}
