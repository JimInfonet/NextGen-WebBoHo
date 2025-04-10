namespace InfonetCSCR.Web.Models;

public class CardProfileTimeLimitViewModel
{
  public string? ProfileID { get; set; }
  public int? DayOfWeek { get; set; }
  public bool? AllowPurchase { get; set; }
  public bool? TimeRestriction { get; set; }
  public string? StartTime { get; set; }
  public string? EndTime { get; set; }
}
