namespace InfonetCSCR.Web.Models;

public class FunctionListViewModel
{
  public int ButtonType { get; set; } = default!;
  public int FunctionID { get; set; } = default!;
  public string FunctionName { get; set; } = default!;
  public string? PictureName { get; set; }
  public string? DisplayName { get; set; }
  public string? TenderName { get; set; }
}
