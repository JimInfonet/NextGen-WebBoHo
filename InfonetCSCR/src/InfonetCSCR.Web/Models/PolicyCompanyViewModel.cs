namespace InfonetCSCR.Web.Models;

public class PolicyCompanyViewModel
{
  public string? Class { get; set; }
  public int Sequence { get; set; }
  public bool Implemented { get; set; }
  public string? Name { get; set; }
  public string? Description { get; set; }
  public string? Levels { get; set; }
  public string? Choices { get; set; }
  public string? Settings { get; set; }
  public string? VarType { get; set; }
  public bool Active { get; set; }
  public bool Used { get; set; }
  public string? Value { get; set; }
  public bool MultipleLevelExists { get; set; }
  public PolicyChoiceViewModel? ParsedChoices { get; set; }
  public List<string>? LevelsList { get; set; }
}
