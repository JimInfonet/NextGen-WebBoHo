namespace InfonetCSCR.Core.Dtos;

public class CustomerFormDto : Customer
{
  public bool? IsSyncCustInfo { get; set; }
  public string? StmtEmail { get; set; }
  public DateTime? DateOpen { get; set; }
  public decimal? LastStmtBalance { get; set; }
  public DateTime? LastStmtDate { get; set; }
  public DateTime? LastSaleDate { get; set; }
  public decimal? HighStmtBalance { get; set; }
  public DateTime? HighSaleDate { get; set; }
  public decimal? LastPaymentBalance { get; set; }
  public DateTime? LastPaymentDate { get; set; }
}
