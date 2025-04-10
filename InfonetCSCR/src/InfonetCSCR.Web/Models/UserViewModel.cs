namespace InfonetCSCR.Web.Models;

public class UserViewModel
{
  [Required(ErrorMessage = "usernameRequired")]
  [MaxLength(50)]
  public string Code { get; set; } = default!;
  [Required(ErrorMessage = "passwordRequired")]
  [MaxLength(50)]
  public string Password { get; set; } = default!;
}
