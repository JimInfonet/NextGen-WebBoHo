namespace InfonetCSCR.Web.Models;

public class PolicyChoiceViewModel
{
  public string? Type { get; set; }
  public string? Description { get; set; }
  public DateTime? StartTime { get; set; }
  public DateTime? EndTime { get; set; }
  public string? Table {  get; set; }
  public string? Field { get; set; }
  public decimal? StartRange { get; set; }
  public decimal? EndRange { get; set; }
  public List<string>? ChoicesList { get; set; }
}
