namespace InfonetCSCR.Core.Models;

public class CashBonusChart
{
  public string IncrementalType { get; set; }
  public int IncrementalQty { get; set; }
  public decimal BonusRate { get; set; }
  public string? GroupID { get; set; }
}
