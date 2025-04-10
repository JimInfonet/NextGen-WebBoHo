namespace InfonetCSCR.Core.Models;

public class RefreshToken
{
  public string? Token { get; set; }
  public DateTime? ExpiryTime { get; set; }
  public string UserCode { get; set; } = default!;
}
