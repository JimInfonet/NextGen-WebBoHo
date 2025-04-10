namespace InfonetCSCR.Web.Models;

public class CardDetailsViewModel
{
  public DateTime TransactionDate { get; set; }
  public int SaleNo { get; set; }
  public decimal Amount { get; set; }
  public string? TerminalID { get; set; }
  public string? TransactionType { get; set; }
  public string? CardType { get; set; }
  public string? CardName { get; set; }
  public string? CardNumber { get; set; }
  public string? ExpiryDate { get; set; }
  public string? ApprovalCode { get; set; }
  public DateTime TransactionTime { get; set; }
}
