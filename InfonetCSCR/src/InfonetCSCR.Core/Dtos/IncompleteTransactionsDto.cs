namespace InfonetCSCR.Core.Dtos
{
  public class IncompleteTransactionsDto
  {
    public DateTime? SaleDate { get; set; }
    public DateTime? SaleTime { get; set; }
    public int SaleNo { get; set; }
    public string? CardNumber { get; set; }
    public string? ExpiryDate { get; set; }
    public string? CardType { get; set; }
    public decimal CompletionAmount { get; set; }
    public string? ApprovalNum { get; set; }
  }
}
