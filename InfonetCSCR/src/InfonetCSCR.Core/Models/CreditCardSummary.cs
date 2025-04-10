namespace InfonetCSCR.Core.Models;

public class CreditCardSummary
{
  public string? Description { get; set; }
  public string? CardType { get; set; }
  public int Transactions { get; set; }
  public decimal TotalAmount { get; set; }
  public int Till { get; set; }
  public DateTime ShiftDate { get; set; }
}
