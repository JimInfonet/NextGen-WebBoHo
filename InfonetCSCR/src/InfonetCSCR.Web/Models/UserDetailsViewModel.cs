namespace InfonetCSCR.Web.Models;

public class UserDetailsViewModel
{
  [Required(ErrorMessage = "codeIsRequired")]
  [MaxLength(10)]
  public string Code { get; set; }
  [Required(ErrorMessage = "nameIsRequired")]
  [MaxLength(25)]
  public string? Name { get; set; }
  [Required(ErrorMessage = "passwordIsRequired")]
  [MaxLength(31)]
  public string? Password { get; set; }
  public int? Id { get; set; }
  [Required(ErrorMessage = "typeIsRequired")]
  public string? Type { get; set; }
  [Required(ErrorMessage = "DOBIsRequired")]
  public DateTime? DateOfBirth { get; set; }
  public UserGroupViewModel? UserGroup { get; set; }
}
