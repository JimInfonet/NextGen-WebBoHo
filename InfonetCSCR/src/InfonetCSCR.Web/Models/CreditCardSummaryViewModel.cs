namespace InfonetCSCR.Web.Models;

public class CreditCardSummaryViewModel
{
  public string? Description { get; set; }
  public string? CardType { get; set; }
  public int Transactions { get; set; }
  public decimal TotalAmount { get; set; }
  public int Till { get; set; }
  public DateTime ShiftDate { get; set; }
}