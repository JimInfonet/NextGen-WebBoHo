namespace InfonetCSCR.Core.Models;
public class CardRestrictionProfile
{
  public string ProfileID { get; set; } = default!;
  public string? ProfileName { get; set; }
  public decimal? SngleTransaction { get; set; }
  public decimal? DailyTransaction { get; set; }
  public decimal? MonthlyTransaction { get; set; }
  public int? TransactionsPerDay { get; set; }
  public bool? AskForPO { get; set; }
  public bool? PromptForFuel { get; set; }
  public bool? LimitTimeofPurchase { get; set; }
  public bool? RestrictProducts { get; set; }
}
