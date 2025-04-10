namespace InfonetCSCR.Core.Models;

public class CardProfileTimeLimit
{
  public string? ProfileID { get; set; }
  public int? DayOfWeek { get; set; }
  public bool? AllowPurchase { get; set; }
  public bool? TimeRestriction { get; set; }
  public DateTime? StartTime { get; set; }
  public DateTime? EndTime { get; set; }
}
