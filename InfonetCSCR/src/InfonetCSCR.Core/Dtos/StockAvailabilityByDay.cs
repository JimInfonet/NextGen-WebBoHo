namespace InfonetCSCR.Core.Dtos;

public class StockAvailabilityByDay
{
  public int DayOfWeek { get; set; }
  public string? StockCode { get; set; }
  public bool IsAvailable { get; set; }
}
