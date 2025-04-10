namespace InfonetCSCR.Web.Models;

public class NegativeCardViewModel
{
  public string CardNumber { get; set; } = default!;
  public int MessageCode { get; set; }
  public DateTime? ExpiryDate { get; set; }
  public DateTime? EntryDate { get; set; }
}
