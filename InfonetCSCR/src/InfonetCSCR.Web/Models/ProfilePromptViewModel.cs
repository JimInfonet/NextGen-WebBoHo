namespace InfonetCSCR.Web.Models;

public class ProfilePromptViewModel
{
  public string? ProfileID { get; set; }
  public char? Type { get; set; }
  public short? PromptID { get; set; }
  public short? PromptSeq { get; set; }
  public string? OptDataID { get; set; }
  public string? PromptMessage { get; set; }
}
