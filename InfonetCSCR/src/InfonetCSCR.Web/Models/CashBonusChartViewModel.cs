namespace InfonetCSCR.Web.Models;

public class CashBonusChartViewModel
{
  public string IncrementalType { get; set; }
  public int IncrementalQty { get; set; }
  public decimal BonusRate { get; set; }
  public string? GroupID { get; set; }
}