namespace InfonetCSCR.Web.Models;

public class StockAvailabilityByDayViewModel
{
  public int DayOfWeek { get; set; }
  public string? StockCode { get; set; }
  public bool IsAvailable { get; set; }
}
