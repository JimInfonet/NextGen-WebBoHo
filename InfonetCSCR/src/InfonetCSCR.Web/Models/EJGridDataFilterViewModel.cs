namespace InfonetCSCR.Web.Models;

public class EJGridDataFilterViewModel
{
  public int TotalItems { get; set; }
  public IEnumerable<ElectronicJournalReportViewModel> ElectronicJournalGridData { get; set; }
}
