namespace InfonetCSCR.Core.Models;

public class ArCustomer
{
  public string Code { get; set; } = default!;
  public string? StmtEmail { get; set; }
  public DateTime? DateOpen { get; set; }
  public DateTime? LastStmtDate { get; set; }
  public DateTime? LastSaleDate { get; set; }
  public DateTime? HighSaleDate { get; set; }
  public DateTime? LastPaymentDate { get; set; }
  public decimal? HighStmtBalance { get; set; }
  public decimal? LastStmtBalance { get; set; }
  public decimal ? LastPaymentBalance { get; set; }
}
