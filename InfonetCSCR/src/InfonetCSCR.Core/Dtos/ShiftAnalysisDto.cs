namespace InfonetCSCR.Core.Dtos;

public class ShiftAnalysisDto
{
  public IEnumerable<ShiftAnalysis>? ShiftAnalysis { get; set; }
  public IEnumerable<EmployeesAndShifts>? EmployeesAndShifts { get; set; }
}
