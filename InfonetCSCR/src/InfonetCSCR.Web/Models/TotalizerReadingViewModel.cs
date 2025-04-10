namespace InfonetCSCR.Web.Models;

public class TotalizerReadingViewModel
{
  public DateTime Date { get; set; }
  public int Grade { get; set; }
  public int PumpID { get; set; }
  public double Volume { get; set; }
  public double Dollars { get; set; }
  public string FullName { get; set; }
}
