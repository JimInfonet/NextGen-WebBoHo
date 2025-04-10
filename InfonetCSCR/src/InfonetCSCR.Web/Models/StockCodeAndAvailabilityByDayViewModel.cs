namespace InfonetCSCR.Web.Models;

public class StockCodeAndAvailabilityByDayViewModel
{
  public List<StockAvailabilityByDayViewModel> AvailabilityByDays { get; set; }
  public string StockCode { get; set; }
}
