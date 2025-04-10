namespace InfonetCSCR.Core.Models;

public class CustomerCard
{
  public bool? AllowRedemption { get; set; }
  public string CardName { get; set; } = default!;
  public string CardNumber { get; set; } = default!;
  public string CardStatus { get; set; } = default!;
  public string? Code { get; set; }
  public decimal CreditLimit { get; set; } = default!;
  public DateTime? ExpDate { get; set; }
  public decimal Balance { get; set; } = default!;
  public string PIN { get; set; } = default!;
  public string? ProfileID { get; set; }
  public string? TECardNumber { get; set; }
  public string? Sequence { get; set; }
}
