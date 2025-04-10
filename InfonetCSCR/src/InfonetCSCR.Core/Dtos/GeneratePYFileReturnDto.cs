namespace InfonetCSCR.Core.Dtos;

public class GeneratePYFileReturnDto
{
  public bool IsHourly { get; set; }
  public string StoreCode { get; set; }
  public List<TimeSheetHeader> TimesheetHeaders { get; set; }
  public IEnumerable<ShortageDto>[] Shortages { get; set; }
  public IEnumerable<TimesheetDetailDto>[] TimesheetDetails { get; set; }
  public double[] CashShortages { get; set; }
  public double[] PriorShortages { get; set; }
  public double[] TotalHours { get; set; }
  public IEnumerable<double>[] Overtimes { get; set; }
  public double[] StatutoryHours { get; set; }
  public double[] TotalCommissions { get; set; }
}
