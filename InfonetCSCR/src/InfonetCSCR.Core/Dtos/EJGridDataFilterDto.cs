namespace InfonetCSCR.Core.Dtos;

public class EJGridDataFilterDto
{
  public int TotalItems { get; set; }
  public IEnumerable<ElectronicJournalReportData> ElectronicJournalGridData { get; set; }
}
