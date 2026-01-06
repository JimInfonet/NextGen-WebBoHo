namespace InfonetCSCR.Core.Models;

public class Dept
{
  public string DEPT { get; set; } = default!;
  public string DEPT_NAME { get; set; } = default!;
  public int? EOD_Detail { get; set; }
  public int? EodGroup { get; set; }
  public int? Cnt_Detail { get; set; }
}
