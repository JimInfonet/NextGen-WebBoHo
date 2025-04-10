namespace InfonetCSCR.Core.Models;

public class UserDetails
{
  public string Code { get; set; }
  public string? Name { get; set; }
  public string? Password { get; set; }
  public int? Id { get; set; }
  public string? Type { get; set; }
  public DateTime? DateOfBirth { get; set; }
  public UserGroup? UserGroup { get; set; }
}
