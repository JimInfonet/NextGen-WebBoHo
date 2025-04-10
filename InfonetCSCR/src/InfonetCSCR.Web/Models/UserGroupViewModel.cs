namespace InfonetCSCR.Web.Models;


public class UserGroupViewModel
{
  [Required(ErrorMessage = "groupNameIsRequired")]
  [MaxLength(10)]
  public string Code { get; set; }
  [Required(ErrorMessage = "groupDescriptionIsRequired")]
  [MaxLength(30)]
  public string? Name { get; set; }
  [Required(ErrorMessage = "securityLevelIsRequired")]
  public int? SecurityLevel { get; set; }
}
