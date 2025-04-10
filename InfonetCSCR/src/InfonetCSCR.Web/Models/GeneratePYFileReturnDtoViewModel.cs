namespace InfonetCSCR.Web.Models;

public class GeneratePYFileReturnDtoViewModel
{
  public bool IsHourly { get; set; }
  public string StoreCode { get; set; }
  public List<TimeSheetHeaderViewModel> TimesheetHeaders { get; set; }
  public IEnumerable<ShortageDtoViewModel>[] Shortages { get; set; }
  public IEnumerable<TimesheetDetailDtoViewModel>[] TimesheetDetails { get; set; }
  public double[] CashShortages { get; set; }
  public double[] PriorShortages { get; set; }
  public double[] TotalHours { get; set; }
  public IEnumerable<double>[] Overtimes { get; set; }
  public double[] StatutoryHours { get; set; }
  public double[] TotalCommissions { get; set; }
}
