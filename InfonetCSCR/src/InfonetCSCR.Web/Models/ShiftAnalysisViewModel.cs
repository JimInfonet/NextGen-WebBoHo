namespace InfonetCSCR.Web.Models;

public class ShiftAnalysisViewModel
{
  public IEnumerable<ShiftAnalysisDataViewModel>? ShiftAnalysis { get; set; }
  public IEnumerable<EmployeesAndShiftsViewModel>? EmployeesAndShifts { get; set; }
}
