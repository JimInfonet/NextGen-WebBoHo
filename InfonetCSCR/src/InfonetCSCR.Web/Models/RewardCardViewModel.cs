namespace InfonetCSCR.Web.Models;

public class RewardCardViewModel
{
  public string CustomerCardNum { get; set; } = default!;
  [RegularExpression(@"^[+-]?\d+$", ErrorMessage = "theValueMustBeANumber")]
  [MaxLength(30)]
  public string PointCardNum { get; set; } = default!;
  [RegularExpression(@"^[+-]?\d+$", ErrorMessage = "theValueMustBeANumber")]
  [MaxLength(10)]
  public string? PhoneNum { get; set; }
  public bool? IsEditMode { get; set; }
}
