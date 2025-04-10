namespace InfonetCSCR.Web.Models;

public class FunctionButtonViewModel
{
  public int ButtonID { get; set; } = default!;
  public int FunctionID { get; set; } = default!;
  public int PictureID { get; set; } = default!;
  public int? ButtonType { get; set; }
}
