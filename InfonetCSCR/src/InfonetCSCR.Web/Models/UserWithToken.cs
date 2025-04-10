namespace InfonetCSCR.Web.Models;

public class UserWithToken
{
  public UserDetailsViewModel UserDetails { get; set; }
  public TokenModel Token { get; set; }
}
