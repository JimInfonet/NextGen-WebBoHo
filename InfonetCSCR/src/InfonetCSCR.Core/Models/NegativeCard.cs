namespace InfonetCSCR.Core.Models;

public class NegativeCard
{
  public string CardNumber { get; set; } = default!;
  public int MessageCode { get; set; }
  public DateTime? ExpiryDate { get; set; }
  public DateTime? EntryDate { get; set; }
}
